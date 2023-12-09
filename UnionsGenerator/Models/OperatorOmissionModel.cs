namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using System.Collections.Generic;
using System.Linq;

sealed class OperatorOmissionModel
{
    public readonly HashSet<RepresentableTypeModel> Interfaces;
    public readonly HashSet<RepresentableTypeModel> Supertypes;
    public readonly HashSet<RepresentableTypeModel> AllOmissions;

    private OperatorOmissionModel(
        HashSet<RepresentableTypeModel> interfaces,
        HashSet<RepresentableTypeModel> supertypes,
        HashSet<RepresentableTypeModel> allOmissions)
    {
        Interfaces = interfaces;
        Supertypes = supertypes;
        AllOmissions = allOmissions;
    }

    public static OperatorOmissionModel Create(ITypeSymbol target, AnnotationDataModel attributes)
    {
        var interfaces = new HashSet<RepresentableTypeModel>();
        var supertypes = new HashSet<RepresentableTypeModel>();
        var allOmissions = new HashSet<RepresentableTypeModel>();

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
