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

    private readonly List<(Action<ExpandingMacroBuilder> Factory, String TypeName)> _dedicatedReferenceFieldAdditions = [];
    private readonly List<(Action<ExpandingMacroBuilder> Factory, String TypeName)> _dedicatedPureValueTypeFieldAdditions = [];
    private readonly List<(Action<ExpandingMacroBuilder> Factory, String TypeName)> _dedicatedImpureAndUnknownFieldAdditions = [];
    public void AddDedicatedField(StorageStrategy strategy) =>
        (strategy.TypeNature switch
        {
            RepresentableTypeNature.ReferenceType => _dedicatedReferenceFieldAdditions,
            RepresentableTypeNature.PureValueType => _dedicatedPureValueTypeFieldAdditions,
            _ => _dedicatedImpureAndUnknownFieldAdditions
        }).Add(
            (Factory: (b) =>
            _ = b / "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]" /
                "private readonly " * strategy.FullTypeName * ' ' * strategy.SafeAlias.ToGeneratedCamelCase() * ';',
            TypeName: strategy.FullTypeName));

    public void DedicatedReferenceFields(ExpandingMacroBuilder builder) =>
        _dedicatedReferenceFieldAdditions.ForEach(t => t.Factory.Invoke(builder));
    public void DedicatedPureValueTypeFields(ExpandingMacroBuilder builder) =>
        _dedicatedPureValueTypeFieldAdditions.OrderByDescending(static t =>
        {
            var pureValueType = Type.GetType(t.TypeName, false);
            var size = pureValueType != null ?
                Marshal.SizeOf(pureValueType) :
                Int32.MaxValue;
            return size;
        }).ForEach(t => t.Factory.Invoke(builder));
    public void DedicatedImpureAndUnknownFields(ExpandingMacroBuilder builder) =>
        _dedicatedImpureAndUnknownFieldAdditions.ForEach(t => t.Factory.Invoke(builder));

    private Boolean _referenceFieldRequired;
    public void AddReferenceTypeContainerField() => _referenceFieldRequired = true;
    public void ReferenceTypeContainerField(ExpandingMacroBuilder builder)
    {
        if(!_referenceFieldRequired)
            return;

        _ = builder *
            "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]" /
            "private readonly global::System.Object __referenceTypeContainer;";
    }

    private Boolean _valueTypeContainerTypeRequired;
    public void AddValueTypeContainerType() => _valueTypeContainerTypeRequired = true;
    public void AddValueTypeContainerField() => _valueTypeContainerTypeRequired = true;
    private readonly List<Action<ExpandingMacroBuilder>> _valueTypeFieldAdditions = [];
    public void AddValueTypeVontainerInstanceFieldAndCtor(StorageStrategy strategy) =>
        _valueTypeFieldAdditions.Add((b) =>
        {
            if(!_target.Symbol.IsGenericType)
                _ = b % "[global::System.Runtime.InteropServices.FieldOffset(0)]";

            _ = b * "public readonly " * strategy.FullTypeName * ' ' * strategy.SafeAlias * ';' /
                "public " * _target.ValueTypeContainerName * '(' * strategy.FullTypeName * " value) => " /
                strategy.SafeAlias % " = value;";
        });

    public void ValueTypeContainerField(ExpandingMacroBuilder builder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        _ = builder * "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]" /
            "private readonly " * _target.ValueTypeContainerName % " __valueTypeContainer;";
    }

    public void ValueTypeContainerType(ExpandingMacroBuilder builder)
    {
        if(!_valueTypeContainerTypeRequired)
            return;

        _ = builder * (Docs.Summary, "Helper types for storing value types efficiently.");

        if(!_target.Symbol.IsGenericType)
            _ = builder % "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]";

        _ = builder *
                "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]" /
                "readonly struct " * _target.ValueTypeContainerName % '{' *
                (b => b.AppendJoin(
                    _valueTypeFieldAdditions,
                    (b, a, t) =>
                    {
                        a.Invoke(b.WithOperators(builder.CancellationToken));
                        return b;
                    },
                    b.CancellationToken)) % '}';
    }
}
