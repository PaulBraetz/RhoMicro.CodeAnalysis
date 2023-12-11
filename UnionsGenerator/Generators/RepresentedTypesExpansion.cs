namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class RepresentedTypesExpansion(TargetDataModel model) : ExpansionBase(model, Macro.RepresentedTypes)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        _ = builder.AppendLine("#region GetRepresentedType")
            .AppendLine(
            """
            /// <summary>
            /// Gets types of value this union type can represent.
            /// </summary>
            public static global::System.Collections.Generic.IReadOnlyList<Type> RepresentableTypes { get; } = 
                new global::System.Type[]
                {
            """)
            .AppendJoin(
                ",",
                attributes,
                (b, a, t) => b.Append("typeof(").Append(a.Names.FullTypeName).Append(')'),
                cancellationToken)
            .AppendLine("};")
            .AppendLine(
            """
            /// <summary>
            /// Gets type of value represented by this instance.
            /// </summary>
            public Type RepresentedType => 
            """);

#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count == 1)
        {
            _ = builder.Append("typeof(")
                .AppendFull(attributes[0])
                .AppendLine(");");
        } else
        {
            _ = builder.AppendLine("__tag switch {")
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"),
                    cancellationToken)
                .Append("_ => ").AppendLine(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        _ = builder.AppendLine("#endregion");
    }
}
