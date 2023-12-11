namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

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
                (b, s) => b.Append("partial ").Append(s).Append('{'));

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
            _ = resultBuilder.Append('}');

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
    public static Boolean IsPartial(this TypeDeclarationSyntax declarationSyntax) =>
        declarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);

    public static IExpandingMacroStringBuilder<Macro> AppendContainingClassHead(this IExpandingMacroStringBuilder<Macro> builder, ITypeSymbol nestedType)
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
                (b, s) => b.Append("partial ").Append(s).AppendLine('{'));

        return builder;

        static IEnumerable<ITypeSymbol> getContainingTypes(ITypeSymbol symbol)
        {
            while(symbol.ContainingType != null)
            {
                yield return symbol.ContainingType;

                symbol = symbol.ContainingType;
            }
        }
    }
    public static IExpandingMacroStringBuilder<Macro> AppendContainingClassTail(this IExpandingMacroStringBuilder<Macro> builder, ITypeSymbol nestedType)
    {
        var containingType = nestedType.ContainingType;
        while(containingType != null)
        {
            _ = builder.AppendLine('}');

            containingType = containingType.ContainingType;
        }

        return builder;
    }

    public static IExpandingMacroStringBuilder<Macro> AppendFull(this IExpandingMacroStringBuilder<Macro> builder, RepresentableTypeModel data) =>
        builder.Append(data.Names.FullTypeName);
    public static IExpandingMacroStringBuilder<Macro> AppendFull(this IExpandingMacroStringBuilder<Macro> builder, ISymbol symbol) =>
        builder.Append(symbol.ToFullOpenString());
    public static IExpandingMacroStringBuilder<Macro> AppendOpen(this IExpandingMacroStringBuilder<Macro> builder, ISymbol symbol) =>
        builder.Append(symbol.ToMinimalOpenString());
    public static IExpandingMacroStringBuilder<Macro> AppendCommentRef(this IExpandingMacroStringBuilder<Macro> builder, ISymbol symbol) =>
        builder.Append("<see cref=\"").Append(symbol.ToFullOpenString().Replace('<', '{').Replace('>', '}')).Append("\"/>");
    public static IExpandingMacroStringBuilder<Macro> AppendCommentRef(this IExpandingMacroStringBuilder<Macro> builder, RepresentableTypeModel data) =>
        builder.Append(data.DocCommentRef);
    public static IExpandingMacroStringBuilder<Macro> AppendCommentRef(this IExpandingMacroStringBuilder<Macro> builder, TargetDataModel data) =>
        builder.AppendCommentRef(data.Symbol);

    public static IExpandingMacroStringBuilder<Macro> AppendSwitchExpression<T>(
    this IExpandingMacroStringBuilder<Macro> builder,
    IReadOnlyCollection<T> values,
    Appendix<Macro> value,
    Appendix<Macro, T> @case,
    Appendix<Macro, T> body,
    Appendix<Macro> defaultBody,
    CancellationToken cancellationToken) =>
       builder.Append(value, cancellationToken)
        .AppendLine(" switch")
        .AppendLine('{')
        .AppendJoin(
           values,
           (b, v, t) => b.Append(@case, v, t).Append(" => ").Append(body, v, t).AppendLine(','),
           cancellationToken)
        .Append("_ => ")
        .Append(defaultBody, cancellationToken)
        .AppendLine('}');
    public static IExpandingMacroStringBuilder<Macro> AppendTypeSwitchExpression<T>(
    this IExpandingMacroStringBuilder<Macro> builder,
    IReadOnlyCollection<T> values,
    Appendix<Macro> valueTypeExpression,
    Appendix<Macro, T> @case,
    Appendix<Macro, T> body,
    Appendix<Macro> defaultBody,
    CancellationToken cancellationToken) =>
    builder.AppendSwitchExpression(
        values,
        (b, t) => b.Append(valueTypeExpression, t),
        (b, v, t) => b.Append('"').Append(@case, v, t).Append('"'),
        body,
        defaultBody,
        cancellationToken);
    public static IExpandingMacroStringBuilder<Macro> AppendSwitchStatement<T>(
        this IExpandingMacroStringBuilder<Macro> builder,
        IReadOnlyCollection<T> values,
        Appendix<Macro> value,
        Appendix<Macro, T> @case,
        Appendix<Macro, T> body,
        Appendix<Macro> defaultBody,
        CancellationToken cancellationToken) => builder.Append("switch(")
        .Append(value, cancellationToken)
        .AppendLine(')')
        .AppendLine('{')
        .AppendJoin(
            values,
            (b, v, t) => b.Append("case ").Append(@case, v, t).AppendLine(':').AppendLine('{').Append(body, v, t).AppendLine('}'),
            cancellationToken)
        .Append("default:{")
        .Append(defaultBody, cancellationToken)
        .AppendLine('}')
        .AppendLine('}');
    public static IExpandingMacroStringBuilder<Macro> AppendTypeSwitchStatement<T>(
    this IExpandingMacroStringBuilder<Macro> builder,
    IReadOnlyCollection<T> values,
    Appendix<Macro> valueTypeExpression,
    Func<T, RepresentableTypeNames> caseSelector,
    Appendix<Macro, T> body,
    Appendix<Macro> defaultBody,
    CancellationToken cancellationToken) =>
    builder.AppendSwitchStatement(
        values,
        valueTypeExpression,
        (b, v, t) => b.Append('"').Append(caseSelector.Invoke(v).FullTypeName).Append('"'),
        body,
        defaultBody,
        cancellationToken);

    public static IExpandingMacroStringBuilder<Macro> AppendUnsafeConvert(
        this IExpandingMacroStringBuilder<Macro> builder,
        String tFrom,
        String tTo,
        String valueExpression,
        CancellationToken _) =>
        builder.Append("(global::RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.UnsafeConvert<")
        .Append(tFrom)
        .Append(", ")
        .Append(tTo)
        .Append(">(")
        .Append(valueExpression)
        .Append("))");
    public static IExpandingMacroStringBuilder<Macro> AppendFullString(
        this IExpandingMacroStringBuilder<Macro> builder,
        Appendix<Macro> typeExpression,
        CancellationToken cancellationToken) =>
        builder.Append($"(global::RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.GetFullString(").Append(typeExpression, cancellationToken).Append("))");
    public static IExpandingMacroStringBuilder<Macro> AppendInvalidConversionThrow(
        this IExpandingMacroStringBuilder<Macro> builder,
        String typeName,
        CancellationToken _) =>
        builder.Append("throw new global::System.InvalidOperationException($\"The union type instance cannot be converted to an instance of {").Append(typeName).Append("}.\")");
    public static IExpandingMacroStringBuilder<Macro> AppendInvalidCreationThrow(
        this IExpandingMacroStringBuilder<Macro> builder,
        String unionTypeName,
        String valueName,
        CancellationToken _) =>
        builder.Append("throw new global::System.ArgumentException($\"The value provided for \\\"")
            .Append(valueName).Append("\\\" cannot be converted to an instance of {")
            .Append(unionTypeName).Append("}.\", \"").Append(valueName).Append("\")");

    public static IExpandingMacroStringBuilder<Macro> AppendUnknownConversion(
    this IExpandingMacroStringBuilder<Macro> builder,
    UnionDataModel targetUnionType,
    RepresentableTypeModel sourceData,
    RepresentableTypeModel targetData,
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
    public static IExpandingMacroStringBuilder<Macro> AppendKnownConversion(
        this IExpandingMacroStringBuilder<Macro> builder,
        UnionDataModel targetUnionType,
        RepresentableTypeModel sourceData,
        RepresentableTypeModel targetData,
        String parameterName,
        CancellationToken cancellationToken) =>
        builder.Append('(')
            .AppendFull(targetUnionType.Symbol)
            .Append('.')
            .Append(targetData.Names.CreateFromFunctionName)
            .Append('(')
            .Append(sourceData.Storage.TypesafeInstanceVariableExpressionAppendix, parameterName, cancellationToken)
            .Append("))");
}
