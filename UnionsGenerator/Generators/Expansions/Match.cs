namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

internal sealed class Match(TargetDataModel model) : ExpansionBase(model, Macro.Match)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        //commented out because of breaking rewrite changes
        //var representableTypes = Model.Annotations.AllRepresentableTypes;
        //var target = Model.Symbol;
        //var settings = Model.Annotations.Settings;

        //_ = builder * "#region Match" /
        //    "public " * settings.MatchTypeName * " Match<" * settings.MatchTypeName * ">(" *
        //    (b => b.AppendJoin(
        //        ',',
        //        representableTypes,
        //        (b, a, t) => b.WithOperators(t) *
        //            "System.Func<" * a.Names.FullTypeName * ", " * settings.MatchTypeName * "> on" * a.Names.SafeAlias,
        //        b.CancellationToken)) *
        //    ") =>";

        //_ = representableTypes.Count == 1 ?
        //    builder * "on" * representableTypes[0].Names.SafeAlias * ".Invoke(" *
        //    representableTypes[0].Storage.TypesafeInstanceVariableExpression % ");" :
        //    builder * "__tag switch{" *
        //        (b => b.AppendJoin(
        //            representableTypes,
        //            (b, a, t) => b.WithOperators(t) *
        //                a.GetCorrespondingTag(Model) * " => " * "on" * a.Names.SafeAlias * ".Invoke(" *
        //                a.Storage.TypesafeInstanceVariableExpression % "),",
        //        b.CancellationToken)) /
        //        "_ =>" * ConstantSources.InvalidTagStateThrow % "};";

        //_ = builder % "#endregion";
    }
}
