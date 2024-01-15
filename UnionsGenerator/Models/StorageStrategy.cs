namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;

abstract partial class StorageStrategy
{
    #region Constructor
    private StorageStrategy(
        String safeAlias,
        String fullTypeName,
        StorageOption selectedOption,
        RepresentableTypeNature typeNature,
        StorageSelectionViolation violation)
    {
        SafeAlias = safeAlias;
        FullTypeName = fullTypeName;
        SelectedOption = selectedOption;
        TypeNature = typeNature;
        Violation = violation;
    }
    #endregion
    #region Fields
    public readonly String SafeAlias;
    public readonly String FullTypeName;
    public readonly StorageOption SelectedOption;
    public readonly RepresentableTypeNature TypeNature;
    public readonly StorageSelectionViolation Violation;
    #endregion
    #region Factory
    public static StorageStrategy Create(
        String safeAlias,
        String fullTypeName,
        StorageOption selectedOption,
        RepresentableTypeNature typeNature,
        Boolean targetIsGeneric)
    {

        var result = typeNature switch
        {
            RepresentableTypeNature.PureValueType => createForPureValueType(),
            RepresentableTypeNature.ImpureValueType => createForImpureValueType(),
            RepresentableTypeNature.ReferenceType => createForReferenceType(),
            _ => createForUnknownType(),
        };

        return result;

        StorageStrategy createReference(StorageSelectionViolation violation = StorageSelectionViolation.None) =>
            new ReferenceContainerStrategy(safeAlias, fullTypeName, selectedOption, typeNature, violation);
        StorageStrategy createValue(StorageSelectionViolation violation = StorageSelectionViolation.None) =>
            new ValueContainerStrategy(safeAlias, fullTypeName, selectedOption, typeNature, violation);
        StorageStrategy createField(StorageSelectionViolation violation = StorageSelectionViolation.None) =>
            new FieldContainerStrategy(safeAlias, fullTypeName, selectedOption, typeNature, violation);

        /*
        read tables like so:
    type nature | selected strategy | generic strat(diag) : nongeneric strat(diag)
        */

        /*
    PureValue   Reference   => reference(box)
                Value       => field(generic) : value
                Field       => field        
                Auto        => field : value
        */
        StorageStrategy createForPureValueType() =>
            selectedOption switch
            {
                StorageOption.Reference => createReference(StorageSelectionViolation.PureValueReferenceSelection),
                StorageOption.Value => targetIsGeneric ? createField(StorageSelectionViolation.PureValueValueSelectionGeneric) : createValue(),
                StorageOption.Field => createField(),
                _ => targetIsGeneric ? createField() : createValue()
            };
        /*
    ImpureValue Reference   => reference(box)
                Value       => field(tle)            
                Field       => field
                Auto        => field
        */
        StorageStrategy createForImpureValueType() =>
            selectedOption switch
            {
                StorageOption.Reference => createReference(StorageSelectionViolation.ImpureValueReference),
                StorageOption.Value => createField(StorageSelectionViolation.ImpureValueValue),
                StorageOption.Field => createField(),
                _ => createField()
            };
        /*
    Reference   Reference   => reference
                Value       => reference(tle)        
                Field       => field        
                Auto        => reference
        */
        StorageStrategy createForReferenceType() =>
            selectedOption switch
            {
                StorageOption.Reference => createReference(),
                StorageOption.Value => createReference(StorageSelectionViolation.ReferenceValue),
                StorageOption.Field => createField(),
                _ => createReference()
            };
        /*
    Unknown     Reference   => reference(pbox)
                Value       => field(ptle)            
                Field       => field
                Auto        => field
        */
        StorageStrategy createForUnknownType() =>
            selectedOption switch
            {
                StorageOption.Reference => createReference(StorageSelectionViolation.UnknownReference),
                StorageOption.Value => createField(StorageSelectionViolation.UnknownValue),
                StorageOption.Field => createField(),
                _ => createField()
            };
    }
    #endregion
    #region Template Methods
    public abstract void InstanceVariableExpression(
        ExpandingMacroBuilder builder,
        String instance);
    public abstract void TypesafeInstanceVariableExpression(
        ExpandingMacroBuilder builder,
        String instance);
    public abstract void ConvertedInstanceVariableExpression(
        ExpandingMacroBuilder builder,
        String targetType,
        String instance);
    public abstract void InstanceVariableAssignmentExpression(
        ExpandingMacroBuilder builder,
        String valueExpression,
        String instance);
    #endregion

    public void EqualsInvocation(
        ExpandingMacroBuilder builder,
        String instance,
        String otherInstance) =>
        _ = TypeNature is RepresentableTypeNature.ImpureValueType
            or RepresentableTypeNature.PureValueType
            ? builder * '(' * (TypesafeInstanceVariableExpression, instance) * ".Equals(" * (TypesafeInstanceVariableExpression, otherInstance) * "))"
            : builder * '(' * (InstanceVariableExpression, instance) * " == null ? " /
                (InstanceVariableExpression, otherInstance) * " == null : " /
                (InstanceVariableExpression, otherInstance) * " != null && " *
                (TypesafeInstanceVariableExpression, instance) * ".Equals(" * (TypesafeInstanceVariableExpression, otherInstance) * "))";

    public void EqualsInvocation(
        ExpandingMacroBuilder builder,
        String otherInstance) =>
        EqualsInvocation(builder, "this", otherInstance);

    public void InstanceVariableExpression(
        ExpandingMacroBuilder builder) =>
        InstanceVariableExpression(builder, "this");
    public void GetHashCodeInvocation(
        ExpandingMacroBuilder builder,
        String instance) =>
       _ = TypeNature is RepresentableTypeNature.PureValueType
            or RepresentableTypeNature.ImpureValueType
            ? builder * '(' * (TypesafeInstanceVariableExpression, instance) * ".GetHashCode())"
            : builder * '(' * (InstanceVariableExpression, instance) * "?.GetHashCode() ?? 0)";
    public void GetHashCodeInvocation(
        ExpandingMacroBuilder builder) =>
        GetHashCodeInvocation(builder, "this");

    public void TypesafeInstanceVariableExpression(
        ExpandingMacroBuilder builder) => TypesafeInstanceVariableExpression(builder, "this");

    public void ConvertedInstanceVariableExpression(
        ExpandingMacroBuilder builder,
        String targetType) => ConvertedInstanceVariableExpression(builder, targetType, "this");

    public void InstanceVariableAssignmentExpression(
        ExpandingMacroBuilder builder,
        String valueExpression) => InstanceVariableAssignmentExpression(builder, valueExpression, "this");

    public void ToStringInvocation(
        ExpandingMacroBuilder builder,
        String instance)
    {
        _ = builder * '(' * (InstanceVariableExpression, instance);

        if(TypeNature is RepresentableTypeNature.ReferenceType
            or RepresentableTypeNature.UnknownType)
        {
            _ = builder * '?';
        }

        _ = builder * ".ToString())";
    }
    public void ToStringInvocation(ExpandingMacroBuilder builder) =>
        ToStringInvocation(builder, "this");

    public abstract void Visit(StrategySourceHost host);
}
