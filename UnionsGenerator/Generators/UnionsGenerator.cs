namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System;

using FactoryMapProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableDictionary<Models.TypeSignatureModel, EquatableList<Models.FactoryModel>>>;
using PartialUnionTypeModelsProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableList<Models.PartialUnionTypeModel>>;
using PartialTargetTypeModelUnionProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableDictionary<Models.TypeSignatureModel, (EquatableList<Models.PartialRepresentableTypeModel> representableTypes, Boolean isEqualsRequired, Boolean doesNotImplementToString)>>;
using RelationsProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableDictionary<Models.TypeSignatureModel, EquatableList<Models.RelationModel>>>;
using SettingsMapProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<(Models.SettingsModel fallbackSettings, EquatableDictionary<Models.TypeSignatureModel, Models.SettingsModel> definedSettings)>;
using SourceTextProvider = Microsoft.CodeAnalysis.IncrementalValuesProvider<(String hintName, String source)>;
using System.Linq;
using System.Collections.Immutable;
using RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Generates partial union type implementations.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class UnionsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Gets constant source texts to be generated into target projects.
    /// </summary>
    public static IReadOnlyList<String> ConstantSourceTexts { get; } =
        [
            AliasedUnionTypeBaseAttribute.SourceText,
            RelationAttribute<Object>.SourceText,
            UnionTypeFactoryAttribute.SourceText,
            UnionTypeSettingsAttribute.SourceText,
            ConstantSources.Util
        ];
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var factoryMapProvider = CreateFactoryMapProvider(context);
        var settingsProvider = CreateSettingsProvider(context);
        var relationsProvider = CreateRelationsProvider(context);

        var typeTargetProvider = CreateUnionTypeProvider(context);
        var partialTargetProvider = UnionPartialTypeModelProviders(typeTargetProvider);

        var targetTypeModelsProvider = UnifyPartialModels(partialTargetProvider, settingsProvider, factoryMapProvider, relationsProvider);
        var sourceTextProvider = CreateSourceTextProvider(targetTypeModelsProvider);

        context.RegisterSourceOutput(sourceTextProvider, (ctx, sourceData) =>
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            ctx.AddSource(sourceData.hintName, sourceData.source);
        });
        context.RegisterPostInitializationOutput(RegisterConstantSources);
    }
    #region Transformations
    static SourceTextProvider CreateSourceTextProvider(IncrementalValuesProvider<UnionTypeModel> provider) =>
        provider
        .Select((model, ct) =>
        {
            var containsErrors = DiagnosticsAccumulator.Create(model)
                .DiagnoseNonHiddenSeverities()
                .Receive(Providers.All, ct)
                .ContainsErrors;

            return (model, containsErrors);
        })
        .Where(t => !t.containsErrors)
        .Select((tuple, ct) =>
        {
            ct.ThrowIfCancellationRequested();

            var (model, _) = tuple;

            var resultBuilder = new IndentedStringBuilder(
                IndentedStringBuilderOptions.GeneratedFile with
                {
                    AmbientCancellationToken = ct,
                    GeneratorName = "RhoMicro.CodeAnalysis.UnionsGenerator",
                    PrependWarningDisablePragma = true
                });

            _ = resultBuilder.OpenRegionBlock($"Implementation of {model.Signature.Names.FullGenericName}");

            if(model.Settings.Miscellaneous.HasFlag(MiscellaneousSettings.EmitStructuralRepresentation))
            {
                _ = resultBuilder.OpenRegionBlock("Structural Representation").OpenBlock(new(Indentation: "// "));

                new StructuralRepresentationVisitor(resultBuilder).Visit(model);

                resultBuilder.CloseBlock().CloseBlockCore();
            }

            new SourceTextVisitor(resultBuilder).Visit(model);

            var source = resultBuilder.CloseBlock().ToString();
            var hint = model.Signature.Names.FullIdentifierOrHintName;
            var hintName = $"{hint}.g.cs";

            return (hintName, source);
        });
    #endregion
    #region Combinations
    private static IncrementalValuesProvider<UnionTypeModel> UnifyPartialModels(
        PartialTargetTypeModelUnionProvider partialsProvider,
        SettingsMapProvider settingsProvider,
        FactoryMapProvider factoriesProvider,
        RelationsProvider relationsProvider) =>
        partialsProvider
            .Combine(settingsProvider)
            .Combine(factoriesProvider)
            .Combine(relationsProvider)
            .Select((tuple, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var relations = tuple.Right;
                var factoriesMap = tuple.Left.Right;
                var (fallbackSettings, definedSettings) = tuple.Left.Left.Right;
                var partials = tuple.Left.Left.Left;

                var targetTypeModels = new List<UnionTypeModel>();

                foreach(var (targetTypeSig, data) in partials)
                {
                    var (representableTypes, isEqualsRequired, doesNotImplementToString) = data;
                    ct.ThrowIfCancellationRequested();

                    var settings = getSettings(targetTypeSig);
                    var relationAttributes = getRelations(targetTypeSig);
                    var factories = getFactories(targetTypeSig);

                    var targetModel = UnionTypeModel.Create(
                        targetTypeSig,
                        representableTypes,
                        factories,
                        relationAttributes,
                        settings,
                        isEqualsRequired,
                        doesNotImplementToString,
                        ct);

                    targetTypeModels.Add(targetModel);
                }

                return new EquatableList<UnionTypeModel>(targetTypeModels);

                EquatableList<RelationModel> getRelations(TypeSignatureModel target) =>
                    relations!.TryGetValue(target, out var r) ?
                    r :
                    EquatableList<RelationModel>.Empty;
                SettingsModel getSettings(TypeSignatureModel targetTypeSig) =>
                    definedSettings.TryGetValue(targetTypeSig, out var settings) ?
                    settings :
                    fallbackSettings;
                EquatableList<FactoryModel> getFactories(TypeSignatureModel targetTypeSig) =>
                    factoriesMap!.TryGetValue(targetTypeSig, out var factories)
                    ? factories
                    : EquatableList<FactoryModel>.Empty;
            }).SelectMany((models, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return models;
            });
    private static PartialTargetTypeModelUnionProvider UnionPartialTypeModelProviders(PartialUnionTypeModelsProvider provider) =>
        provider
            .Select((keyValuePairs, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var mutableResult = new Dictionary<TypeSignatureModel, (List<PartialRepresentableTypeModel> representableTypes, HashSet<TypeSignatureModel> mappedRepresentableTypes)>();
                var result = new Dictionary<TypeSignatureModel, (EquatableList<PartialRepresentableTypeModel> representableTypes, Boolean isEqualsRequired, Boolean doesNotImplementToString)>();
                var isEqualsRequiredAccumulator = true;
                var doesNotImplementToStringAccumulator = true;
                foreach(var (key, value, isEqualsRequired, doesNotImplementToString, _) in keyValuePairs)
                {
                    isEqualsRequiredAccumulator &= isEqualsRequired;
                    doesNotImplementToStringAccumulator &= doesNotImplementToString;

                    ct.ThrowIfCancellationRequested();
                    if(!mutableResult.TryGetValue(key, out var data))
                    {
                        data = ([], []);
                        mutableResult.Add(key, data);
                        result.Add(key, (new(data.representableTypes), isEqualsRequiredAccumulator, doesNotImplementToStringAccumulator));
                    } else
                    {
                        result[key] = (result[key].representableTypes, isEqualsRequiredAccumulator, doesNotImplementToStringAccumulator);
                    }

                    if(data.mappedRepresentableTypes.Add(value.Signature))
                        data.representableTypes.Add(value);
                }

                return new EquatableDictionary<TypeSignatureModel, (EquatableList<PartialRepresentableTypeModel> representableTypes, Boolean isEqualsRequired, Boolean doesNotImplementToString)>(result);
            });
    #endregion
    #region Relations FAWMN
    private static RelationsProvider CreateRelationsProvider(IncrementalGeneratorInitializationContext context)
    {
        var result = Enumerable.Range(2, 8)
            .Select(i => $"RhoMicro.CodeAnalysis.RelationAttribute`{i}")
            .Select(createProvider)
            .Aggregate(
                createProvider("RhoMicro.CodeAnalysis.RelationAttribute`1"),
                (leftProvider, rightProvider) =>
                leftProvider.Combine(rightProvider)
                .Select((tuple, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    var (leftList, rightList) = tuple;
                    var result = leftList.Concat(rightList)
                        .GroupBy(t => t.targetSignature)
                        .Select(g => (targetSignature: g.Key, relations: g.SelectMany(t => t.relations).ToEquatableList(ct)))
                        .ToEquatableList(ct);

                    return result;
                }))
            .Select((tuples, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var mutableResult = new Dictionary<TypeSignatureModel, List<RelationModel>>();
                var result = new Dictionary<TypeSignatureModel, EquatableList<RelationModel>>();
                for(var i = 0; i < tuples.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var (targetSignature, relations) = tuples[i];
                    if(!mutableResult.TryGetValue(targetSignature, out var models))
                    {
                        models = [];
                        mutableResult.Add(targetSignature, models);
                        result.Add(targetSignature, new(models));
                    }

                    models.AddRange(relations);
                }

                //Map(UnionTypeSignature, List(Relation))
                return result.AsEquatable();
            });

        return result;

        IncrementalValueProvider<EquatableList<(TypeSignatureModel targetSignature, EquatableList<RelationModel> relations)>> createProvider(String name) =>
            context.SyntaxProvider.ForAttributeWithMetadataName<
            (TypeSignatureModel targetSignature, EquatableList<RelationModel> relations)?>(
            name,
            Qualifications.IsUnionTypeDeclarationSyntax,
            (ctx, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                //check if target of [Relation] is regular struct or class
                if(ctx.TargetSymbol is not INamedTypeSymbol target ||
                   target.TypeKind is not TypeKind.Struct and not TypeKind.Class ||
                   target.IsRecord)
                {
                    return null;
                }

                ct.ThrowIfCancellationRequested();
                var targetSignature = TypeSignatureModel.Create(target, ct);
                var relations = new List<RelationModel>();
                //check if T in [Relation<T>] is named type & is actual union type (has attribute on self or type param) <== consider marker attribute on generated impl?
                for(var i = 0; i < ctx.Attributes.Length; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    if(ctx.Attributes[i].AttributeClass?.TypeArguments.Length is null or < 1)
                        continue;

                    foreach(var potentialRelationSymbol in ctx.Attributes[i].AttributeClass!.TypeArguments)
                    {
                        ct.ThrowIfCancellationRequested();
                        if(potentialRelationSymbol is INamedTypeSymbol relationSymbol &&
                           relationSymbol.GetAttributes()
                                .OfAliasedUnionTypeBaseAttribute()
                                .Concat(relationSymbol.TypeParameters
                                    .SelectMany(p => p.GetAttributes().OfAliasedUnionTypeBaseAttribute()))
                                .Any())
                        {
                            var model = RelationModel.Create(target, relationSymbol, ct);
                            relations.Add(model);
                        }
                    }
                }

                return (targetSignature, relations.AsEquatable());
            }).Where(m => m.HasValue && m.Value.relations.Count > 0)
            .Select((t, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return t!.Value;
            })
            .Collect()
            .WithCollectionComparer()
            .Select((tuples, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return tuples.AsEquatable();
            });
    }
    #endregion
    #region Settings FAWMN
    private static IncrementalValueProvider<(SettingsAttributeData data, SettingsModel parsed)> CreateAssemblySettingsProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
            UnionTypeSettingsAttribute.MetadataName,
            (node, ct) => node is CompilationUnitSyntax,
            (ctx, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                var result = SettingsAttributeData.CreateAssemblySettingsWithDefaults(ctx.Attributes[0]);
                return result;
            })
        .Collect()
        .WithCollectionComparer()
        .Select((attributes, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            var result = attributes.Length > 0 ?
                attributes[0] :
                SettingsAttributeData.Default;

            return result;
        }).Select((data, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            var attribute = UnionTypeSettingsAttribute.TryCreate(data, out var a) ?
                a :
                new();

            ct.ThrowIfCancellationRequested();
            var settings = SettingsModel.Create(attribute!);

            return (data, settings);
        });
    private static SettingsMapProvider CreateSettingsProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName<
            (TypeSignatureModel? targetSignature, SettingsAttributeData Settings)>(
            UnionTypeSettingsAttribute.MetadataName,
            Qualifications.IsUnionTypeDeclarationSyntax,
            (ctx, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                var settings = SettingsAttributeData.CreateDeclaredSettings(ctx, ct);
                var targetSignature = ctx.TargetSymbol is ITypeSymbol type ?
                    TypeSignatureModel.Create(type, ct) :
                    null;

                var result = (targetSignature, settings);

                return result;
            }).Where(t => t.targetSignature != null)
            .Collect()
            .WithCollectionComparer()
            .Select((tuples, ct) =>
            {
                //convert to equatable for combine result to be equatable
                ct.ThrowIfCancellationRequested();
                return tuples.AsEquatable();
            })
            .Combine(CreateAssemblySettingsProvider(context))
            .Select((tuple, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                var (declaredSettingsTuples, assemblySettingsTuple) = tuple;
                var (assemblySettingsData, assemblySettings) = assemblySettingsTuple;

                var definedSettings = new Dictionary<TypeSignatureModel, SettingsModel>();
                foreach(var (target, declaredSettings) in declaredSettingsTuples)
                {
                    ct.ThrowIfCancellationRequested();

                    if(target == null || definedSettings.ContainsKey(target))
                        continue;

                    var defaultsAppliedSettings = SettingsAttributeData.CreateDeclaredSettingsWithDefaults(
                        declaredSettings,
                        assemblySettingsData);

                    if(!UnionTypeSettingsAttribute.TryCreate(defaultsAppliedSettings, out var parsedSettings))
                        continue;

                    var settings = SettingsModel.Create(parsedSettings!);

                    definedSettings.Add(target, settings);
                }

                return (assemblySettings, definedSettings.AsEquatable());
            });
    #endregion
    #region Factory FAWMN
    private static FactoryMapProvider CreateFactoryMapProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName<
            (TypeSignatureModel targetSignature, TypeSignatureModel representableTypeSignature, FactoryModel factory)?>(
            UnionTypeFactoryAttribute.MetadataName,
            Qualifications.IsUnionTypeFactoryDeclarationSyntax,
            (ctx, ct) =>
            {
                //transform factory to (containing union type, representable type to create union with, factory model)

                ct.ThrowIfCancellationRequested();
                //check target is valid parameter in method symbol
                if(ctx.TargetSymbol is not IParameterSymbol
                    {
                        ContainingSymbol: IMethodSymbol
                        {
                            ReturnType.NullableAnnotation: NullableAnnotation.NotAnnotated or NullableAnnotation.None
                        }
                    })
                {
                    return null;
                }

                ct.ThrowIfCancellationRequested();
                //check method returns non nullable union type
                var method = (IMethodSymbol)ctx.TargetSymbol.ContainingSymbol;
                if(!method.ReturnType.Equals(method.ContainingType, SymbolEqualityComparer.Default))
                {
                    return null;
                }

                ct.ThrowIfCancellationRequested();
                var targetSignature = TypeSignatureModel.Create(method.ContainingType, ct);
                var representableType = ( (IParameterSymbol)ctx.TargetSymbol ).Type;
                var representableTypeSignature = TypeSignatureModel.Create(representableType, ct);
                var factory = FactoryModel.CreateCustom(method.Name, representableType, ct);

                return (targetSignature, representableTypeSignature, factory);
            }).Where(t => t.HasValue)
            .Collect()
            .WithCollectionComparer()
            .Select((tuples, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var mutableResult = new Dictionary<TypeSignatureModel, (HashSet<TypeSignatureModel> set, List<FactoryModel> list)>();
                var result = new Dictionary<TypeSignatureModel, EquatableList<FactoryModel>>();
                for(var i = 0; i < tuples.Length; i++)
                {
                    var (targetSignature, representableTypeSignature, factory) = tuples[i]!.Value;
                    if(!mutableResult.TryGetValue(targetSignature, out var mutableItem))
                    {
                        mutableItem = ([], []);
                        mutableResult.Add(targetSignature, mutableItem);
                        result.Add(targetSignature, new(mutableItem.list));
                    }

                    if(mutableItem.set.Add(representableTypeSignature))
                        mutableItem.list.Add(factory);
                }

                //Map(UnionTypeSignature, Map(RepresentableTypeSignature, Factory))
                return result.AsEquatable();
            });
    #endregion
    #region UnionType FAWMN
    private static PartialUnionTypeModelsProvider CreateTypeDeclarationTargetProvider(IncrementalGeneratorInitializationContext context, String name) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
            name,
            Qualifications.IsUnionTypeDeclarationSyntax,
            PartialUnionTypeModel.CreateFromTypeDeclaration)
        .Collect()
        .WithCollectionComparer()
        .Select((models, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            var result = models.SelectMany(m => m).ToEquatableList(ct);
            return result;
        });
    private static PartialUnionTypeModelsProvider CreateTypeParameterTargetProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
            Qualifications.NonGenericFullMetadataName,
            Qualifications.IsUnionTypeParameterSyntax,
            PartialUnionTypeModel.CreateFromTypeParameter)
            .Where(m => m != null)
            .Collect()
            .WithCollectionComparer()
            .Select((models, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return models.AsEquatable();
            })!;
    private static PartialUnionTypeModelsProvider CreateUnionTypeProvider(IncrementalGeneratorInitializationContext context) =>
        Qualifications.GenericMetadataNames
            .Select(name => CreateTypeDeclarationTargetProvider(context, name))
            .Aggregate(
                CreateTypeParameterTargetProvider(context),
                (leftProvider, rightProvider) =>
                leftProvider.Combine(rightProvider)
                .Select((models, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    var result = models.Left.Concat(models.Right).ToEquatableList(ct);
                    return result;
                }));
    #endregion
    #region Constant Sources
    private static void RegisterConstantSources(IncrementalGeneratorPostInitializationContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        context.AddSource("RhoMicro_CodeAnalysis_UnionTypeAttribute.g.cs", AliasedUnionTypeBaseAttribute.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_RelationTypeAttribute.g.cs", RelationAttribute<Object>.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_UnionFactoryAttribute.g.cs", UnionTypeFactoryAttribute.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_UnionTypeSettingsAttribute.g.cs", UnionTypeSettingsAttribute.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_UnionsGenerator_Generated_Util.g.cs", ConstantSources.Util);
    }
    #endregion
}
