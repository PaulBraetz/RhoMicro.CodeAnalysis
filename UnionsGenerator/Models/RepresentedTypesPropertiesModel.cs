namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Text;

readonly struct RepresentedTypesPropertiesModel
{
    public readonly String SourceText;
    private RepresentedTypesPropertiesModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<RepresentedTypesPropertiesModel> context) =>
        context.Source.SetRepresentedTypesPropertiesFunction(context.Model);

    static RepresentedTypesPropertiesModel Create(ModelCreationContext context)
    {
        var attributes = context.TargetData.Annotations.AllRepresentableTypes;

        var sourceTextBuilder = new StringBuilder()
            .AppendLine(
            """
            /// <summary>
            /// Gets types of value this union type can represent.
            /// </summary>
            public static global::System.Collections.Generic.IReadOnlyList<Type> RepresentableTypes { get; } = 
                new global::System.Type[]
                {
            """)
            .AppendAggregateJoin(
                ",",
                attributes,
                (b, a) => b.Append("typeof(").Append(a.Names.FullTypeName).Append(')'))
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
            _ = sourceTextBuilder.Append("typeof(")
                .AppendFull(attributes[0])
                .AppendLine(");");
        } else
        {
            _ = sourceTextBuilder.AppendLine("__tag switch {")
                .AppendAggregate(
                    attributes,
                    (b, a) => b.Append(a.CorrespondingTag).Append(" => typeof(")
                        .AppendFull(a).AppendLine("),"))
                .Append("_ => ").AppendLine(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        }
#pragma warning restore IDE0045 // Convert to conditional expression

        var sourceText = sourceTextBuilder.ToString();
        var result = new RepresentedTypesPropertiesModel(sourceText);

        return result;
    }
}
