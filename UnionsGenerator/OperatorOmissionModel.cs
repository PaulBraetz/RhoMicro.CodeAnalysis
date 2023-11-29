namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;

sealed class OperatorOmissionModel
{
    public readonly HashSet<RepresentableTypeData> Interfaces;
    public readonly HashSet<RepresentableTypeData> Supertypes;
    public readonly HashSet<RepresentableTypeData> AllOmissions;

    private OperatorOmissionModel(
        HashSet<RepresentableTypeData> interfaces,
        HashSet<RepresentableTypeData> supertypes,
        HashSet<RepresentableTypeData> allOmissions)
    {
        Interfaces = interfaces;
        Supertypes = supertypes;
        AllOmissions = allOmissions;
    }

    public static OperatorOmissionModel Create(ITypeSymbol target, AnnotationDataModel attributes)
    {
        var interfaces = new HashSet<RepresentableTypeData>();
        var supertypes = new HashSet<RepresentableTypeData>();
        var allOmissions = new HashSet<RepresentableTypeData>();

        var concreteAttributes = attributes.AllRepresentableTypes.Where(a => !a.Attribute.RepresentableTypeIsGenericParameter);

        foreach(var attribute in concreteAttributes)
        {
            if(target.InheritsFrom(attribute.Attribute.RepresentableTypeSymbol!))
            {
                _ = allOmissions.Add(attribute);
                _ = supertypes.Add(attribute);
            }

            if(attribute.Attribute.RepresentableTypeSymbol!.TypeKind == TypeKind.Interface)
            {
                _ = allOmissions.Add(attribute);
                _ = interfaces.Add(attribute);
            }
        }

        var result = new OperatorOmissionModel(interfaces, supertypes, allOmissions);

        return result;
    }
}
