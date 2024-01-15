namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System.Linq;

// ATTENTION: order influences type order => changes are breaking
enum RepresentableTypeNature
{
    ReferenceType,
    ImpureValueType,
    PureValueType,
    UnknownType
}
static class RepresentableTypeNatureFactory
{
    public static RepresentableTypeNature Create(UnionTypeAttribute attribute)
    {
        var representedSymbol = attribute.RepresentableTypeSymbol;

        if(representedSymbol == null)
            return RepresentableTypeNature.UnknownType;

        if(representedSymbol.IsPureValueType())
            return RepresentableTypeNature.PureValueType;

        if(representedSymbol.IsValueType)
            return RepresentableTypeNature.ImpureValueType;

        if(representedSymbol.IsReferenceType)
            return RepresentableTypeNature.ReferenceType;

        return RepresentableTypeNature.UnknownType;
    }
}
