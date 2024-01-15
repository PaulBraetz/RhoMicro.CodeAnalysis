namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class RepresentedTypes(TargetDataModel model) : ExpansionBase(model, Macro.RepresentedTypes)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        _ = builder * "#region GetRepresentedType" /
            (Docs.Summary, b => _ = b * "Gets the types of value this union type can represent.") *
            """
            public static global::System.Collections.Generic.IReadOnlyList<Type> RepresentableTypes { get; } = 
                new global::System.Type[]
                {
            """ /
            (b => b.AppendJoin(
                ',',
                attributes,
                (b, a, t) => b.WithOperators(t) *
                    "typeof(" * a.Names.FullTypeName * ')',
                b.CancellationToken)) /
            "};" /
            (Docs.Summary, b => _ = b * "Gets the type of value represented by this instance.") *
            "public Type RepresentedType => ";

#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count == 1)
        {
            _ = builder * "typeof(" * attributes[0].Names.FullTypeName * ");";
        } else
        {
            _ = builder * "__tag switch {" /
                (b => b.AppendJoin(
                    attributes,
                    (b, a, t) => b.WithOperators(t) *
                        a.GetCorrespondingTag(Model) * " => typeof(" * a.Names.FullTypeName % "),",
                    b.CancellationToken)) *
                "_ => " * ConstantSources.InvalidTagStateThrow /
                "};";
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        _ = builder % "#endregion";
    }
}
