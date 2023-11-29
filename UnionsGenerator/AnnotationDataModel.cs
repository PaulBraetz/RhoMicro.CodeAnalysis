namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

sealed class AnnotationDataModel : IEquatable<AnnotationDataModel>
{
    public readonly IReadOnlyList<RepresentableTypeData> AllRepresentableTypes;
    public readonly IReadOnlyList<RepresentableTypeData> RepresentableReferenceTypes;
    public readonly IReadOnlyList<RepresentableTypeData> RepresentableMixedValueTypes;
    public readonly IReadOnlyList<RepresentableTypeData> RepresentablePureValueTypes;
    public readonly IReadOnlyList<RepresentableTypeData> AllRepresentableValueTypes;
    public readonly IReadOnlyList<RepresentableTypeData> RepresentableUnknownTypes;

    public readonly IReadOnlyList<RelationAttribute> Relations;
    public readonly UnionTypeSettingsAttribute Settings;

    private AnnotationDataModel(
        UnionTypeSettingsAttribute settings,
        IReadOnlyList<RepresentableTypeData> allRepresentableTypes,
        IReadOnlyList<RepresentableTypeData> representableReferenceTypes,
        IReadOnlyList<RepresentableTypeData> representableUnknownTypes,
        IReadOnlyList<RepresentableTypeData> representablePureValueTypes,
        IReadOnlyList<RepresentableTypeData> representableMixedValueTypes,
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

        List<RepresentableTypeData> allRepresentableTypes = [];
        List<RepresentableTypeData> representableReferenceTypes = [];
        List<RepresentableTypeData> representableMixedValueTypes = [];
        List<RepresentableTypeData> representablePureValueTypes = [];
        List<RepresentableTypeData> representableUnknownTypes = [];

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

    public override Boolean Equals(Object? obj) =>
        obj is AnnotationDataModel other && Equals(other);
    public Boolean Equals(AnnotationDataModel other) =>
        other is not null &&
        other.AllRepresentableTypes.SequenceEqual(AllRepresentableTypes) &&
        other.Relations.SequenceEqual(Relations) &&
        other.Settings.Equals(Settings);

    public override Int32 GetHashCode()
    {
        var hashCode = AllRepresentableTypes.Aggregate(-40951477, (h, a) => h * -1521134295 + a.GetHashCode());
        hashCode = Relations.Aggregate(hashCode, (h, a) => h * -1521134295 + a.GetHashCode());
        hashCode = hashCode * -1521134295 + Settings.GetHashCode();

        return hashCode;
    }

    public static Boolean operator ==(AnnotationDataModel left, AnnotationDataModel right) => EqualityComparer<AnnotationDataModel>.Default.Equals(left, right);
    public static Boolean operator !=(AnnotationDataModel left, AnnotationDataModel right) => !(left == right);
}
