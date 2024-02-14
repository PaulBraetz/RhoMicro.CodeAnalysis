namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;

using System;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;

using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendables;

abstract partial class StorageStrategy
{
    #region Constructor
    private StorageStrategy(
        SettingsModel settings,
        PartialRepresentableTypeModel representableType,
        StorageOption selectedOption,
        StorageSelectionViolation violation)
    {
        Settings = settings;
        RepresentableType = representableType;
        SelectedOption = selectedOption;
        Violation = violation;
        FieldName = representableType.Alias.ToGeneratedCamelCase();
        NullableFieldBang = RepresentableType.Options.HasFlag(UnionTypeOptions.Nullable) ? String.Empty : "!";
        NullableFieldQuestionMark = RepresentableType.Signature is { Nature: TypeNature.UnknownType or TypeNature.ReferenceType } ? "?" : String.Empty;
    }
    #endregion
    #region Fields
    public String FieldName { get; }
    protected SettingsModel Settings { get; }
    public PartialRepresentableTypeModel RepresentableType { get; }
    public StorageOption SelectedOption { get; }
    public abstract StorageOption ActualOption { get; }
    public StorageSelectionViolation Violation { get; }
    protected String NullableFieldBang { get; }
    public String NullableFieldQuestionMark { get; }
    #endregion
    #region Factory
    public static StorageStrategy Create(
        SettingsModel settings,
        Boolean unionTypeIsGeneric,
        PartialRepresentableTypeModel representableType)
    {
        var selectedOption = representableType.Storage;
        var result = representableType.Signature.Nature switch
        {
            TypeNature.PureValueType => createForPureValueType(),
            TypeNature.ImpureValueType => createForImpureValueType(),
            TypeNature.ReferenceType => createForReferenceType(),
            _ => createForUnknownType(),
        };

        return result;

        StorageStrategy createReference(StorageSelectionViolation violation = StorageSelectionViolation.None) =>
            new ReferenceContainerStrategy(settings, representableType, selectedOption, violation);
        StorageStrategy createValue(StorageSelectionViolation violation = StorageSelectionViolation.None) =>
            new ValueContainerStrategy(settings, representableType, selectedOption, violation);
        StorageStrategy createField(StorageSelectionViolation violation = StorageSelectionViolation.None) =>
            new FieldContainerStrategy(settings, representableType, selectedOption, violation);

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
                StorageOption.Value => unionTypeIsGeneric ? createField(StorageSelectionViolation.PureValueValueSelectionGeneric) : createValue(),
                StorageOption.Field => createField(),
                _ => unionTypeIsGeneric ? createField() : createValue()
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
    public abstract IndentedStringBuilderAppendable InstanceVariableExpression(String instance);
    public abstract IndentedStringBuilderAppendable StrongInstanceVariableExpression(String instance);
    public abstract IndentedStringBuilderAppendable ConvertedInstanceVariableExpression(String targetType, String instance);
    public abstract IndentedStringBuilderAppendable InstanceVariableAssignmentExpression(String valueExpression, String instance);
    #endregion

    public IndentedStringBuilderAppendable EqualsInvocation(String instance, String otherInstance) =>
        new(b => _ = b.Operators + '(' + "System.Collections.Generic.EqualityComparer<" + RepresentableType.Signature.Names.FullGenericNullableName +
                ">.Default.Equals(" + StrongInstanceVariableExpression(instance) + ", " + StrongInstanceVariableExpression(otherInstance) + "))");

    public IndentedStringBuilderAppendable EqualsInvocation(String otherInstance) => EqualsInvocation("this", otherInstance);

    public IndentedStringBuilderAppendable InstanceVariableExpression() => InstanceVariableExpression("this");
    public IndentedStringBuilderAppendable GetHashCodeInvocation(String instance) =>
       new(b => _ = b.Operators +
       "(System.Collections.Generic.EqualityComparer<" + RepresentableType.Signature.Names.FullGenericNullableName + ">.Default.GetHashCode(" + StrongInstanceVariableExpression(instance) + "))");
    public IndentedStringBuilderAppendable GetHashCodeInvocation() =>
        GetHashCodeInvocation("this");

    public IndentedStringBuilderAppendable TypesafeInstanceVariableExpression()
        => StrongInstanceVariableExpression("this");

    public IndentedStringBuilderAppendable ConvertedInstanceVariableExpression(String targetType)
        => ConvertedInstanceVariableExpression(targetType, "this");

    public IndentedStringBuilderAppendable InstanceVariableAssignmentExpression(String valueExpression)
        => InstanceVariableAssignmentExpression(valueExpression, "this");

    public IndentedStringBuilderAppendable ToStringInvocation(String instance) =>
        new(b => _ = b.Operators + '(' + InstanceVariableExpression(instance) + NullableFieldQuestionMark + ".ToString() ?? System.String.Empty)");
    public IndentedStringBuilderAppendable ToStringInvocation()
        => ToStringInvocation("this");

    public abstract void Visit(StrategySourceHost host);
}
