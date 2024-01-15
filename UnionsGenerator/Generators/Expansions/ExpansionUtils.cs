namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

static class ExpansionUtils
{
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
    _ = builder * '(' * targetUnionType.Symbol.ToFullOpenString() * '.' * targetData.Names.GeneratedFactoryName * '(' * parameterName * '.' * sourceData.Names.AsPropertyName * "))";
    public static void KnownConversion(
        ExpandingMacroBuilder builder,
        UnionDataModel targetUnionType,
        RepresentableTypeModel sourceData,
        RepresentableTypeModel targetData,
        String parameterName) =>
        _ = builder * '(' * targetUnionType.Symbol.ToFullOpenString() * '.' * targetData.Names.GeneratedFactoryName *
        '(' * (sourceData.Storage.TypesafeInstanceVariableExpression, parameterName) * "))";
}