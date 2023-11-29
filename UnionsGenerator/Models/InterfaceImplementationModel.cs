namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Linq;
using System.Text;

readonly struct InterfaceImplementationModel
{
    private InterfaceImplementationModel(String sourceText) => SourceText = sourceText;

    public readonly String SourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<InterfaceImplementationModel> context) =>
        context.Source.SetInterfaceImplementation(context.Model);

    static InterfaceImplementationModel Create(ModelCreationContext context)
    {
        var attributes = context.TargetData.Annotations;
        var target = context.TargetData.Symbol;
        var targetDeclaration = context.TargetData.TargetDeclaration;

        var sourceTextBuilder = attributes.AllRepresentableTypes
            .Select((a, i) => (Name: a.Names.FullTypeName, Index: i))
            .Aggregate(
                new StringBuilder(": global::RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions.IUnion<").AppendOpen(target).Append(','),
                (b, n) => b.Append(n.Name).Append(n.Index != attributes.AllRepresentableTypes.Count - 1 ? "," : String.Empty))
            .AppendLine(">,")
            .Append("global::System.IEquatable<")
            .AppendOpen(target)
            .Append('>');

        if(!(attributes.AllRepresentableTypes.Count == 1 &&
             attributes.AllRepresentableTypes[0].Attribute.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary)))
        {
            var omissions = context.TargetData.OperatorOmissions.AllOmissions;
            _ = attributes.AllRepresentableTypes
                .Where(a => !omissions.Contains(a) &&
                            !a.Attribute.RepresentableTypeIsGenericParameter ||
                             a.Attribute.Options.HasFlag(UnionTypeOptions.SupersetOfParameter))
                .Select(a => a.Names.FullTypeName)
                .Aggregate(
                    sourceTextBuilder,
                    (b, n) => b.AppendLine(",")
                        .Append(" global::RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions.ISuperset<")
                        .Append(n)
                        .Append(',')
                        .AppendOpen(target)
                        .Append('>'));
        }

        var sourceText = sourceTextBuilder.ToString();

        var result = new InterfaceImplementationModel(sourceText);

        return result;
    }
}
