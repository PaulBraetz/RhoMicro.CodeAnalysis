namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System;
using System.Threading;

internal static class ToStringFunction
{
    private sealed class SimpleImplementation(TargetDataModel model) : ExpansionBase(model, Macro.ToString)
    {
        protected override void Expand(ExpandingMacroBuilder builder)
        {
            _ = builder *
            "#region ToString" /
            "#nullable enable" /
            (Docs.Summary, b => _ = b * "Returns a string representation of the current instance.") *
            "public override String? ToString() => ";

            SimpleToStringExpressionAppendix(builder, Model);

            _ = builder % ';' % "#nullable restore" % "#endregion";
        }
    }

    private sealed class DetailedImplementation(TargetDataModel model) : ExpansionBase(model, Macro.ToString)
    {
        protected override void Expand(ExpandingMacroBuilder builder)
        {
            var target = Model.Symbol;
            var attributes = Model.Annotations.AllRepresentableTypes;

            _ = builder *
                "#region ToString" /
                "#nullable enable" /
                (Docs.Summary, b => _ = b * "Returns a string representation of the current instance.") *
                "public override String? ToString(){var stringRepresentation = " *
                (SimpleToStringExpressionAppendix, Model);

            _ = builder * "; var result = $\"" * target.Name * '(';

            _ = ( attributes.Count == 1 ?
                builder * '<' * attributes[0].Names.SimpleTypeName * '>' :
                builder * ( b => b.AppendJoin(
                    '|',
                    attributes,
                    (IExpandingMacroStringBuilder<Macro> b, RepresentableTypeModel a, CancellationToken t) => b.WithOperators(t) *
                        "{(" * "__tag == " * a.GetCorrespondingTag(Model) * '?' *
                        "\"<" * a.Names.SafeAlias * ">\"" * ':' *
                        '\"' * a.Names.SafeAlias * "\")}",
                    builder.CancellationToken) ) ) %
                "){{{stringRepresentation}}}\"; return result;}" %
                "#nullable restore" %
                "#endregion";
        }
    }

    private sealed class NoImplementation : IMacroExpansion<Macro>
    {
        private NoImplementation() { }
        public static NoImplementation Instance = new();

        public Macro Macro { get; } = Macro.ToString;

        public void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
        {
            //_ = builder.WithOperators(cancellationToken) %
            //    "#region ToString" %
            //    "//no implementation" %
            //    "#endregion";
        }
    }
    private static void SimpleToStringExpressionAppendix(ExpandingMacroBuilder builder, TargetDataModel model)
    {
        var attributes = model.Annotations;

        _ = attributes.AllRepresentableTypes.Count == 1 ?
            builder * attributes.AllRepresentableTypes[0].Storage.ToStringInvocation :
            builder * "__tag switch{" *
                ( b => b.AppendJoin(
                    attributes.AllRepresentableTypes,
                    (b, a, t) => b.WithOperators(t) *
                        a.GetCorrespondingTag(model) * " => " * a.Storage.ToStringInvocation * ',',
                b.CancellationToken) ) *
                "_ => " * ConstantSources.InvalidTagStateThrow / '}';
    }

    public static IMacroExpansion<Macro> Create(TargetDataModel _)
    {
        IMacroExpansion<Macro> result = NoImplementation.Instance;
        //commented out because of breaking rewrite changes
        //ImplementsToString(model) ?
        //NoImplementation.Instance :
        //model.Annotations.Settings.ToStringSetting switch
        //{
        //    ToStringSetting.Simple => new SimpleImplementation(model),
        //    ToStringSetting.Detailed => new DetailedImplementation(model),
        //    _ => NoImplementation.Instance
        //};

        return result;
    }

    private static Boolean ImplementsToString(TargetDataModel model)
    {
        var result = model.Symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(s => s.IsOverride &&
                s.Name == nameof(ToString) &&
                s.Parameters.Length == 0);

        return result;
    }
}
