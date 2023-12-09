namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

sealed class AnnotationDataModel
{
    public readonly IReadOnlyList<RepresentableTypeModel> AllRepresentableTypes;
    public readonly IReadOnlyList<RepresentableTypeModel> RepresentableReferenceTypes;
    public readonly IReadOnlyList<RepresentableTypeModel> RepresentableMixedValueTypes;
    public readonly IReadOnlyList<RepresentableTypeModel> RepresentablePureValueTypes;
    public readonly IReadOnlyList<RepresentableTypeModel> AllRepresentableValueTypes;
    public readonly IReadOnlyList<RepresentableTypeModel> RepresentableUnknownTypes;

    public readonly IReadOnlyList<RelationAttribute> Relations;
    public readonly UnionTypeSettingsAttribute Settings;

    private AnnotationDataModel(
        UnionTypeSettingsAttribute settings,
        IReadOnlyList<RepresentableTypeModel> allRepresentableTypes,
        IReadOnlyList<RepresentableTypeModel> representableReferenceTypes,
        IReadOnlyList<RepresentableTypeModel> representableUnknownTypes,
        IReadOnlyList<RepresentableTypeModel> representablePureValueTypes,
        IReadOnlyList<RepresentableTypeModel> representableMixedValueTypes,
        IReadOnlyList<RelationAttribute> relations)
    {
        Settings = settings;
        Relations = relations;

        AllRepresentableTypes = allRepresentableTypes;
        RepresentableReferenceTypes = representableReferenceTypes;
        RepresentableUnknownTypes = representableUnknownTypes;
        RepresentablePureValueTypes = representablePureValueTypes;
        RepresentableMixedValueTypes = representableMixedValueTypes;

        AllRepresentableValueTypes = RepresentablePureValueTypes.Concat(RepresentableMixedValueTypes).ToList();
    }

    public static AnnotationDataModel Create(INamedTypeSymbol target)
    {
        var attributes = target.GetAttributes();

        //DO NOT CHANGE THIS ALGO, compatibility depends on deterministic order of types
        var orderedRepresentableTypes = attributes
            .OfUnionTypeAttribute()
            .Select(a => a.ExtractData(target))
            .GroupBy(a => a.Nature == RepresentableTypeNature.UnknownType)
            .OrderBy(g => g.Key) //generic params come last
            .Select(g =>
                g.Key ?
                g.OrderBy(a => a.Names.FullTypeName) :
                g.OrderBy(a => a.Names.FullTypeName))
            .SelectMany(g => g);

        // reference types
        // value types
        // generic params

        List<RepresentableTypeModel> allRepresentableTypes = [];
        List<RepresentableTypeModel> representableReferenceTypes = [];
        List<RepresentableTypeModel> representableMixedValueTypes = [];
        List<RepresentableTypeModel> representablePureValueTypes = [];
        List<RepresentableTypeModel> representableUnknownTypes = [];

        foreach(var representableType in orderedRepresentableTypes)
        {
            (representableType.Nature switch
            {
                RepresentableTypeNature.ReferenceType => representableReferenceTypes,
                RepresentableTypeNature.PureValueType => representablePureValueTypes,
                RepresentableTypeNature.ImpureValueType => representableMixedValueTypes,
                _ => representableUnknownTypes
            }).Add(representableType);

            allRepresentableTypes.Add(representableType);
        }

        var settings = attributes.OfUnionTypeSettingsAttribute().SingleOrDefault() ??
            target.ContainingAssembly.GetAttributes().OfUnionTypeSettingsAttribute().SingleOrDefault() ??
            new UnionTypeSettingsAttribute();

        var relations = attributes.OfRelationAttribute().ToList();

        var result = new AnnotationDataModel(
            settings: settings,
            relations: relations,
            allRepresentableTypes: allRepresentableTypes,
            representableReferenceTypes: representableReferenceTypes,
            representablePureValueTypes: representablePureValueTypes,
            representableMixedValueTypes: representableMixedValueTypes,
            representableUnknownTypes: representableUnknownTypes);

        return result;
    }
}
