namespace RhoMicro.CodeAnalysis.Library.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Immutable;

readonly record struct ContainingTypeModel(
    String Visibility,
    String TypeModifier,
    String Name,
    ImmutableArray<String> TypeParameters) :
    IIndentedStringBuilderAppendable
{
    public static ContainingTypeModel Create(INamedTypeSymbol symbol) =>
        new(SyntaxFacts.GetText(symbol.DeclaredAccessibility),
            Utils.GetTypeModifiers(symbol),
            symbol.Name,
            symbol.TypeParameters.Select(p => p.Name).ToImmutableArray());

    public void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Append(Visibility)
            .Append(' ')
            .Append(TypeModifier)
            .Append(' ')
            .Append(Name);

        if(TypeParameters.Length > 0)
            _ = builder.OpenAngledBlock().AppendJoin(", ", TypeParameters).CloseBlock();
    }

    public Boolean Equals(ContainingTypeModel other) =>
        other.Visibility == Visibility &&
        other.TypeModifier == TypeModifier &&
        other.Name == Name &&
        other.TypeParameters.Length == TypeParameters.Length;
    public override Int32 GetHashCode() =>
        (Visibility, TypeModifier, Name, TypeParameters.Length).GetHashCode();
}
