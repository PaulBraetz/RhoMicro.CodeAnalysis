namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class Factories(TargetDataModel model) : ExpansionBase(model, Macro.Factories)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder /
            "#region Factories"
            .AppendJoin(
                representableTypes,
                (b, a, t) => b *
                    "public static ").AppendOpen(target).Append(" Create(").AppendFull(a).AppendLine(" value) => new(value);" /
                    "/// <summary>" *
                    "/// Creates a new instance of ").AppendCommentRef(target).AppendLine("." /
                    "/// </summary>" *
                    "public static ".AppendOpen(target).Append(' ').Append(a.Names.CreateFromFunctionName).Append('(')
                    .AppendFull(a).AppendLine(" value) => new(value);"),
                cancellationToken) *
            "public static Boolean TryCreate<"
            .Append(settings.GenericTValueName) *
            ">("
            .Append(settings.GenericTValueName) *
            " value, out ").AppendOpen(target).AppendLine(" instance){"
            .AppendTypeSwitchStatement(
                representableTypes,
                (b, t) => b.AppendFullString((b, t) => b.Append("typeof(").Append(settings.GenericTValueName).Append(')'), t),
                static t => t.Names,
                (b, v, t) => b.Append("instance = new(")
                    .AppendUnsafeConvert(settings.GenericTValueName, v.Names.FullTypeName, "value", t) *
                    ");return true;",
                static (b, t) => b.Append("instance = default; return false;"),
                cancellationToken)
            .AppendLine('}') *
            "public static ").AppendOpen(target).AppendLine(" Create<"
            .Append(settings.GenericTValueName) *
            ">("
            .Append(settings.GenericTValueName) /
            " value){"
            .AppendTypeSwitchStatement(
                representableTypes,
                (b, t) => b.AppendFullString((b, t) => b.Append("typeof(").Append(settings.GenericTValueName).Append(')'), t),
                t => t.Names,
                (b, v, t) => b.Append("return new(").AppendUnsafeConvert(settings.GenericTValueName, v.Names.FullTypeName, "value", t).Append(");"),
                (b, t) => b.AppendInvalidCreationThrow($"\"{target.ToFullOpenString()}\"", "value", t).Append(';'),
                cancellationToken)
            .AppendLine('}') /
            "#endregion";
    }
}
