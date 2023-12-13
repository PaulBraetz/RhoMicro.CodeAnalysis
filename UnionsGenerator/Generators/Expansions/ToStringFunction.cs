namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

static class ToStringFunction
{
    private static void AppendDetailed(ExpandingMacroBuilder builder, TargetDataModel model)
    {
        var target = model.Symbol;
        var attributes = model.Annotations.AllRepresentableTypes;

        _ = builder -
            "#region ToString" -
             "#nullable enable" +
             (Extensions.DocCommentAppendix, "Returns a string representation of the current instance.") -
            "public override String? ToString(){var stringRepresentation = " +
            (SimpleToStringExpressionAppendix, model);

        _ = builder * "; var result = $\"" * target.Name * '(';

        _ = (attributes.Count == 1 ?
            builder * '<' * attributes[0].Names.SimpleTypeName * '>' :
            builder.AppendJoin(
                " | ",
                attributes,
                (b, a, t) => b.GetOperators(t) *
                    "{(" *
                    "__tag == " *
                    a.GetCorrespondingTag(model) *
                    '?' *
                    "\"<").Append(a.Names.SafeAlias).Append(">\"".Append(':')
                    .Append('\"').Append(a.Names.SafeAlias).Append("\")}"),
                cancellationToken)) *
            "){{{stringRepresentation}}}\"; return result;}" /
            "#nullable restore" /
            "#endregion";
    }
    private static void AppendSimple(IExpandingMacroStringBuilder<Macro> builder, TargetDataModel model, CancellationToken cancellationToken)
    {
        _ = builder.Append("#nullable enable") /
            "/// <summary>" /
            "/// Returns a string representation of the current instance." /
            "/// </summary>" *
            "public override String? ToString() => ";

        SimpleToStringExpressionAppendix(builder, model, cancellationToken);

        _ = builder.AppendLine(';') /
            "#nullable restore";
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
                (b, a, t) => b.Append(a.CorrespondingTag) *
                    " => ".Append(a.Storage.ToStringInvocationAppendix, t)
                    .AppendLine(','),
                cancellationToken) *
                "_ => ".AppendLine(ConstantSources.InvalidTagStateThrow)
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
