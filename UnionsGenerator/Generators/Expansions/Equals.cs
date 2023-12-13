namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class Equals(TargetDataModel model) : ExpansionBase(model, Macro.Equals)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        var target = Model.Symbol;
        var attributes = Model.Annotations;

        _ = builder /
            "#region Equality" *
            "public override Boolean Equals(Object obj) => obj is "
            .AppendOpen(target).AppendLine(" union && Equals(union);") *
            "public Boolean Equals("
            .AppendOpen(target).AppendLine(" obj) =>");

        if(target.IsReferenceType)
            _ = builder.Append(" obj != null && ");

        if(attributes.AllRepresentableTypes.Count > 1)
        {
            _ = builder.AppendLine(" __tag == obj.__tag && __tag switch{")
                .AppendJoin(
                    attributes.AllRepresentableTypes,
                    (b, a, t) => b.Append(a.CorrespondingTag) *
                        " => "
                        .Append(a.Storage.EqualsInvocationAppendix, "obj", t)
                        .AppendLine(','),
                    cancellationToken) *
                "_ => ".Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');
        } else if(attributes.AllRepresentableTypes.Count == 1)
        {
            _ = builder.Append(attributes.AllRepresentableTypes[0].Storage.EqualsInvocationAppendix, "obj", cancellationToken);
        }

        _ = builder.AppendLine(';');

        if(Model.Symbol.IsValueType)
        {
            _ = builder.Append("public static Boolean operator ==(")
                .Append(Model.Symbol.Name) *
                " a, "
                .Append(Model.Symbol.Name) /
                " b) => a.Equals(b);" *
                "public static Boolean operator !=("
                .Append(Model.Symbol.Name) *
                " a, "
                .Append(Model.Symbol.Name) /
                " b) => !(a == b);";
        }

        _ = builder.AppendLine("#endregion");
    }
}
