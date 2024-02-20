namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;
using System.Linq;

public class ConstructorTests : TestBase
{
    [Fact]
    public void GeneratesPrivateInterfaceAccessibilityForPublicIfInconvertible() =>
        TestUnionType(
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
        TestUnionType(
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
        TestUnionType(
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
        TestUnionType(
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
        TestUnionType(
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
        TestUnionType(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Int32>]
        [UnionType<System.String>]
        [UnionTypeSettings(ConstructorAccessibility = ConstructorAccessibilitySetting.Public)]
        readonly partial struct IntOrString { }
        """,
        s => s.Constructors.All(c => c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public));
}
