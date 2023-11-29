namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

internal static partial class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> handler)
    {
        foreach(var v in values)
        {
            handler.Invoke(v);
        }
    }

    private static readonly Dictionary<ITypeSymbol, Boolean?> _valueTypeCache = new(SymbolEqualityComparer.Default);
    public static Boolean IsPureValueType(this ITypeSymbol symbol)
    {
        evaluate(symbol);

        if(!_valueTypeCache[symbol].HasValue)
        {
            throw new Exception($"Unable to determine whether {symbol.Name} is value type.");
        }

        var result = _valueTypeCache[symbol]!.Value;

        return result;

        static void evaluate(ITypeSymbol symbol)
        {
            if(_valueTypeCache.TryGetValue(symbol, out var currentResult))
            {
                //cache could be initialized but undefined (null)
                if(currentResult.HasValue)
                {
                    //cache was not null
                    return;
                }
            } else
            {
                //initialize cache for type
                _valueTypeCache[symbol] = null;
            }

            if(!symbol.IsValueType)
            {
                _valueTypeCache[symbol] = false;
                return;
            }

            var members = symbol.GetMembers();
            foreach(var member in members)
            {
                if(member is IFieldSymbol field)
                {
                    //is field type uninitialized in cache?
                    if(!_valueTypeCache.ContainsKey(field.Type))
                    {
                        //initialize & define
                        evaluate(field.Type);
                    }

                    var fieldTypeIsValueType = _valueTypeCache[field.Type];
                    if(fieldTypeIsValueType.HasValue && !fieldTypeIsValueType.Value)
                    {
                        //field type was initialized but found not to be value type
                        //apply transitive property
                        _valueTypeCache[symbol] = false;
                        return;
                    }
                }
            }

            //no issues found :)
            _valueTypeCache[symbol] = true;
        }
    }
    public static IncrementalValuesProvider<SourceCarry<TResult>> SelectCarry<TSource, TResult>(
        this IncrementalValuesProvider<SourceCarry<TSource>> provider,
        Func<TSource, DiagnosticsModelBuilder, SourceModelBuilder, CancellationToken, TResult> project) =>
        provider.Select((c, t) => c.Project((s, d, b) => project.Invoke(s, d, b, t)));
    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>> SelectCarry<TModel>(
        this IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider,
        Func<ModelCreationContext, TModel> modelFactory,
        Action<ModelIntegrationContext<TModel>> modelIntegration) =>
        provider.SelectCarry((c, d, s, t) =>
        {
            t.ThrowIfCancellationRequested();

            var invocationContext = new ModelCreationContext(c, t);
            var model = modelFactory.Invoke(invocationContext);

            var integrationContext = new ModelIntegrationContext<TModel>(d, s, t, model);
            modelIntegration.Invoke(integrationContext);

            return c;
        });

    public static Boolean ShouldReport(this Diagnostic diagnostic, DiagnosticsLevelSettings settings) =>
        diagnostic.Severity switch
        {
            DiagnosticSeverity.Error => settings.HasFlag(DiagnosticsLevelSettings.Error),
            DiagnosticSeverity.Warning => settings.HasFlag(DiagnosticsLevelSettings.Warning),
            DiagnosticSeverity.Info => settings.HasFlag(DiagnosticsLevelSettings.Info),
            _ => false
        };

    public static Boolean IsError(this Diagnostic diagnostic) =>
        diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error;
    public static Boolean HasUnionTypeAttribute(this ITypeSymbol symbol) =>
        symbol.GetAttributes().OfUnionTypeAttribute().Any();
    public static String ToDocCompatString(this ITypeSymbol symbol) =>
        symbol.ToFullString().Replace("<", "&lt;").Replace(">", "&gt;");
    public static String ToIdentifierCompatString(this ITypeSymbol symbol) =>
        symbol.ToOpenString()
            .Replace("<", "_of_")
            .Replace('>', '_')
            .Replace(",", "_and_")
            .Replace(" ", String.Empty)
            .TrimEnd('_');
    public static String ToHintName(this ITypeSymbol symbol) =>
        symbol.ToFullString()
            .Replace("<", "_of_")
            .Replace('>', '_')
            .Replace(",", "_and_")
            .Replace(" ", String.Empty)
            .Replace('.', '_')
            .Replace("::", "_")
            .TrimEnd('_');
    public static String ToOpenString(this ISymbol symbol) =>
        symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithMiscellaneousOptions(
                    /*
                        get rid of special types

                             10110
                        NAND 00100
                          => 10010

                             10110
                          &! 00100
                          => 10010

                             00100
                           ^ 11111
                          => 11011

                             10110
                           & 11011
                          => 10010
                    */
                    SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
                    (SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue))
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters));
    private static readonly SymbolDisplayFormat _fullStringFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
                    .WithMiscellaneousOptions(
                    /*
                        get rid of special types

                             10110
                        NAND 00100
                          => 10010

                             10110
                          &! 00100
                          => 10010

                             00100
                           ^ 11111
                          => 11011

                             10110
                           & 11011
                          => 10010
                    */
                    SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
                    (SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue))
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters);
    public static String ToFullString(this ISymbol symbol) => symbol.ToDisplayString(_fullStringFormat);
    private static readonly SymbolDisplayFormat _toTypeStringFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
                    .WithMiscellaneousOptions(
                    /*
                        get rid of special types

                             10110
                        NAND 00100
                          => 10010

                             10110
                          &! 00100
                          => 10010

                             00100
                           ^ 11111
                          => 11011

                             10110
                           & 11011
                          => 10010
                    */
                    SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
                    (SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue))
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);
    public static String ToTypeString(this ITypeSymbol symbol) => symbol.ToDisplayString(_toTypeStringFormat);

    public static StringBuilder AppendOpen(this StringBuilder builder, RepresentableTypeData data) =>
        builder.Append(data.Names.OpenTypeName);
    public static StringBuilder AppendOpen(this StringBuilder builder, ISymbol symbol) =>
        builder.Append(symbol.ToOpenString());
    public static StringBuilder AppendCommentRef(this StringBuilder builder, RepresentableTypeData data) =>
        builder.Append(data.DocCommentRef);
    public static StringBuilder AppendCommentRef(this StringBuilder builder, TargetDataModel data) =>
        builder.AppendCommentRef(data.Symbol);
    public static StringBuilder AppendCommentRef(this StringBuilder builder, ISymbol symbol) =>
        builder.Append("<see cref=\"").Append(symbol.ToFullString().Replace('<', '{').Replace('>', '}')).Append("\"/>");
    public static StringBuilder AppendFull(this StringBuilder builder, ISymbol symbol) =>
        builder.Append(symbol.ToFullString());
    public static StringBuilder AppendFull(this StringBuilder builder, RepresentableTypeData data) =>
        builder.Append(data.Names.FullTypeName);

    public static String GetContainingClassHead(this ITypeSymbol nestedType)
    {
        var resultBuilder = new StringBuilder();
        var headers = getContainingTypes(nestedType)
            .Select(s => s.TypeKind switch
            {
                TypeKind.Class => "class ",
                TypeKind.Struct => "struct ",
                TypeKind.Interface => "interface ",
                _ => null
            } + s.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)))
            .Where(k => k != null)
            .Aggregate(
                resultBuilder,
                (b, s) => b.Append("partial ").Append(s).AppendLine("{"));

        var result = resultBuilder.ToString();

        return result;

        static IEnumerable<ITypeSymbol> getContainingTypes(ITypeSymbol symbol)
        {
            if(symbol.ContainingType != null)
            {
                return getContainingTypes(symbol.ContainingType).Append(symbol.ContainingType);
            }

            return Array.Empty<ITypeSymbol>();
        }
    }
    public static String GetContainingClassTail(this ITypeSymbol nestedType)
    {
        var resultBuilder = new StringBuilder();
        var containingType = nestedType.ContainingType;
        while(containingType != null)
        {
            _ = resultBuilder.AppendLine("}");

            containingType = containingType.ContainingType;
        }

        var result = resultBuilder.ToString();

        return result;
    }

    //source: https://stackoverflow.com/a/58853591
    private static readonly Regex _camelCasePattern =
        new(@"([A-Z])([A-Z]+|[a-z0-9_]+)($|[A-Z]\w*)", RegexOptions.Compiled);
    public static String ToCamelCase(this String value)
    {
        if(value.Length == 0)
        {
            return value;
        }

        if(value.Length == 1)
        {
            return value.ToLowerInvariant();
        }

        //source: https://stackoverflow.com/a/58853591
        var result = _camelCasePattern.Replace(
            value,
            static m => $"{m.Groups[1].Value.ToLowerInvariant()}{m.Groups[2].Value.ToLowerInvariant()}{m.Groups[3].Value}");

        return result;
    }
    public static String ToGeneratedCamelCase(this String value)
    {
        var result = value.ToCamelCase();
        if(result.StartsWith("__"))
        {
            return result;
        } else if(result.StartsWith("_"))
        {
            return $"_{result}";
        }

        return $"__{result}";
    }
    public static StringBuilder AppendSwitchExpression<T>(
    this StringBuilder builder,
    IReadOnlyCollection<T> values,
    Func<StringBuilder, StringBuilder> valueBuilder,
    Func<StringBuilder, T, StringBuilder> caseBuilder,
    Func<StringBuilder, T, StringBuilder> bodyBuilder,
    Func<StringBuilder, StringBuilder> defaultBodyBuilder) =>
   defaultBodyBuilder.Invoke(
       valueBuilder.Invoke(builder)
       .AppendLine(" switch")
       .AppendLine("{")
       .AppendAggregate(
           values,
           (b, v) => bodyBuilder.Invoke(
               caseBuilder.Invoke(b, v).Append(" => "),
               v).AppendLine(","))
       .Append("_ => "))
    .AppendLine("}");
    public static StringBuilder AppendTypeSwitchExpression<T>(
    this StringBuilder builder,
    IReadOnlyCollection<T> values,
    String valueTypeExpression,
    Func<T, String> caseBuilder,
    Func<StringBuilder, T, StringBuilder> bodyBuilder,
    Func<StringBuilder, StringBuilder> defaultBodyBuilder) =>
    builder.AppendSwitchExpression(
        values,
        b => b.Append(valueTypeExpression),
        (b, v) => b.Append('"').Append(caseBuilder.Invoke(v)).Append('"'),
        bodyBuilder,
        defaultBodyBuilder);
    public static StringBuilder AppendSwitchStatement<T>(
        this StringBuilder builder,
        IReadOnlyCollection<T> values,
        Func<StringBuilder, StringBuilder> valueBuilder,
        Func<StringBuilder, T, StringBuilder> caseBuilder,
        Func<StringBuilder, T, StringBuilder> bodyBuilder,
        Func<StringBuilder, StringBuilder> defaultBodyBuilder) =>
        defaultBodyBuilder.Invoke(
            valueBuilder.Invoke(
                builder.Append("switch("))
            .AppendLine(")")
            .AppendLine("{")
            .AppendAggregate(
                values,
                (b, v) => bodyBuilder.Invoke(
                    caseBuilder.Invoke(
                        b.Append("case "), v)
                    .AppendLine(":")
                    .AppendLine("{"),
                    v)
                .AppendLine("}"))
            .Append("default:{"))
        .AppendLine("}")
        .AppendLine("}");
    public static StringBuilder AppendTypeSwitchStatement<T>(
    this StringBuilder builder,
    IReadOnlyCollection<T> values,
    String valueTypeExpression,
    Func<T, RepresentableTypeNames> caseBuilder,
    Func<StringBuilder, T, StringBuilder> bodyBuilder,
    Func<StringBuilder, StringBuilder> defaultBodyBuilder) =>
    builder.AppendSwitchStatement(
        values,
        b => b.Append(valueTypeExpression),
        (b, v) => b.Append('"').Append(caseBuilder.Invoke(v).FullTypeName).Append('"'),
        bodyBuilder,
        defaultBodyBuilder);

    public static StringBuilder AppendUnknownConversion(
        this StringBuilder builder,
        UnionDataModel targetUnionType,
        RepresentableTypeData sourceData,
        RepresentableTypeData targetData,
        String parameterName) =>
        builder.Append('(')
            .AppendFull(targetUnionType.Symbol)
            .Append('.')
            .Append(targetData.Names.CreateFromFunctionName)
            .Append('(')
            .Append(parameterName)
            .Append('.')
            .Append(sourceData.Names.AsPropertyName)
            .Append("))");
    public static StringBuilder AppendKnownConversion(
        this StringBuilder builder,
        UnionDataModel targetUnionType,
        RepresentableTypeData sourceData,
        RepresentableTypeData targetData,
        String parameterName) =>
        builder.Append('(')
            .AppendFull(targetUnionType.Symbol)
            .Append('.')
            .Append(targetData.Names.CreateFromFunctionName)
            .Append('(')
            .Append(sourceData.Storage.GetInstanceVariableExpression(parameterName))
            .Append("))");

    public static StringBuilder AppendJoin<T>(this StringBuilder builder, String separator, IEnumerable<T> values)
    {
        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        _ = builder.Append(iterator.Current?.ToString() ?? String.Empty);

        while(iterator.MoveNext())
        {
            _ = builder.Append(separator).Append(iterator.Current?.ToString() ?? String.Empty);
        }

        return builder;
    }
    public static StringBuilder AppendAggregate<T>(
        this StringBuilder builder,
        IEnumerable<T> values,
        Func<StringBuilder, T, StringBuilder> aggregation) => values.Aggregate(builder, aggregation);
    public static StringBuilder AppendAggregateJoin<T>(
        this StringBuilder builder,
        String separator,
        IEnumerable<T> values,
        Func<StringBuilder, T, StringBuilder> aggregation)
    {
        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        builder = aggregation.Invoke(builder, iterator.Current);

        while(iterator.MoveNext())
        {
            builder = aggregation.Invoke(builder.Append(separator), iterator.Current);
        }

        return builder;
    }
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, Char separator, IEnumerable<T> values)
    {
        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        _ = builder.Append(iterator.Current?.ToString() ?? String.Empty);

        while(iterator.MoveNext())
        {
            _ = builder.Append(separator).Append(iterator.Current?.ToString() ?? String.Empty);
        }

        return builder;
    }

    public static Boolean InheritsFrom(this ITypeSymbol subtype, ITypeSymbol supertype)
    {
        var baseTypes = getBaseTypes(subtype);
        if(baseTypes.Contains(supertype, SymbolEqualityComparer.Default))
        {
            return true;
        }

        var interfaces = subtype.AllInterfaces;
        if(interfaces.Contains(supertype, SymbolEqualityComparer.Default))
        {
            return true;
        }

        return false;

        static IEnumerable<INamedTypeSymbol> getBaseTypes(ITypeSymbol symbol)
        {
            var baseType = symbol.BaseType;
            while(baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
    }
    private static readonly HashSet<String> _attributeNames =
    [
        nameof(UnionTypeAttribute),
        nameof(UnionTypeAttribute).Substring(0, nameof(UnionTypeAttribute).Length - nameof(Attribute).Length),
        nameof(UnionTypeSettingsAttribute),
        nameof(UnionTypeSettingsAttribute).Substring(0, nameof(UnionTypeSettingsAttribute).Length - nameof(Attribute).Length),
        nameof(RelationAttribute),
        nameof(RelationAttribute).Substring(0, nameof(RelationAttribute).Length - nameof(Attribute).Length)
    ];

    public static Boolean IsUnionDeclaration(SyntaxNode node, CancellationToken _)
    {
        var result = node is TypeDeclarationSyntax typeDeclaration &&
            typeDeclaration.AttributeLists
            .SelectMany(a => a.Attributes)
            .Select(a => a.Name.ToString())
            .Any(_attributeNames.Contains);

        return result;
    }
    public static Boolean IsPartial(this TypeDeclarationSyntax declarationSyntax) =>
        declarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);
}
