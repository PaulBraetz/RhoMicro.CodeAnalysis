namespace RhoMicro.CodeAnalysis.CopyToGenerator;

using Microsoft.CodeAnalysis;

using System.Linq;

readonly struct Model : IEquatable<Model>
{
    private Model(String @namespace, String name, Boolean isInGlobalNamespace, IReadOnlyList<PropertyModel> properties)
    {
        Namespace = @namespace;
        Name = name;
        IsInGlobalNamespace = isInGlobalNamespace;
        Properties = properties;
    }

    public static Model Create(INamedTypeSymbol symbol) => new(
        @namespace: symbol.ContainingNamespace.ToDisplayString(),
        name: symbol.Name,
        isInGlobalNamespace: symbol.ContainingNamespace.IsGlobalNamespace,
        properties: symbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                        p.SetMethod != null &&
                        p.GetMethod != null &&
                        p.SetMethod.DeclaredAccessibility == Accessibility.Public &&
                        p.GetMethod.DeclaredAccessibility == Accessibility.Public)
            .Select(PropertyModel.Create)
            .ToList());

    public String Namespace { get; }
    public String Name { get; }
    public Boolean IsInGlobalNamespace { get; }
    public IReadOnlyList<PropertyModel> Properties { get; }

    public override Boolean Equals(Object? obj) => obj is Model model && Equals(model);
    public Boolean Equals(Model other) => Namespace == other.Namespace &&
        Name == other.Name &&
        IsInGlobalNamespace == other.IsInGlobalNamespace &&
        Properties.SequenceEqual(other.Properties);

    public override Int32 GetHashCode()
    {
        var hashCode = 1428914654;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Namespace);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + IsInGlobalNamespace.GetHashCode();
        hashCode = Properties.Aggregate(hashCode * -1521134295, (c, p) => c + p.GetHashCode());
        return hashCode;
    }

    public static Boolean operator ==(Model left, Model right) => left.Equals(right);
    public static Boolean operator !=(Model left, Model right) => !(left == right);
}
