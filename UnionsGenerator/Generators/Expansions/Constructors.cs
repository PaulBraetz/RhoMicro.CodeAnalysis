namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class Constructors(TargetDataModel model) : ExpansionBase(model, Macro.Constructors)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var target = Model.Symbol;
        var annotations = Model.Annotations;

        _ = builder.AppendLine("#region Constructors")
            .AppendJoin(annotations.AllRepresentableTypes, (b, e, t) =>
            {
                var accessibility = Model.GetSpecificAccessibility(e);

                _ = (b.GetOperators(builder.CancellationToken) +
                    accessibility + ' ' + target.Name + '(')
                    .AppendFull(e) /
                    " value){";

                if(annotations.AllRepresentableTypes.Count > 1)
                    _ = b.Append("__tag = ").Append(e.GetCorrespondingTag(Model)).AppendLine(';');
                var result = b.Append(
                    e.Storage.InstanceVariableAssignmentExpressionAppendix,
                    ("value", "this")
                    , builder.CancellationToken) /
                    ";}";

                return result;
            }, builder.CancellationToken).AppendLine("#endregion");
    }
}
