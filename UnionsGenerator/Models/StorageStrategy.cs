namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
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
    public abstract void TypesafeInstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String instance,
        CancellationToken cancellationToken);
    public abstract void AppendConvertedInstanceVariableExpression(
        IExpandingMacroStringBuilder<Macro> builder,
        (String targetType, String instance) model,
        CancellationToken cancellationToken);
    public abstract void InstanceVariableAssignmentExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        (String valueExpression, String instance) model,
        CancellationToken cancellationToken);
    #endregion

    public void EqualsInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        (String instance, String otherInstance) model,
        CancellationToken cancellationToken) =>
        _ = TypeNature is RepresentableTypeNature.ImpureValueType
            or RepresentableTypeNature.PureValueType
            ? builder
                .Append('(')
                .Append(
                    TypesafeInstanceVariableExpressionAppendix,
                    model.instance,
                    cancellationToken)
                .Append(".Equals(")
                .Append(
                    TypesafeInstanceVariableExpressionAppendix,
                    model.otherInstance,
                    cancellationToken)
                .Append("))")
            : builder
                .Append('(')
                .Append(
                    InstanceVariableExpressionAppendix,
                    model.instance,
                    cancellationToken)
                .Append(" == null ? ")
                .Append(
                    InstanceVariableExpressionAppendix,
                    model.otherInstance,
                    cancellationToken)
                .Append(" == null : ")
                .Append(
                    InstanceVariableExpressionAppendix,
                    model.otherInstance,
                    cancellationToken)
                .Append(" != null && ")
                .Append(
                    TypesafeInstanceVariableExpressionAppendix,
                    model.instance,
                    cancellationToken)
                .Append(".Equals(")
                .Append(
                    TypesafeInstanceVariableExpressionAppendix,
                    model.otherInstance,
                    cancellationToken)
                .Append("))");

    public void EqualsInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String otherInstance,
        CancellationToken cancellationToken) =>
        EqualsInvocationAppendix(builder, ("this", otherInstance), cancellationToken);

    public void InstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) =>
        InstanceVariableExpressionAppendix(builder, "this", cancellationToken);
    public void GetHashCodeInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String instance,
        CancellationToken cancellationToken) =>
       _ = TypeNature is RepresentableTypeNature.PureValueType
            or RepresentableTypeNature.ImpureValueType
            ? builder.Append('(')
                .Append(TypesafeInstanceVariableExpressionAppendix, instance, cancellationToken)
                .Append(".GetHashCode())")
            : builder.Append('(')
                .Append(InstanceVariableExpressionAppendix, instance, cancellationToken)
                .Append("?.GetHashCode() ?? 0)");
    public void GetHashCodeInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) =>
        GetHashCodeInvocationAppendix(builder, "this", cancellationToken);

    public void TypesafeInstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) => TypesafeInstanceVariableExpressionAppendix(builder, "this", cancellationToken);

    public void ConvertedInstanceVariableExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String targetType,
        CancellationToken cancellationToken) => AppendConvertedInstanceVariableExpression(builder, (targetType, "this"), cancellationToken);

    public void InstanceVariableAssignmentExpressionAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String valueExpression,
        CancellationToken cancellationToken) => InstanceVariableAssignmentExpressionAppendix(builder, (valueExpression, "this"), cancellationToken);

    public void ToStringInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        String instance,
        CancellationToken cancellationToken)
    {
        _ = builder.Append('(')
            .Append(InstanceVariableExpressionAppendix, instance, cancellationToken);

        if(TypeNature is RepresentableTypeNature.ReferenceType
            or RepresentableTypeNature.UnknownType)
        {
            _ = builder.Append('?');
        }

        _ = builder.Append(".ToString())");
    }
    public void ToStringInvocationAppendix(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken) =>
        ToStringInvocationAppendix(builder, "this", cancellationToken);

    public abstract void Visit(StrategySourceHost host);
}
