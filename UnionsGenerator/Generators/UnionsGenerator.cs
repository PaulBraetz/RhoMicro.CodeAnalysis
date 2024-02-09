namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator._Models;
using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System;

using FactoryMapProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableDictionary<_Models.TypeSignatureModel, EquatableDictionary<_Models.TypeSignatureModel, _Models.FactoryModel>>>;
using PartialUnionTypeModelsProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableList<_Models.PartialUnionTypeModel>>;
using PartialTargetTypeModelUnionProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableDictionary<_Models.TypeSignatureModel, EquatableList<_Models.PartialRepresentableTypeModel>>>;
using RelationsProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<EquatableDictionary<_Models.TypeSignatureModel, EquatableList<_Models.RelationModel>>>;
using SettingsMapProvider = Microsoft.CodeAnalysis.IncrementalValueProvider<(_Models.SettingsModel fallbackSettings, EquatableDictionary<_Models.TypeSignatureModel, _Models.SettingsModel> definedSettings)>;
using SourceTextProvider = Microsoft.CodeAnalysis.IncrementalValuesProvider<(String hintName, String source)>;

[Generator(LanguageNames.CSharp)]
internal class UnionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var factoryMapProvider = CreateFactoryMapProvider(context);
        var settingsProvider = CreateSettingsProvider(context);
        var relationsProvider = CreateRelationsProvider(context);

        var typeTargetProvider = CreateTypeDeclarationTargetProvider(context);
        var parameterTargetProvider = CreateTypeParameterTargetProvider(context);
        var partialTargetProvider = UnionPartialTypeModelProviders(typeTargetProvider, parameterTargetProvider);

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
        provider.Select((model, ct) =>
        {
            ct.ThrowIfCancellationRequested();

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
                var factories = tuple.Left.Right;
                var (fallbackSettings, definedSettings) = tuple.Left.Left.Right;
                var partials = tuple.Left.Left.Left;

                var targetTypeModels = new List<UnionTypeModel>();

                foreach(var (targetTypeSig, partialUnionTypes) in partials)
                {
                    ct.ThrowIfCancellationRequested();

                    var settings = getSettings(targetTypeSig);
                    var relationAttributes = getRelations(targetTypeSig);
                    var unionTypeModels = partialUnionTypes.Select(partial =>
                    {
                        var factory = getFactory(targetTypeSig, partial);
                        var result = RepresentableTypeModel.Create(partial, factory, ct);
                        return result;
                    }).Take(256) //limit to 256 due to tag type being byte
                    .ToEquatableList(ct);

                    var targetModel = UnionTypeModel.Create(targetTypeSig, unionTypeModels, relationAttributes, settings, ct);
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
                FactoryModel getFactory(TypeSignatureModel targetTypeSig, PartialRepresentableTypeModel partialModel) =>
                    factories!.TryGetValue(targetTypeSig, out var representableTypeFactoryMap) &&
                    representableTypeFactoryMap.TryGetValue(partialModel.Signature, out var factory) ?
                    factory :
                    FactoryModel.CreateGenerated(partialModel);
            }).SelectMany((models, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return models;
            });
    private static PartialTargetTypeModelUnionProvider UnionPartialTypeModelProviders(
        PartialUnionTypeModelsProvider typeDeclarationTargetprovider,
        PartialUnionTypeModelsProvider typeParameterTargetProvider) =>
        typeDeclarationTargetprovider.Combine(typeParameterTargetProvider)
            .Select((pair, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var (left, right) = pair;
                var keyValuePairs = left.Concat(right);
                var mutableResult = new Dictionary<TypeSignatureModel, (List<PartialRepresentableTypeModel> models, HashSet<TypeSignatureModel> mappedRepresentableTypes)>();
                var result = new Dictionary<TypeSignatureModel, EquatableList<PartialRepresentableTypeModel>>();
                foreach(var (key, value) in keyValuePairs)
                {
                    ct.ThrowIfCancellationRequested();
                    if(!mutableResult.TryGetValue(key, out var data))
                    {
                        data = ([], []);
                        mutableResult.Add(key, data);
                        result.Add(key, new(data.models));
                    }

                    if(data.mappedRepresentableTypes.Add(value.Signature))
                        data.models.Add(value);
                }

                return new EquatableDictionary<TypeSignatureModel, EquatableList<PartialRepresentableTypeModel>>(result);
            });
    #endregion
    #region Relations FAWMN
    private static RelationsProvider CreateRelationsProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName<
            (TypeSignatureModel targetSignature, RelationModel attributeModel)?>(
            "RhoMicro.CodeAnalysis.UnionRelationAttribute`1",
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
                //check if T in [Relation<T>] is named type & is actual union type (has attribute on self or type param) <== consider marker attribute on generated impl?
                if(ctx.Attributes[0].AttributeClass?.TypeArguments.Length != 1 ||
                   ctx.Attributes[0].AttributeClass!.TypeArguments[0] is not INamedTypeSymbol relationSymbol ||
                   relationSymbol.GetAttributes()
                    .OfUnionTypeBaseAttribute()
                    .Concat(relationSymbol.TypeParameters
                        .SelectMany(p => p.GetAttributes().OfUnionTypeBaseAttribute()))
                    .Any())
                {
                    return null;
                }

                var targetSignature = TypeSignatureModel.Create(target, ct);
                var attributeModel = RelationModel.Create(target, relationSymbol, ct);

                return (targetSignature, attributeModel);
            }).Where(m => m.HasValue)
        .Collect()
        .WithCollectionComparer()
        .Select((tuples, ct) =>
        {
            ct.ThrowIfCancellationRequested();

            var mutableResult = new Dictionary<TypeSignatureModel, List<RelationModel>>();
            var result = new Dictionary<TypeSignatureModel, EquatableList<RelationModel>>();
            for(var i = 0; i < tuples.Length; i++)
            {
                var (targetSignature, relationAttributeModel) = tuples[i]!.Value;
                if(!mutableResult.TryGetValue(targetSignature, out var models))
                {
                    models = [];
                    mutableResult.Add(targetSignature, models);
                    result.Add(targetSignature, new(models));
                }

                models.Add(relationAttributeModel);
            }

            //Map(UnionTypeSignature, List(Relation))
            return result.AsEquatable();
        });
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
            var settings = SettingsModel.Create(attribute!, data.ImplementsToString);

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

                    var settings = SettingsModel.Create(parsedSettings!, defaultsAppliedSettings.ImplementsToString);

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

                var mutableResult = new Dictionary<TypeSignatureModel, Dictionary<TypeSignatureModel, FactoryModel>>();
                var result = new Dictionary<TypeSignatureModel, EquatableDictionary<TypeSignatureModel, FactoryModel>>();
                for(var i = 0; i < tuples.Length; i++)
                {
                    var (targetSignature, representableTypeSignature, factory) = tuples[i]!.Value;
                    if(!mutableResult.TryGetValue(targetSignature, out var factoryMap))
                    {
                        factoryMap = [];
                        mutableResult.Add(targetSignature, factoryMap);
                        result.Add(targetSignature, new(factoryMap));
                    }

                    if(!factoryMap.ContainsKey(representableTypeSignature))
                        factoryMap.Add(representableTypeSignature, factory);
                }

                //Map(UnionTypeSignature, Map(RepresentableTypeSignature, Factory))
                return result.AsEquatable();
            });
    #endregion
    #region Type Parameter UnionTypeAttribute FAWMN
    private static PartialUnionTypeModelsProvider CreateTypeParameterTargetProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
                UnionTypeBaseAttribute.NonGenericMetadataName,
                Qualifications.IsUnionTypeParameterSyntax,
                PartialUnionTypeModel.CreateFromTypeParameter)
            .Where(m => m != default)
            .Collect()
            .WithCollectionComparer()
            .Select((models, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return models.AsEquatable();
            })!;
    #endregion
    #region Type Declaration UnionTypeAttribute FAWMN
    private static PartialUnionTypeModelsProvider CreateTypeDeclarationTargetProvider(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
                UnionTypeBaseAttribute.GenericMetadataName,
                Qualifications.IsUnionTypeDeclarationSyntax,
                PartialUnionTypeModel.CreateFromTypeDeclaration)
            .Collect()
            .WithCollectionComparer()
            .Select((modelLists, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return modelLists.SelectMany(l => l).ToEquatableList(ct);
            })!;
    #endregion
    #region Constant Sources
    private static void RegisterConstantSources(IncrementalGeneratorPostInitializationContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        context.AddSource("RhoMicro_CodeAnalysis_UnionTypeAttribute.g.cs", UnionTypeBaseAttribute.SourceText); //contains implementation source too (same file)
        context.AddSource("RhoMicro_CodeAnalysis_RelationTypeAttribute.g.cs", RelationAttribute<Object>.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_UnionFactoryAttribute.g.cs", UnionTypeFactoryAttribute.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_UnionTypeSettingsAttribute.g.cs", UnionTypeSettingsAttribute.SourceText);
        context.AddSource("RhoMicro_CodeAnalysis_UnionsGenerator_Generated_Util.g.cs", ConstantSources.Util);
    }
    #endregion
}

