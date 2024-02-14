namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;

enum StorageSelectionViolation
{
    None,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Pure Value</description></item>
    /// <item><term>Selected</term><description>Reference</description></item>
    /// <item><term>Actual</term><description>Reference</description></item>
    /// <item><term>Diagnostic</term><description>warn about definite boxing</description></item>
    /// </list>
    /// </summary>
    PureValueReferenceSelection,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Pure Value</description></item>
    /// <item><term>Selected</term><description>Value</description></item>
    /// <item><term>Actual</term><description>Field</description></item>
    /// <item><term>Diagnostic</term><description>ignored due to generic type</description></item>
    /// </list>
    /// </summary>
    PureValueValueSelectionGeneric,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Impure Value</description></item>
    /// <item><term>Selected</term><description>Reference</description></item>
    /// <item><term>Actual</term><description>Reference</description></item>
    /// <item><term>Diagnostic</term><description>warn about definite boxing</description></item>
    /// </list>
    /// </summary>
    ImpureValueReference,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Impure Value</description></item>
    /// <item><term>Selected</term><description>Value</description></item>
    /// <item><term>Actual</term><description>Field</description></item>
    /// <item><term>Diagnostic</term><description>ignored due to guaranteed tle</description></item>
    /// </list>
    /// </summary>
    ImpureValueValue,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Reference</description></item>
    /// <item><term>Selected</term><description>Value</description></item>
    /// <item><term>Actual</term><description>Reference</description></item>
    /// <item><term>Diagnostic</term><description>ignored due to guaranteed tle</description></item>
    /// </list>
    /// </summary>
    ReferenceValue,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Unknown</description></item>
    /// <item><term>Selected</term><description>Reference</description></item>
    /// <item><term>Actual</term><description>Reference</description></item>
    /// <item><term>Diagnostic</term><description>warn about possible boxing</description></item>
    /// </list>
    /// </summary>
    UnknownReference,
    /// <summary>
    /// <list type="table">
    /// <item><term>Type</term><description>Unknown</description></item>
    /// <item><term>Selected</term><description>Value</description></item>
    /// <item><term>Actual</term><description>Field</description></item>
    /// <item><term>Diagnostic</term><description>warn about possible tle</description></item>
    /// </list>
    /// </summary>
    UnknownValue
}