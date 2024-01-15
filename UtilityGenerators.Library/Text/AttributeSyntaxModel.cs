namespace RhoMicro.CodeAnalysis.Library.Text;

using RhoMicro.CodeAnalysis.Library;

using System.Collections.Immutable;
readonly record struct AttributeSyntaxModel(
    String Namespace,
    String Name,
    ImmutableArray<AttributeConstructorArgumentModel> ConstructorArguments,
    ImmutableArray<AttributePropertyArgumentModel> PropertyArguments) :
    IIndentedStringBuilderAppendable
{
    public void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.OpenBlock(Block.Brackets)
            .Append(Namespace)
            .Append('.')
            .Append(Name)
            .OpenParensBlock()
            .AppendJoin(", ", ConstructorArguments, cancellationToken);
        if(PropertyArguments.Length > 0 && ConstructorArguments.Length > 0)
            _ = builder.Append(", ");
        _ = builder.AppendJoin(", ", PropertyArguments, cancellationToken)
            .CloseAllBlocks();
    }

    private static readonly ImmutableArrayCollectionEqualityComparer<AttributeConstructorArgumentModel> _ctorArgsComparer =
        ImmutableArrayCollectionEqualityComparer<AttributeConstructorArgumentModel>.Default;
    private static readonly ImmutableArrayCollectionEqualityComparer<AttributePropertyArgumentModel> _propArgsComparer =
        ImmutableArrayCollectionEqualityComparer<AttributePropertyArgumentModel>.Default;
    public Boolean Equals(AttributeSyntaxModel other) =>
        other.Namespace == Namespace &&
        other.Name == Name &&
        _ctorArgsComparer.Equals(other.ConstructorArguments, ConstructorArguments) &&
        _propArgsComparer.Equals(other.PropertyArguments, PropertyArguments);
    public override Int32 GetHashCode() =>
        (Namespace, Name, _ctorArgsComparer.GetHashCode(ConstructorArguments), _propArgsComparer.GetHashCode(PropertyArguments)).GetHashCode();
}
