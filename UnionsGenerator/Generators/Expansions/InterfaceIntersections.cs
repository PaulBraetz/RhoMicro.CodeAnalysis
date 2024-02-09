namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

internal sealed class InterfaceIntersections(TargetDataModel model) : ExpansionBase(model, Macro.InterfaceIntersections)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        //var intersections = Model.Annotations.AllRepresentableTypes
        //    .SelectMany(t =>
        //    {
        //        var name = t.Names.FullTypeName;
        //        var symbol = t.Attribute.RepresentableTypeSymbol;
        //        var interfaces = symbol?.AllInterfaces ?? ImmutableArray.Create<INamedTypeSymbol>();
        //        var result = interfaces.Select(@interface => (name, @interface));

        //        return result;
        //    })
        //    .Where(t => t.@interface.TypeArguments.Length == t.@interface.TypeParameters.Length)
        //    .GroupBy(t => t.@interface.MetadataName)
        //    .Where(g => g.Count() == Model.Annotations.AllRepresentableTypes.Count)
        //    .Select(g => g.First().@interface)
        //    .ToList();

        //if(intersections.Count == 0)
        //    return;

        //_ = builder %
        //    "#region Interface Intersections";

        //foreach(var intersection in intersections)
        //{
        //    var members = intersection.GetMembers();
        //    var methods = members.OfType<IMethodSymbol>();
        //    var properties = members.OfType<IPropertySymbol>();

        //    foreach(var method in methods)
        //    {
        //        _ = builder *
        //            "private " * method.ReturnType.ToFullOpenString() * ' ' *
        //            intersection.ToFullOpenString() * '.' * method.ToFullOpenString() *
        //            '(' * (b=>method.Parameters.Aggregate(b, (b,p)=>b * p.RefKind)) * ')';

        //    }
        //}
    }
}
