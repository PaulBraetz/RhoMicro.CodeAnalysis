namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

sealed class StrategySourceHost(TargetDataModel target)
{
    private readonly TargetDataModel _target = target;

    private readonly List<(Action<IExpandingMacroStringBuilder<Macro>, CancellationToken> Factory, String TypeName)> _dedicatedReferenceFieldAdditions = [];
    private readonly List<(Action<IExpandingMacroStringBuilder<Macro>, CancellationToken> Factory, String TypeName)> _dedicatedPureValueTypeFieldAdditions = [];
    private readonly List<(Action<IExpandingMacroStringBuilder<Macro>, CancellationToken> Factory, String TypeName)> _dedicatedImpureAndUnknownFieldAdditions = [];
    public void AddDedicatedField(StorageStrategy strategy) =>
        (strategy.TypeNature switch
        {
            RepresentableTypeNature.ReferenceType => _dedicatedReferenceFieldAdditions,
            RepresentableTypeNature.PureValueType => _dedicatedPureValueTypeFieldAdditions,
            _ => _dedicatedImpureAndUnknownFieldAdditions
        }).Add(
            (Factory: (b, t) =>
            b.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("private readonly ")
                .Append(strategy.FullTypeName).Append(' ')
                .Append(strategy.SafeAlias.ToGeneratedCamelCase())
                .AppendLine(';'),
            TypeName: strategy.FullTypeName));

    public void DedicatedReferenceFieldsAppendix(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
        _dedicatedReferenceFieldAdditions.ForEach(t => t.Factory.Invoke(builder, cancellationToken));
    public void DedicatedPureValueTypeFieldsAppendix(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
        _dedicatedPureValueTypeFieldAdditions.OrderByDescending(static t =>
        {
            var pureValueType = Type.GetType(t.TypeName, false);
            var size = pureValueType != null ?
                Marshal.SizeOf(pureValueType) :
                Int32.MaxValue;
            return size;
        }).ForEach(t => t.Factory.Invoke(builder, cancellationToken));
    public void AppendDedicatedImpureAndUnknownFields(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
        _dedicatedImpureAndUnknownFieldAdditions.ForEach(t => t.Factory.Invoke(builder, cancellationToken));

    private Boolean _referenceFieldRequired;
    public void AddReferenceTypeContainerField() => _referenceFieldRequired = true;
    public void ReferenceTypeContainerFieldAppendix(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        if(!_referenceFieldRequired)
            return;

        cancellationToken.ThrowIfCancellationRequested();

        _ = builder
            .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
            .AppendLine("private readonly global::System.Object __referenceTypeContainer;");
    }

    private Boolean _valueTypeContainerTypeRequired;
    public void AddValueTypeContainerType() => _valueTypeContainerTypeRequired = true;
    public void AddValueTypeContainerField() => _valueTypeContainerTypeRequired = true;
    private readonly List<Appendix<Macro>> _valueTypeFieldAdditions = [];
    public void AddValueTypeVontainerInstanceFieldAndCtor(StorageStrategy strategy) =>
        _valueTypeFieldAdditions.Add((b, t) =>
        {
            if(!_target.Symbol.IsGenericType)
                _ = b.AppendLine("[global::System.Runtime.InteropServices.FieldOffset(0)]");

            _ = b.Append("public readonly ").Append(strategy.FullTypeName).Append(' ')
                .Append(strategy.SafeAlias).AppendLine(';')
                .Append("public ")
                .Append(_target.ValueTypeContainerName)
                .Append('(').Append(strategy.FullTypeName)
                .AppendLine(" value) => ")
                .Append(strategy.SafeAlias).AppendLine(" = value;");
        });

    public void ValueTypeContainerFieldAppendix(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        cancellationToken.ThrowIfCancellationRequested();

        _ = builder
            .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
            .AppendLine("private readonly ")
            .Append(_target.ValueTypeContainerName)
            .Append(" __valueTypeContainer;");
    }

    public void ValueTypeContainerTypeAppendix(ExpandingMacroBuilder builder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        _ = builder +
            (Extensions.DocCommentAppendix, "Helper types for storing value types efficiently.");

        if(!_target.Symbol.IsGenericType)
            _ = builder.AppendLine("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");

        _ = builder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("readonly struct ")
                .Append(_target.ValueTypeContainerName)
                .AppendLine('{')
                .AppendJoin(
                    _valueTypeFieldAdditions,
                    (b, a, t) => b.Append(a, t),
                    cancellationToken)
                .AppendLine('}');
    }
}
