namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

static class ToStringExpansion
{
    private static void AppendDetailed(IExpandingMacroStringBuilder<Macro> builder, TargetDataModel model, CancellationToken cancellationToken)
    {
        var target = model.Symbol;
        var attributes = model.Annotations.AllRepresentableTypes;

        _ = builder
            .AppendLine("#region ToString")
             .AppendLine("#nullable enable")
            .AppendLine("/// <summary>")
            .AppendLine("/// Returns a string representation of the current instance.")
            .AppendLine("/// </summary>")
            .Append("public override String? ToString(){var stringRepresentation = ")
            .Append(SimpleToStringExpressionAppendix, model, cancellationToken);

        _ = builder
            .Append("; var result = $\"")
            .Append(target.Name)
            .Append('(');

        _ = (attributes.Count == 1 ?
            builder.Append('<').Append(attributes[0].Names.SimpleTypeName).Append('>') :
            builder.AppendJoin(
                " | ",
                attributes,
                (b, a, t) =>
                    b.Append("{(").Append("__tag == ").Append(a.CorrespondingTag).Append('?')
                    .Append("\"<").Append(a.Names.SafeAlias).Append(">\"").Append(':')
                    .Append('\"').Append(a.Names.SafeAlias).Append("\")}"),
                cancellationToken))
            .Append("){{{stringRepresentation}}}\"; return result;}")
            .AppendLine("#nullable restore")
            .AppendLine("#endregion");
    }
    private static void AppendSimple(IExpandingMacroStringBuilder<Macro> builder, TargetDataModel model, CancellationToken cancellationToken)
    {
        _ = builder.Append("#nullable enable")
            .AppendLine("/// <summary>")
            .AppendLine("/// Returns a string representation of the current instance.")
            .AppendLine("/// </summary>")
            .Append("public override String? ToString() => ");

        SimpleToStringExpressionAppendix(builder, model, cancellationToken);

        _ = builder.AppendLine(';')
            .AppendLine("#nullable restore");
    }
    private static void AppendNone(IExpandingMacroStringBuilder<Macro> _0, TargetDataModel _1, CancellationToken _2) { }

    private static void SimpleToStringExpressionAppendix(IExpandingMacroStringBuilder<Macro> builder, TargetDataModel model, CancellationToken cancellationToken)
    {
        var attributes = model.Annotations;

        _ = attributes.AllRepresentableTypes.Count == 1 ?
            builder.Append(attributes.AllRepresentableTypes[0].Storage.ToStringInvocationAppendix, cancellationToken) :
            builder.Append("__tag switch{")
                .AppendJoin(
                attributes.AllRepresentableTypes,
                (b, a, t) => b.Append(a.CorrespondingTag)
                    .Append(" => ").Append(a.Storage.ToStringInvocationAppendix, t)
                    .AppendLine(','),
                cancellationToken)
                .Append("_ => ").AppendLine(ConstantSources.InvalidTagStateThrow)
                .AppendLine('}');
    }

    public static IMacroExpansion<Macro> Create(TargetDataModel model)
    {
        Appendix<Macro, TargetDataModel> appendix = model.Annotations.Settings.ToStringSetting switch
        {
            ToStringSetting.Simple => AppendSimple,
            ToStringSetting.Detailed => AppendDetailed,
            _ => AppendNone
        };
        var result = MacroExpansion.Create(Macro.ToString, (b, t) => appendix.Invoke(b, model, t));

        return result;
    }
}
