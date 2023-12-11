namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class EqualsExpansion(TargetDataModel model) : ExpansionBase(model, Macro.Equals)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var target = Model.Symbol;
        var attributes = Model.Annotations;

        _ = builder
            .AppendLine("#region Equality")
            .Append("public override Boolean Equals(Object obj) => obj is ")
            .AppendOpen(target).AppendLine(" union && Equals(union);")
            .Append("public Boolean Equals(")
            .AppendOpen(target).AppendLine(" obj) =>");

        if(target.IsReferenceType)
            _ = builder.Append(" obj != null && ");

        if(attributes.AllRepresentableTypes.Count > 1)
        {
            _ = builder.AppendLine(" __tag == obj.__tag && __tag switch{")
                .AppendJoin(
                    attributes.AllRepresentableTypes,
                    (b, a, t) => b.Append(a.CorrespondingTag)
                        .Append(" => ")
                        .Append(a.Storage.EqualsInvocationAppendix, "obj" , t)
                        .AppendLine(','),
                    cancellationToken)
                .Append("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');
        } else if(attributes.AllRepresentableTypes.Count == 1)
        {
            _ = builder.Append(attributes.AllRepresentableTypes[0].Storage.EqualsInvocationAppendix, "obj", cancellationToken);
        }

        _ = builder.AppendLine(';');

        if(Model.Symbol.IsValueType)
        {
            _ = builder.Append("public static Boolean operator ==(")
                .Append(Model.Symbol.Name)
                .Append(" a, ")
                .Append(Model.Symbol.Name)
                .AppendLine(" b) => a.Equals(b);")
                .Append("public static Boolean operator !=(")
                .Append(Model.Symbol.Name)
                .Append(" a, ")
                .Append(Model.Symbol.Name)
                .AppendLine(" b) => !(a == b);");
        }

        _ = builder.AppendLine("#endregion");
    }
}
