namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;
using System.Linq;

public class ConstructorTests : TestBase
{
    [Fact]
    public void GeneratesPrivateInterfaceAccessibilityForPublicIfInconvertible() =>
        UnionType(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Collections.IEnumerable>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.PublicIfInconvertible)]
        readonly partial struct IntOrString { }
        """,
        s => s.Constructors.All(c =>
            c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private
            || c.Parameters.Single().Type.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface));
    [Fact]
    public void GeneratesPrivateObjectAccessibilityForPublicIfInconvertible() =>
        UnionType(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Object>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.PublicIfInconvertible)]
        readonly partial struct IntOrString { }
        """,
        s => s.Constructors.All(c =>
            c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private
            || c.Parameters.Single().Type.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Object));
    [Fact]
    public void GeneratesPrivateSupertypeAccessibilityForPublicIfInconvertible() =>
        UnionType(
        """
        using RhoMicro.CodeAnalysis;
        class Supertype { }
        [UnionType<Supertype>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.PublicIfInconvertible)]
        partial class IntOrString : Supertype  { }
        """,
        s => s.Constructors.All(c =>
            c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private
            || c.Parameters.Single().Type.Name == "Supertype"),
        unionTypeName: "IntOrString");
    [Fact]
    public void GeneratesPrivateAccessibilityForPublicIfInconvertible() =>
        UnionType(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Int32>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.PublicIfInconvertible)]
        readonly partial struct IntOrString { }
        """,
        s => s.Constructors.All(c => c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private));
    [Fact]
    public void GeneratesPrivateAccessibilityForPrivate() =>
        UnionType(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Int32>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.Private)]
        readonly partial struct IntOrString { }
        """,
        s => s.Constructors.All(c => c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private));
    [Fact]
    public void GeneratesPublicAccessibilityForPublic() =>
        UnionType(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Int32>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.Public)]
        readonly partial struct IntOrString { }
        """,
        s => s.Constructors.All(c => c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public));
}