internal enum Macro
{
    Head,
    NestedClasses,
    Constructors,
    Fields,
    Factories,
    Switch,
    Match,
    RepresentedTypes,
    IsAsProperties,
    IsAsFunctions,
    InterfaceIntersections,
    GetHashcode,
    Equals,
    ToString,
    Conversion,
    Tail,

    ConversionFunctionsCache
}
#if false
[Generator(LanguageNames.CSharp)]
internal class UnionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //TODO: implement model for efficient equality
        //TODO: implement incremental model building?
        var provider = context.SyntaxProvider.ForUnionTypeAttribute(
            static (n, t) => Util.IsGeneratorTarget(n),
            static (c, t) =>
            {
                var model = TargetDataModel.Create((TypeDeclarationSyntax)c.TargetNode, c.SemanticModel);

                var context = GeneratorContext.Create<Macro, TargetDataModel>(
                    model,
                    configureSourceTextBuilder: b => b.AppendHeader("RhoMicro.CodeAnalysis.UnionsGenerator")
                        .AppendMacros(t)
                        .Format()
                        .WithOperators<Macro, TargetDataModel>(t)
                        .Receive(new Head(model), t)
                        .Receive(new NestedTypes(model), t)
                        .Receive(new Constructors(model), t)
                        .Receive(new Fields(model), t)
                        .Receive(new Factories(model), t)
                        .Receive(new Tail(model), t)
                        .Receive(new RepresentedTypes(model), t)
                        .Receive(new IsAsProperties(model), t)
                        .Receive(new IsAsFunctions(model), t)
                        .Receive(new Match(model), t)
                        .Receive(new InterfaceIntersections(model), t)
                        .Receive(new GetHashCode(model), t)
                        .Receive(new Equals(model), t)
                        .Receive(new ConversionFunctionsCache(model), t)
                        .Receive(new Expansions.Switch(model), t)
                        .Receive(ToStringFunction.Create(model), t)
                        .Receive(new Expansions.Conversion(model), t),
                    configureDiagnosticsAccumulator: d => d
                        .DiagnoseNonHiddenSeverities()
                        .ReportOnlyNoneLocations()
                        .Receive(Providers.All, t));

                var source = BuildSource(context, t);

                var hintName = model.Symbol.ToHintName();
                var hintNameWithExtension = $"{hintName}.g.cs";
                var result = (source, hintName: hintNameWithExtension);

                if(TryGetSourceEmission(model, (source, hintName), out var emissionResult, t))
                {
                    return (result, emissionResult);
                }

                return (result, null);
            });

        context.RegisterSourceOutput(provider, static (c, t) =>
        {
            t.result.source.AddToContext(c, t.result.hintName);
            if(t.emissionResult.HasValue)
            {
                c.AddSource(t.emissionResult.Value.hintName, t.emissionResult.Value.source);
            }
        });
        context.RegisterPostInitializationOutput(static c => c.AddSource("Util.g.cs", ConstantSources.Util));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionFactoryAttribute)}.g.cs", UnionFactoryAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeBaseAttribute)}.g.cs", UnionTypeBaseAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(RelationAttribute)}.g.cs", RelationAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeSettingsAttribute)}.g.cs", UnionTypeSettingsAttribute.SourceText));
    }
    private static Boolean TryGetSourceEmission(TargetDataModel model, (GeneratorContextBuildResult<TargetDataModel> source, String hintName) s, out (String source, String hintName)? result, CancellationToken cancellationToken)
    {
        result = default;
        if(!model.Annotations.Settings.EmitGeneratedSourceCode)
            return false;

        var builder = ExpandingMacroStringBuilder.Create<Macro>().WithOperators(cancellationToken);
        Head.CreateEmissionHead(model).Expand(builder, cancellationToken);
        _ = builder *
            "public const String GeneratedSourceCode =" /
            "\"\"\"" /
            s.source.SourceText /
            "\"\"\";";
        new Tail(model).Expand(builder, cancellationToken);

        var resultSource = builder.Build();
        var resultHintName = $"{s.hintName}_Source.g.cs";

        result = (resultSource, resultHintName);
        return true;
    }
    private static GeneratorContextBuildResult<TargetDataModel> BuildSource(IGeneratorContext<Macro, TargetDataModel> context, CancellationToken t)
    {
        var source = context.BuildSource(t);

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source.SourceText,
            new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.Diagnose, SourceCodeKind.Regular),
            cancellationToken: t);
        var diagnostics = syntaxTree.GetDiagnostics(t);
        if(diagnostics.Any())
        {
            var newSourceText = $"/*\nThe generator failed to generate diagnostics-free source code.\nThis indicates a bug in the generator.\nPlease report this issue to the maintainer at https://github.com/PaulBraetz/RhoMicro.CodeAnalysis/issues.\n{String.Join("\n", diagnostics)}\n*/\n{String.Concat(Enumerable.Repeat('/', 150))}\n{source.SourceText}";
            source = source.WithSourceText(newSourceText);
        }

        return source;
    }
}
#endif