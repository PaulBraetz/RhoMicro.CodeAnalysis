namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Collections.Generic;
using System.Text;

sealed class StrategySourceHost(TargetDataModel target)
{
    private readonly TargetDataModel _target = target;

    private readonly List<Action<StringBuilder>> _dedicatedFieldAdditions = [];
    public void AddDedicatedField(StorageStrategy strategy) =>
        _dedicatedFieldAdditions.Add((s) =>
            s.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("private readonly ")
                .Append(strategy.FullTypeName).Append(' ')
                .Append(strategy.SafeAlias.ToGeneratedCamelCase())
                .AppendLine(";"));
    public void AppendDedicatedFields(StringBuilder sourceTextBuilder) =>
        _dedicatedFieldAdditions.ForEach(a => a.Invoke(sourceTextBuilder));

    private Boolean _referenceFieldRequired;
    public void AddReferenceTypeContainerField() => _referenceFieldRequired = true;
    public void AppendReferenceTypeContainerField(StringBuilder sourceTextBuilder)
    {
        if(!_referenceFieldRequired)
            return;
        _ = sourceTextBuilder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("private readonly global::System.Object __referenceTypeContainer;");
    }

    private Boolean _valueTypeContainerTypeRequired;
    public void AddValueTypeContainerType() => _valueTypeContainerTypeRequired = true;
    public void AddValueTypeContainerField() => _valueTypeContainerTypeRequired = true;
    private readonly List<Action<StringBuilder>> _valueTypeFieldAdditions = [];
    public void AddValueTypeVontainerInstanceFieldAndCtor(StorageStrategy strategy) =>
        _valueTypeFieldAdditions.Add((s) =>
        {
            if(!_target.Symbol.IsGenericType)
            {
                _ = s.AppendLine("[global::System.Runtime.InteropServices.FieldOffset(0)]");
            }

            _ = s.Append("public readonly ").Append(strategy.FullTypeName).Append(' ')
                .Append(strategy.SafeAlias).AppendLine(";")
                .Append("public ")
                .Append(_target.ValueTypeContainerName)
                .Append('(').Append(strategy.FullTypeName)
                .AppendLine(" value) => ")
                .Append(strategy.SafeAlias).AppendLine(" = value;");
        });

    public void AppendValueTypeContainerField(StringBuilder sourceTextBuilder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;
        _ = sourceTextBuilder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("private readonly ")
                .Append(_target.ValueTypeContainerName)
                .Append(" __valueTypeContainer;");
    }
    
    public void AppendValueTypeContainerType(StringBuilder sourceTextBuilder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        if(!_target.Symbol.IsGenericType)
        {
            _ = sourceTextBuilder.AppendLine(
                "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");
        }

        _ = sourceTextBuilder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("internal readonly struct ")
                .Append(_target.ValueTypeContainerName)
                .AppendLine("{")
                .AppendAggregate(
                    _valueTypeFieldAdditions,
                    (b, a) =>
                    {
                        a.Invoke(b);
                        return b;
                    })
                .AppendLine("}");
    }
}
