namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.Common;
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
    public abstract void InstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String instance,
        CancellationToken cancellationToken);
    public void InstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) => InstanceVariableExpressionAppendix(builder, "this", cancellationToken);

    public abstract void AppendConvertedInstanceVariableExpression(
        IExpandingMacroStringBuilder<Macro> builder,
        (String targetType, String instance) model,
        CancellationToken cancellationToken);
    public void ConvertedInstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String targetType,
        CancellationToken cancellationToken) => AppendConvertedInstanceVariableExpression(builder, (targetType, "this"), cancellationToken);

    public abstract void InstanceVariableAssignmentExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        (String valueExpression, String instance) model,
        CancellationToken cancellationToken);
    public void InstanceVariableAssignmentExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String valueExpression,
        CancellationToken cancellationToken) => InstanceVariableAssignmentExpressionAppendix(builder, (valueExpression, "this"), cancellationToken);
    #endregion

    public void ToStringInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String instance,
        CancellationToken cancellationToken)
    {
        _ = builder.Append('(')
            .Append(InstanceVariableExpressionAppendix, instance, cancellationToken);

        if(TypeNature == RepresentableTypeNature.ReferenceType)
        {
            _ = builder.Append('?');
        }

        _ = builder.Append(".ToString())");
    }
    public void ToStringInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) => ToStringInvocationAppendix(builder, "this", cancellationToken);

    public void GetHashCodeInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String instance,
        CancellationToken cancellationToken) =>
        builder.Append("(global::System.Collections.Generic.EqualityComparer<")
            .Append(FullTypeName)
            .Append(">.Default.GetHashCode(")
            .Append(InstanceVariableExpressionAppendix, instance, cancellationToken)
            .Append("))");
    public void GetHashCodeInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) => GetHashCodeInvocationAppendix(builder, "this", cancellationToken);

    public void EqualsInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        (String instance, String parameter) model,
        CancellationToken cancellationToken)
    {
        var (instance, parameter) = model;

        _ = builder.Append("(global::System.Collections.Generic.EqualityComparer<")
            .Append(FullTypeName)
            .Append(">.Default.Equals(")
            .Append(InstanceVariableExpressionAppendix, instance, cancellationToken)
            .Append(", ")
            .Append(InstanceVariableExpressionAppendix, parameter, cancellationToken)
            .Append("))");
    }
    public void EqualsInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) => EqualsInvocationAppendix(builder, ("this", "obj"), cancellationToken);

    public abstract void Visit(StrategySourceHost host);
}
