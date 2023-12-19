namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using System.Text.RegularExpressions;

static class Docs
{
    public static void Inherit(ExpandingMacroBuilder builder) =>
        _ = builder % "/// <inheritdoc/>";

    public static void Summary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary) =>
        _ = builder *
            "/// <summary>" /
            "/// " % summary %
            "/// </summary>";
    public static void Summary(ExpandingMacroBuilder builder, String summary) =>
        _ = builder *
            "/// <summary>" /
            "/// " % summary %
            "/// </summary>";
    public static void Summary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary, Action<ExpandingMacroBuilder> returns) =>
        _ = builder *
            (Summary, summary) *
            "/// <returns>" /
            "/// " % returns %
            "/// </returns>";
    public static void Summary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> typeParameters) =>
        _ = builder *
            (Summary, summary) *
            typeParameters.Select(p => (Action<ExpandingMacroBuilder>)(b => _ = b *
            "/// <typeparam name=\"" * p.Name * "\">" /
            "/// " % p.Summary %
            "/// </typeparam>"));

    public static void MethodSummary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters) =>
        _ = builder *
            (Summary, summary) *
            parameters.Select(p => (Action<ExpandingMacroBuilder>)(b => _ = b *
            "/// <param name=\"" * p.Name * "\">" /
            "/// " % p.Summary %
            "/// </param>"));
    public static void MethodSummary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> typeParameters) =>
        _ = builder *
            (Summary, summary, typeParameters) *
            parameters.Select(p => (Action<ExpandingMacroBuilder>)(b => _ = b *
            "/// <param name=\"" * p.Name * "\">" /
            "/// " % p.Summary %
            "/// </param>"));
    public static void MethodSummary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters, Action<ExpandingMacroBuilder> returns) =>
        _ = builder *
            (b => MethodSummary(b, summary, parameters)) *
            "/// <returns>" /
            "/// " % returns %
            "/// </returns>";
    public static void MethodSummary(ExpandingMacroBuilder builder, Action<ExpandingMacroBuilder> summary, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters, IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> typeParameters, Action<ExpandingMacroBuilder> returns) =>
        _ = builder *
            (b => MethodSummary(b, summary, parameters, typeParameters)) *
            "/// <returns>" /
            "/// " % returns %
            "/// </returns>";
}
internal static class Extensions
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
            throw new Exception($"Unable to determine whether {symbol.Name} is value type.");

        var result = _valueTypeCache[symbol]!.Value;

        return result;

        static void evaluate(ITypeSymbol symbol)
        {
            if(_valueTypeCache.TryGetValue(symbol, out var currentResult))
            {
                //cache could be initialized but undefined (null)
                if(currentResult.HasValue)
                    //cache was not null
                    return;
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
                if(member is IFieldSymbol field && !field.IsStatic)
                {
                    //is field type uninitialized in cache?
                    if(!_valueTypeCache.ContainsKey(field.Type))
                        //initialize & define
                        evaluate(field.Type);

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

    public static String ToIdentifierCompatString(this ITypeSymbol symbol) =>
        symbol.ToMinimalOpenString()
            .Replace("<", "_of_")
            .Replace('>', '_')
            .Replace(",", "_and_")
            .Replace(" ", String.Empty)
            .TrimEnd('_');
    public static String ToHintName(this ITypeSymbol symbol) =>
        symbol.ToFullOpenString()
            .Replace("<", "_of_")
            .Replace('>', '_')
            .Replace(",", "_and_")
            .Replace(" ", String.Empty)
            .Replace('.', '_')
            .Replace("::", "_")
            .TrimEnd('_');
    private static readonly SymbolDisplayFormat _minimalStringFormat =
    SymbolDisplayFormat.MinimallyQualifiedFormat
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
    public static String ToMinimalOpenString(this ISymbol symbol) => symbol.ToDisplayString(_minimalStringFormat);

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
    public static String ToFullOpenString(this ISymbol symbol) => symbol.ToDisplayString(_fullStringFormat);

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

    //source: https://stackoverflow.com/a/58853591
    private static readonly Regex _camelCasePattern =
        new(@"([A-Z])([A-Z]+|[a-z0-9_]+)($|[A-Z]\w*)", RegexOptions.Compiled);
    public static String ToCamelCase(this String value)
    {
        if(value.Length == 0)
            return value;

        if(value.Length == 1)
            return value.ToLowerInvariant();

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

    public static Boolean InheritsFrom(this ITypeSymbol subtype, ITypeSymbol supertype)
    {
        var baseTypes = getBaseTypes(subtype);
        if(baseTypes.Contains(supertype, SymbolEqualityComparer.Default))
            return true;

        var interfaces = subtype.AllInterfaces;
        if(interfaces.Contains(supertype, SymbolEqualityComparer.Default))
            return true;

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
    public static Boolean IsPartial(this TypeDeclarationSyntax declarationSyntax) =>
        declarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);

    public static ExpandingMacroBuilder WithOperators(this IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
        builder.WithOperators<Macro, TargetDataModel>(cancellationToken);

    public static void ContainingClassHead(ExpandingMacroBuilder builder, ITypeSymbol nestedType)
    {
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
                builder,
                (b, s) => b * "partial " * s % '{');

        static IEnumerable<ITypeSymbol> getContainingTypes(ITypeSymbol symbol)
        {
            while(symbol.ContainingType != null)
            {
                yield return symbol.ContainingType;

                symbol = symbol.ContainingType;
            }
        }
    }
    public static void ContainingClassTail(ExpandingMacroBuilder builder, ITypeSymbol nestedType)
    {
        var containingType = nestedType.ContainingType;
        while(containingType != null)
        {
            _ = builder % '}';

            containingType = containingType.ContainingType;
        }
    }

    public static void CommentRef(this ISymbol symbol, ExpandingMacroBuilder builder) =>
        _ = builder * "<see cref=\"" * symbol.ToFullOpenString().Replace('<', '{').Replace('>', '}') * "\"/>";
    public static void CommentRef(this RepresentableTypeModel data, ExpandingMacroBuilder builder) =>
        _ = builder * data.DocCommentRef;
    public static void CommentRef(this TargetDataModel data, ExpandingMacroBuilder builder) =>
        data.Symbol.CommentRef(builder);

    public static void SwitchExpression<T>(
    ExpandingMacroBuilder builder,
    IReadOnlyCollection<T> values,
    Action<ExpandingMacroBuilder> value,
    Action<ExpandingMacroBuilder, T> @case,
    Action<ExpandingMacroBuilder, T> body,
    Action<ExpandingMacroBuilder> defaultBody) =>
       _ = (builder * value / " switch" % '{')
        .AppendJoin(
           values,
           (b, v, t) => _ = b.WithOperators(builder.CancellationToken) * (b => @case(b, v)) * " => " * (b => body(b, v)) % ',',
           builder.CancellationToken)
        .WithOperators(builder.CancellationToken) *
        "_ => " * defaultBody / String.Empty % '}';
    public static void TypeSwitchExpression<T>(
    ExpandingMacroBuilder builder,
    IReadOnlyCollection<T> values,
    Action<ExpandingMacroBuilder> valueType,
    Action<ExpandingMacroBuilder, T> @case,
    Action<ExpandingMacroBuilder, T> body,
    Action<ExpandingMacroBuilder> defaultBody) =>
    SwitchExpression(
        builder,
        values,
        (b) => _ = b * valueType,
        (b, v) => _ = b * '"' * (b => @case(b, v)) * '"',
        body,
        defaultBody);
    public static void SwitchStatement<T>(
        ExpandingMacroBuilder builder,
        IReadOnlyCollection<T> values,
        Action<ExpandingMacroBuilder> value,
        Action<ExpandingMacroBuilder, T> @case,
        Action<ExpandingMacroBuilder, T> body,
        Action<ExpandingMacroBuilder> defaultBody) =>
        _ = (builder * "switch(" * value % ')' % '{')
        .AppendJoin(
            values,
            (b, v, t) => _ = b.WithOperators(t) * "case " * (b => @case(b, v)) * ':' / '{' / (b => body(b, v)) % '}',
            builder.CancellationToken)
        .WithOperators(builder.CancellationToken) *
        "default:{" * defaultBody % '}' % '}';
    public static void TypeSwitchStatement<T>(
    ExpandingMacroBuilder builder,
    IReadOnlyCollection<T> values,
    Action<ExpandingMacroBuilder> valueTypeExpression,
    Func<T, RepresentableTypeNames> caseSelector,
    Action<ExpandingMacroBuilder, T> body,
    Action<ExpandingMacroBuilder> defaultBody) =>
    SwitchStatement(
        builder,
        values,
        valueTypeExpression,
        (b, v) => _ = b * '"' * caseSelector.Invoke(v).FullTypeName * '"',
        body,
        defaultBody);

    public static void UtilUnsafeConvert(
        ExpandingMacroBuilder builder,
        String tFrom,
        String tTo,
        String valueExpression) =>
        _ = builder * "(global::RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.UnsafeConvert<" * tFrom * ", " * tTo * ">(" * valueExpression * "))";
    public static void UtilFullString(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> typeExpression) =>
        _ = builder * $"(global::RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.GetFullString(" * typeExpression * "))";
    public static void InvalidConversionThrow(
        ExpandingMacroBuilder builder,
        String typeName) =>
        _ = builder * "throw new global::System.InvalidOperationException($\"The union type instance cannot be converted to an instance of {" * typeName * "}.\")";
    public static void InvalidCreationThrow(
        ExpandingMacroBuilder builder,
        String unionTypeName,
        String valueName) =>
        builder.Append("throw new global::System.ArgumentException($\"The value provided for \\\"")
            .Append(valueName).Append("\\\" cannot be converted to an instance of {")
            .Append(unionTypeName).Append("}.\", \"").Append(valueName).Append("\")");

    public static void UnknownConversion(
    ExpandingMacroBuilder builder,
    UnionDataModel targetUnionType,
    RepresentableTypeModel sourceData,
    RepresentableTypeModel targetData,
    String parameterName) =>
    _ = builder * '(' * targetUnionType.Symbol.ToFullOpenString() * '.' * targetData.Names.CreateFromFunctionName * '(' * parameterName * '.' * sourceData.Names.AsPropertyName * "))";
    public static void KnownConversion(
        ExpandingMacroBuilder builder,
        UnionDataModel targetUnionType,
        RepresentableTypeModel sourceData,
        RepresentableTypeModel targetData,
        String parameterName) =>
        _ = builder * '(' * targetUnionType.Symbol.ToFullOpenString() * '.' * targetData.Names.CreateFromFunctionName *
        '(' * (sourceData.Storage.TypesafeInstanceVariableExpression, parameterName) * "))";
}
