namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class FactoriesExpansion(TargetDataModel model) : ExpansionBase(model, Macro.Factories)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder
            .AppendLine("#region Factories")
            .AppendJoin(
                representableTypes,
                (b, a, t) => b
                    .AppendLine("/// </inheritdoc>")
                    .Append("public static ").AppendOpen(target).Append(" Create(").AppendFull(a).AppendLine(" value) => new(value);")
                    .AppendLine("/// <summary>")
                    .Append("/// Creates a new instance of <see cref=\"").AppendCommentRef(target).AppendLine("\"/>.")
                    .AppendLine("/// </summary>")
                    .Append("public static ").AppendOpen(target).Append(' ').Append(a.Names.CreateFromFunctionName).Append('(')
                    .AppendFull(a).AppendLine(" value) => new(value);"),
                cancellationToken)
            .AppendLine("/// </inheritdoc>")
            .Append("public static Boolean TryCreate<")
            .Append(settings.GenericTValueName)
            .Append(">(")
            .Append(settings.GenericTValueName)
            .Append(" value, out ").AppendOpen(target).AppendLine(" instance){")
            .AppendTypeSwitchStatement(
                representableTypes,
                (b, t) => b.AppendFullString((b, t) => b.Append("typeof(").Append(settings.GenericTValueName).Append(')'), t),
                static t => t.Names,
                (b, v, t) => b.Append("instance = new(")
                    .AppendUnsafeConvert(settings.GenericTValueName, v.Names.FullTypeName, "value", t)
                    .Append(");return true;"),
                static (b, t) => b.Append("instance = default; return false;"),
                cancellationToken)
            .AppendLine('}')
            .AppendLine("/// </inheritdoc>")
            .Append("public static ").AppendOpen(target).AppendLine(" Create<")
            .Append(settings.GenericTValueName)
            .Append(">(")
            .Append(settings.GenericTValueName)
            .AppendLine(" value){")
            .AppendTypeSwitchStatement(
                representableTypes,
                (b, t) => b.AppendFullString((b, t) => b.Append("typeof(").Append(settings.GenericTValueName).Append(')'), t),
                t => t.Names,
                (b, v, t) => b.Append("return new(").AppendUnsafeConvert(settings.GenericTValueName, v.Names.FullTypeName, "value", t).Append(");"),
                (b, t) => b.AppendInvalidCreationThrow($"\"{target.ToFullOpenString()}\"", "value", t).Append(';'),
                cancellationToken)
            .AppendLine('}')
            .AppendLine("#endregion");
    }
}
