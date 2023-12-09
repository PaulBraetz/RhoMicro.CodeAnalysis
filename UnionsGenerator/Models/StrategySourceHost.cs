namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

    public void AppendDedicatedReferenceFields(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
        _dedicatedReferenceFieldAdditions.ForEach(t => t.Factory.Invoke(builder, cancellationToken));
    public void AppendDedicatedPureValueTypeFields(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
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
    public void AppendReferenceTypeContainerField(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
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
    private readonly List<Action<IExpandingMacroStringBuilder<Macro>, CancellationToken>> _valueTypeFieldAdditions = [];
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

    public void AppendValueTypeContainerField(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
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

    public void AppendValueTypeContainerType(IExpandingMacroStringBuilder<Macro> builder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        if(!_target.Symbol.IsGenericType)
            _ = builder.AppendLine("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");

        _ = builder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("internal readonly struct ")
                .Append(_target.ValueTypeContainerName)
                .AppendLine('{')
                .AppendJoin(
                    _valueTypeFieldAdditions,
                    (b, a, t) =>
                    {
                        a.Invoke(b, t);
                        return b;
                    })
                .AppendLine('}');
    }
}
