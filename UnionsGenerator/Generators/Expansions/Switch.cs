namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;
using System.Linq;

sealed class Switch(TargetDataModel model) : ExpansionBase(model, Macro.Switch)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;

        _ = builder * "#region Switch" /
            (b => Docs.MethodSummary(
                builder: b,
                summary: b => _ = b * "Invokes a handler based on the type of value being represented.",
                parameters: representableTypes.Select<RepresentableTypeModel, (String Name, Action<ExpandingMacroBuilder> Summary)>(a => (
                    Name: a.Names.SafeAlias,
                    Summary: b => _ = b * "The handler to invoke if the union is currently representing an instance of " * a.CommentRef * '.')))) *
            (b => b.AppendJoin(
                representableTypes,
                (b, a, t) => b.WithOperators(t) *
                    "/// <param name=\"on" * a.Names.SafeAlias *
                    "\">" *
                    a.CommentRef * ".</param>",
                b.CancellationToken)) /
            "public void Switch(" *
            (b => b.AppendJoin(
                ',',
                representableTypes,
                (b, a, t) => b.WithOperators(t) *
                    "global::System.Action<" * a.Names.FullTypeName * "> on" * a.Names.SafeAlias,
                b.CancellationToken)) *
            "){";

        _ = representableTypes.Count == 1
            ? builder *
                "on" * representableTypes[0].Names.SafeAlias * ".Invoke(" * representableTypes[0].Storage.TypesafeInstanceVariableExpression * ')'
            : builder *
                "switch(__tag){" /
                (b => b.AppendJoin(
                    representableTypes,
                    (b, a, t) => b.WithOperators(t) *
                        "case " * a.GetCorrespondingTag(Model) * ':' /
                        "on" * a.Names.SafeAlias * ".Invoke(" * a.Storage.TypesafeInstanceVariableExpression % ");return;",
                    b.CancellationToken)) /
                "default:" * ConstantSources.InvalidTagStateThrow * ";}";

        _ = builder %
            ";}" %
            "#endregion";
    }
}
