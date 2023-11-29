namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using Microsoft.CodeAnalysis;

using System;
using System.Linq;
using System.Text;

readonly struct FactoryFunctionsModel
{
    public readonly String SourceText;
    private FactoryFunctionsModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<FactoryFunctionsModel> context) =>
        context.Source.SetFactoryFunctions(context.Model);

    private static FactoryFunctionsModel Create(ModelCreationContext context)
    {
        var representableTypes = context.TargetData.Annotations.AllRepresentableTypes;
        var target = context.TargetData.Symbol;
        var settings = context.TargetData.Annotations.Settings;

        var sourceText = new StringBuilder()
            .AppendAggregate(
                representableTypes,
                (b, a) => b.AppendLine(
                    $$"""
                    /// </inheritdoc>
                    public static {{target.ToOpenString()}} Create({{a.Names.FullTypeName}} value) => new(value);
                    /// <summary>
                    /// Creates a new instance of <see cref="{{target.ToDocCompatString()}}"/>.
                    /// </summary>
                    public static {{target.ToOpenString()}} {{a.Names.CreateFromFunctionName}}({{a.Names.FullTypeName}} value) => new(value);
                    """))
            .AppendLine("/// </inheritdoc>")
            .Append("public static Boolean TryCreate<")
            .Append(settings.GenericTValueName)
            .Append(">(")
            .Append(settings.GenericTValueName)
            .Append(" value, out ").AppendOpen(target).AppendLine(" instance){")
            .AppendTypeSwitchStatement(
                representableTypes,
                ConstantSources.GetFullString($"typeof({settings.GenericTValueName})"),
                t => t.Names,
                (b, t) => b.Append("instance = new(")
                    .Append(ConstantSources.UnsafeConvert(settings.GenericTValueName, t.Names.FullTypeName, "value"))
                    .Append(");return true;"),
                static b => b.Append("instance = default; return false;"))
            .AppendLine("}")
            .AppendLine("/// </inheritdoc>")
            .Append("public static ").AppendOpen(target).AppendLine(" Create<")
            .Append(settings.GenericTValueName)
            .Append(">(")
            .Append(settings.GenericTValueName)
            .AppendLine(" value){")
            .AppendTypeSwitchStatement(
                representableTypes,
                ConstantSources.GetFullString($"typeof({settings.GenericTValueName})"),
                t => t.Names,
                (b, t) => b.Append("return new(")
                    .Append(ConstantSources.UnsafeConvert(settings.GenericTValueName, t.Names.FullTypeName, "value"))
                    .Append(");"),
                b => b.Append(ConstantSources.InvalidCreationThrow($"\"{target.ToOpenString()}\"", "value")).Append(';'))
            .AppendLine("}")
            .ToString();

        var result = new FactoryFunctionsModel(sourceText);

        return result;
    }
}
