namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.Library;

sealed class IsAsFunctions(TargetDataModel model) : ExpansionBase(model, Macro.IsAsFunctions)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;
        _ = builder * "#region IsAsFunctions" /
            (b => Docs.MethodSummary(b,
            summary: b => _ = b * "Determines whether this instance is representing a value of type <typeparamref name=\"" * settings.GenericTValueName * "\"/>.",
            parameters: [],
            typeParameters: [(Name: settings.GenericTValueName, Summary: b => _ = b * "The type whose representation in this instance to determine.")],
            returns: b => _ = b * "<see langword=\"true\"/> if this instance is representing a value of type <typeparamref name=\"" * settings.GenericTValueName * "\"/>; otherwise, <see langword=\"false\"/>.")) *
            "public global::System.Boolean Is<" * settings.GenericTValueName * ">() => ";

#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder * "typeof(" * settings.GenericTValueName * ") == __tag switch {" *
                (b => b.AppendJoin(
                    attributes,
                    (b, a, t) => b.WithOperators(builder.CancellationToken) *
                        a.GetCorrespondingTag(Model) * " => typeof(" * a.Names.FullTypeName % "),",
                    builder.CancellationToken)) /
                "_ => " * ConstantSources.InvalidTagStateThrow /
                "};";
        } else
        {
            _ = builder * "typeof(" * settings.GenericTValueName * ") == typeof(" * attributes[0].Names.FullTypeName % ");";
        }

        _ = builder *
            (b => Docs.MethodSummary(b,
            summary: b => _ = b * "Determines whether this instance is representing a value of type <typeparamref name=\"" * settings.GenericTValueName * "\"/>.",
            parameters: [(Name: "value", Summary: b => _ = b * "If this instance is representing a value of type <typeparamref name=\"" * settings.GenericTValueName * "\"/>, this parameter will contain that value; otherwise, <see langword=\"default\"/>.")],
            typeParameters: [(Name: settings.GenericTValueName, Summary: b => _ = b * "The type whose representation in this instance to determine.")],
            returns: b => _ = b * "<see langword=\"true\"/> if this instance is representing a value of type <typeparamref name=\"" * settings.GenericTValueName * "\"/>; otherwise, <see langword=\"false\"/>.")) /
            "public global::System.Boolean Is<" * settings.GenericTValueName * ">(out " * settings.GenericTValueName * "? value){";

        if(attributes.Count > 1)
        {
            _ = builder *
                (b => SwitchStatement(
                    b,
                    attributes,
                    (b) => _ = b * "__tag",
                    (b, a) => _ = b *
                        a.GetCorrespondingTag(Model) * " when typeof(" * settings.GenericTValueName * ") == typeof(" * a.Names.FullTypeName * ')',
                    (b, a) => _ = b *
                        "value = " * (a.Storage.ConvertedInstanceVariableExpression, settings.GenericTValueName) * ';' * "return true;",
                    (b) => _ = b *
                        "value = default; return false;")) /
                "}";
        } else
        {
            _ = builder * "if(typeof(" * settings.GenericTValueName * ") == typeof(" * attributes[0].Names.FullTypeName *
                ")){value = " * (attributes[0].Storage.ConvertedInstanceVariableExpression, settings.GenericTValueName) /
                "; return true;} value = default; return false;}";
        }

        _ = builder *
            (b => Docs.MethodSummary(b,
            summary: b => _ = b * "Determines whether this instance is representing a value of the type provided.",
            parameters: [(Name: "type", Summary: b => _ = b * "The type whose representation in this instance to determine.")],
            returns: b => _ = b * "<see langword=\"true\"/> if this instance is representing a value of the type provided; otherwise, <see langword=\"false\"/>.")) /
            "public global::System.Boolean Is(Type type) => ";
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder * "type == __tag switch {" *
                (b => b.AppendJoin(
                    attributes,
                    (b, a, t) => b.WithOperators(builder.CancellationToken) *
                        a.GetCorrespondingTag(Model) * " => typeof(" * a.Names.FullTypeName * "),",
                    b.CancellationToken)) /
                "_ => " * ConstantSources.InvalidTagStateThrow %
                "};";
        } else
        {
            _ = builder * "type == typeof(" * attributes[0].Names.FullTypeName % ");";
        }

        _ = builder * "public " * settings.GenericTValueName * " As<" * settings.GenericTValueName * ">() => ";

        if(attributes.Count > 1)
        {
            _ = builder * "__tag switch {" *
                (b => b.AppendJoin(
                    attributes,
                    (b, a, t) => b.WithOperators(builder.CancellationToken) *
                        a.GetCorrespondingTag(Model) * " => typeof(" * settings.GenericTValueName * ") == typeof(" *
                        a.Names.FullTypeName * ")?" * (a.Storage.ConvertedInstanceVariableExpression, settings.GenericTValueName) *
                        ':' * (InvalidConversionThrow, $"typeof({settings.GenericTValueName})") % ',',
                    b.CancellationToken)) /
                "_ => " * (InvalidConversionThrow, $"typeof({settings.GenericTValueName})") %
                "};";
        } else
        {
            _ = builder * "typeof(" * settings.GenericTValueName * ") == typeof(" *
                attributes[0].Names.FullTypeName * ")?" *
                (attributes[0].Storage.ConvertedInstanceVariableExpression, settings.GenericTValueName) *
                ':' * (InvalidConversionThrow, $"typeof({settings.GenericTValueName})") %
                ';';
        }

        _ = builder % "#endregion";
    }
}
