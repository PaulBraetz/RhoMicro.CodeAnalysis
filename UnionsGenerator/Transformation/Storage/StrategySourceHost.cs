namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.Library.Text;
using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendables;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;
using System.Reflection;

sealed class StrategySourceHost(
    SettingsModel settings,
    TypeSignatureModel unionTypeSignature,
    Boolean unionTypeIsGeneric,
    EquatableList<RepresentableTypeModel> representableTypes)
{
    private readonly SettingsModel _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly EquatableList<RepresentableTypeModel> _representableTypes = representableTypes ?? throw new ArgumentNullException(nameof(representableTypes));

    private readonly List<(IndentedStringBuilderAppendable Appendable, String TypeName)> _dedicatedReferenceFieldAdditions = [];
    private readonly List<(IndentedStringBuilderAppendable Appendable, String TypeName)> _dedicatedPureValueTypeFieldAdditions = [];
    private readonly List<(IndentedStringBuilderAppendable Appendable, String TypeName)> _dedicatedImpureAndUnknownFieldAdditions = [];
    public void AddDedicatedField(StorageStrategy strategy)
    {
        var fullName = strategy.RepresentableType.Signature.Names.FullGenericName;
        ( strategy.RepresentableType.Signature.Nature switch
        {
            TypeNature.ReferenceType => _dedicatedReferenceFieldAdditions,
            TypeNature.PureValueType => _dedicatedPureValueTypeFieldAdditions,
            _ => _dedicatedImpureAndUnknownFieldAdditions
        } ).Add(
            (Appendable: new((b) =>
            {
                _ = b
                .Comment.OpenSummary()
                .Append("Contains the value of instances of ").Comment.SeeRef(unionTypeSignature).Append(" representing an instance of ")
                .Comment.Ref(strategy.RepresentableType.Signature)
                .Append('.')
                .CloseBlock()
                .GeneratedUnnavigableInternalCode(unionTypeSignature)
                .Append("private readonly ").Append(fullName).Append(strategy.NullableFieldQuestionMark).Append(' ').Append(strategy.FieldName).AppendLine(';');
            }),
            TypeName: fullName));
    }

    public void DedicatedReferenceFields(IndentedStringBuilder builder) =>
        _dedicatedReferenceFieldAdditions.ForEach(t => t.Appendable.AppendTo(builder));
    public void DedicatedPureValueTypeFields(IndentedStringBuilder builder) =>
        _dedicatedPureValueTypeFieldAdditions.OrderByDescending(static t =>
        {
            var pureValueType = Type.GetType(t.TypeName, false);
            var size = pureValueType != null ?
                Marshal.SizeOf(pureValueType) :
                Int32.MaxValue;
            return size;
        }).ForEach(t => t.Appendable.AppendTo(builder));
    public void DedicatedImpureAndUnknownFields(IndentedStringBuilder builder) =>
        _dedicatedImpureAndUnknownFieldAdditions.ForEach(t => t.Appendable.AppendTo(builder));

    private Boolean _referenceFieldRequired;
    public void AddReferenceTypeContainerField() => _referenceFieldRequired = true;
    public void ReferenceTypeContainerField(IndentedStringBuilder builder)
    {
        if(!_referenceFieldRequired)
            return;

        _ = builder.Comment.OpenSummary()
            .Append("Contains the value of instances of ").Comment.SeeRef(unionTypeSignature).Append(" representing one of these types:")
            .Comment.OpenList("bullet");

        var referenceTypes = representableTypes
            .Select(t => t.StorageStrategy.Value)
            .Where(s => s.ActualOption == StorageOption.Reference)
            .Select(s => s.RepresentableType.Signature);
        foreach(var referenceType in referenceTypes)
        {
            _ = builder.Comment.OpenItem()
                .Comment.Ref(referenceType)
                .CloseBlock();
        }

        _ = builder.CloseBlock()
            .CloseBlock()
            .GeneratedUnnavigableInternalCode(unionTypeSignature)
            .Append("private readonly System.Object? ").Append(settings.ReferenceTypeContainerName).AppendLine(';');
    }

    private Boolean _valueTypeContainerTypeRequired;
    public void AddValueTypeContainerType() => _valueTypeContainerTypeRequired = true;
    public void AddValueTypeContainerField() => _valueTypeContainerTypeRequired = true;
    private readonly List<IndentedStringBuilderAppendable> _valueTypeFieldAdditions = [];

    public void AddValueTypeContainerInstanceFieldAndCtor(StorageStrategy strategy) =>
        _valueTypeFieldAdditions.Add(new((b) =>
        {
            var fullName = strategy.RepresentableType.Signature.Names.FullGenericName;

            _ = b.Comment.OpenSummary()
                .Append("Contains the value of instances of ").Comment.SeeRef(unionTypeSignature).Append(" representing an instance of ")
                .Comment.Ref(strategy.RepresentableType.Signature)
                .Append('.')
                .CloseBlock()
                .Append(b =>
                {
                    if(!unionTypeIsGeneric)
                        b.Append("[System.Runtime.InteropServices.FieldOffset(0)]").AppendLineCore();
                })
                .Append("public readonly ").Append(fullName)
                .Append(' ').Append(strategy.RepresentableType.Alias).AppendLine(';')
                .Append("public ").Append(settings.ValueTypeContainerTypeName).Append('(')
                .Append(fullName).Append(" value) => this.")
                .Append(strategy.RepresentableType.Alias).AppendLine(" = value;");
        }));

    public void ValueTypeContainerField(IndentedStringBuilder builder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        _ = builder.Comment.OpenSummary()
            .Append("Contains the value of instances of ").Comment.SeeRef(unionTypeSignature).Append(" representing one of these types:")
            .Comment.OpenList("bullet");

        var valueTypes = representableTypes
            .Select(t => t.StorageStrategy.Value)
            .Where(s => s.ActualOption == StorageOption.Value)
            .Select(s => s.RepresentableType.Signature);
        foreach(var valueType in valueTypes)
        {
            _ = builder.Comment.OpenItem()
                .Comment.Ref(valueType)
                .CloseBlock();
        }

        _ = builder.CloseBlock()
            .CloseBlock()
            .GeneratedUnnavigableInternalCode(unionTypeSignature)
            .Append("private readonly ").Append(settings.ValueTypeContainerTypeName)
            .Append(' ').Append(settings.ValueTypeContainerName).Append(';');
    }
    public void ValueTypeContainerType(IndentedStringBuilder builder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        _ = builder.Comment.OpenSummary()
            .Append("Helper type for storing value types efficiently.")
            .CloseBlock()
            .GeneratedUnnavigableInternalCode(unionTypeSignature);

        if(!unionTypeIsGeneric)
            _ = builder.AppendLine("[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]");

        _ = builder
            .Append("readonly struct ").Append(settings.ValueTypeContainerTypeName)
            .OpenBracesBlock()
            .AppendJoinLines(StringOrChar.Empty, _valueTypeFieldAdditions)
            .CloseBlock();
    }
}

file static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        foreach(var value in values)
            action.Invoke(value);
    }
}