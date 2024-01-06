namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

sealed class Equals(TargetDataModel model) : ExpansionBase(model, Macro.Equals)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var target = Model.Symbol;
        var attributes = Model.Annotations;

        _ = builder *
            "#region Equality" /
            Docs.Inherit *
            "public override Boolean Equals(Object obj) => obj is " * target.ToMinimalOpenString() * " union && Equals(union);";

        if(ImplementsEquals())
        {
            _ = builder % String.Empty %
                "#endregion";

            return;
        }

        _ = builder /
            Docs.Inherit *
            "public Boolean Equals(" * target.ToMinimalOpenString() % " obj) =>";

        if(target.IsReferenceType)
            _ = builder * " obj != null && ";

        if(attributes.AllRepresentableTypes.Count > 1)
        {
            _ = (builder % " __tag == obj.__tag && __tag switch{")
                .AppendJoin(
                    attributes.AllRepresentableTypes,
                    (b, a, t) => b.WithOperators(builder.CancellationToken) *
                        a.GetCorrespondingTag(Model) * " => " * (a.Storage.EqualsInvocation, "obj") % ',',
                    builder.CancellationToken).WithOperators(builder.CancellationToken) *
                "_ => " * ConstantSources.InvalidTagStateThrow * '}';
        } else if(attributes.AllRepresentableTypes.Count == 1)
        {
            _ = builder * (attributes.AllRepresentableTypes[0].Storage.EqualsInvocation, "obj");
        }

        _ = builder % ';';

        if(Model.Symbol.IsValueType)
        {
            _ = builder * "public static Boolean operator ==(" * Model.Symbol.ToMinimalOpenString() * " a, " * Model.Symbol.ToMinimalOpenString() * " b) => a.Equals(b);" /
                "public static Boolean operator !=(" * Model.Symbol.ToMinimalOpenString() * " a, " * Model.Symbol.ToMinimalOpenString() / " b) => !(a == b);";
        }

        _ = builder % "#endregion";
    }

    private Boolean ImplementsEquals()
    {
        var result = Model.Symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(s =>
                s.Name == nameof(Equals) &&
                s.Parameters.Length == 1 &&
                s.Parameters[0].Type.Equals(Model.Symbol, SymbolEqualityComparer.Default) &&
                Model.Symbol.Equals(s.ContainingSymbol, SymbolEqualityComparer.Default));

        return result;
    }
}
