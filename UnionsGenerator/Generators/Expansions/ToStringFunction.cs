namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

static class ToStringFunction
{
    private static void DetailedImpl(ExpandingMacroBuilder builder, TargetDataModel model)
    {
        var target = model.Symbol;
        var attributes = model.Annotations.AllRepresentableTypes;

        _ = builder *
            "#region ToString" /
            "#nullable enable" /
            (Docs.Summary, "Returns a string representation of the current instance.") *
            "public override String? ToString(){var stringRepresentation = " *
            (SimpleToStringExpressionAppendix, model);

        _ = builder * "; var result = $\"" * target.Name * '(';

        _ = (attributes.Count == 1 ?
            builder * '<' * attributes[0].Names.SimpleTypeName * '>' :
            builder * (b => b.AppendJoin(
                '|',
                attributes,
                (b, a, t) => b.WithOperators(t) *
                    "{(" * "__tag == " * a.GetCorrespondingTag(model) * '?' *
                    "\"<" * a.Names.SafeAlias * ">\"" * ':' *
                    '\"' * a.Names.SafeAlias * "\")}",
                builder.CancellationToken))) %
            "){{{stringRepresentation}}}\"; return result;}" %
            "#nullable restore" %
            "#endregion";
    }
    private static void SimpleImpl(ExpandingMacroBuilder builder, TargetDataModel model)
    {
        _ = builder *
            "#nullable enable" /
            (Docs.Summary, "Returns a string representation of the current instance.") *
            "public override String? ToString() => ";

        SimpleToStringExpressionAppendix(builder, model);

        _ = builder % ';' % "#nullable restore";
    }

    private static void SimpleToStringExpressionAppendix(ExpandingMacroBuilder builder, TargetDataModel model)
    {
        var attributes = model.Annotations;

        _ = attributes.AllRepresentableTypes.Count == 1 ?
            builder * attributes.AllRepresentableTypes[0].Storage.ToStringInvocation :
            builder * "__tag switch{" *
                (b => b.AppendJoin(
                    attributes.AllRepresentableTypes,
                    (b, a, t) => b.WithOperators(t) *
                        a.GetCorrespondingTag(model) * " => " * a.Storage.ToStringInvocation * ',',
                b.CancellationToken)) *
                "_ => " * ConstantSources.InvalidTagStateThrow / '}';
    }

    public static IMacroExpansion<Macro> Create(TargetDataModel model)
    {
        Action<ExpandingMacroBuilder, TargetDataModel> appendix = model.Annotations.Settings.ToStringSetting switch
        {
            ToStringSetting.Simple => SimpleImpl,
            ToStringSetting.Detailed => DetailedImpl,
            _ => static (b, m) => { } //append none
        };
        var result = MacroExpansion.Create(Macro.ToString, (b, t) => appendix.Invoke(b.WithOperators(t), model));

        return result;
    }
}
