namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

/// <summary>
/// Defines the type of relations that can be defined between two union types.
/// </summary>
internal enum RelationType
{
    /// <summary>
    /// There is no relation between the provided type and target type.
    /// </summary>
    None,
    /// <summary>
    /// The relation is defined on both the target type as well as the relation type.
    /// </summary>
    BidirectionalRelation,
    /// <summary>
    /// The target type is a superset of the provided type.
    /// This means that two conversion operations will be generated:
    /// <list type="bullet">
    /// <item><description>
    /// an <em>implicit</em> conversion operator from the provided type to the target type
    /// </description></item>
    /// <item><description>
    /// an <em>explicit</em> conversion operator from the target type to the provided type
    /// </description></item>
    /// </list>
    /// This option is not available if the provided type has already defined a relation to the target type.
    /// </summary>
    Superset,
    /// <summary>
    /// The target type is a subset of the provided type.
    /// This means that two conversion operations will be generated:
    /// <list type="bullet">
    /// <item><description>
    /// an <em>implicit</em> conversion operator from the target type to the provided type
    /// </description></item>
    /// <item><description>
    /// an <em>explicit</em> conversion operator from the provided type to the target type
    /// </description></item>
    /// </list>
    /// This option is not available if the provided type has already defined a relation to the target type.
    /// </summary>
    Subset,
    /// <summary>
    /// The target type intersects the provided type.
    /// This means that two conversion operations will be generated:
    /// <list type="bullet">
    /// <item><description>
    /// an <em>explicit</em> conversion operator from the target type to the provided type
    /// </description></item>
    /// <item><description>
    /// an <em>explicit</em> conversion operator from the provided type to the target type
    /// </description></item>
    /// </list>
    /// This option is not available if the provided type has already defined a relation to the target type.
    /// </summary>
    Intersection,
    /// <summary>
    /// The target type is congruent to the provided type.
    /// This means that two conversion operations will be generated:
    /// <list type="bullet">
    /// <item><description>
    /// an <em>implicit</em> conversion operator from the target type to the provided type
    /// </description></item>
    /// <item><description>
    /// an <em>implicit</em> conversion operator from the provided type to the target type
    /// </description></item>
    /// </list>
    /// This option is not available if the provided type has already defined a relation to the target type.
    /// </summary>
    Congruent
}
