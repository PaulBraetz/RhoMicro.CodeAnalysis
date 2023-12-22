namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using System.Text.RegularExpressions;
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

    public static void CommentRef(this ISymbol symbol, ExpandingMacroBuilder builder) =>
        _ = builder * "<see cref=\"" * symbol.ToFullOpenString().Replace('<', '{').Replace('>', '}') * "\"/>";
    public static void CommentRef(this RepresentableTypeModel data, ExpandingMacroBuilder builder) =>
        _ = builder * data.DocCommentRef;
    public static void CommentRef(this TargetDataModel data, ExpandingMacroBuilder builder) =>
        data.Symbol.CommentRef(builder);
}
