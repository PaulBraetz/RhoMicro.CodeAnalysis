namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

/// <summary>
/// Defines the type of relations that can be defined between two union types.
/// </summary>
internal enum RelationType
{
    /// <summary>
    /// There is no relation between the provided type and target type.
    /// They do not share any representable types.
    /// No conversion operators will be generated.
    /// </summary>
    Disjunct,
    /// <summary>
    /// The relation is defined on both the target type as well as the relation type.
    /// Only for one of the two union types will conversion operators be generated.
    /// </summary>
    BidirectionalRelation,
    /// <summary>
    /// The target type is a superset of the provided type.
    /// The target type may represent all of the provided types representable types.
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
    /// The provided type may represent all of the target types representable types.
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
    /// The target type may represent some, but not all of the provided types representable types; and vice-versa.
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
    /// The target type may represent all of the provided types representable types; and vice-versa.
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
