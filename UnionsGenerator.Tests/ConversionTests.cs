namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

using System.Security.Cryptography.X509Certificates;

using Microsoft.CodeAnalysis;

public class ConversionTests : GeneratorTest
{
    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Collections.IEnumerable>]
        readonly partial struct Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.IDisposable>]
        readonly partial struct Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        interface IInterface { }
        [UnionType<IInterface>]
        readonly partial struct Union { }
        """)]
    public void OmitsInterfaceConversions(String source) =>
        TestUnionType(
            source,
            t => {/*diagnostics due to supertype/interface conversions would fail*/ },
            unionTypeName: "Union");

    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        [UnionType<String>]
        partial class Subset { }
        [UnionType<String>]
        [UnionType<Int32>]
        [Relation<Subset>]
        partial class Union { }
        """)]
    public void SubsetConversions(String source)
    {
        AssertConversionOperator(
            source,
            "Union",
            toPredicate: SymbolEqualityComparer.Default.Equals,
            fromPredicate: (parameter, union) => parameter.Name == "Subset",
            opName: WellKnownMemberNames.ImplicitConversionName);

        AssertConversionOperator(
            source,
            "Union",
            toPredicate: (parameter, union) => parameter.Name == "Subset",
            fromPredicate: SymbolEqualityComparer.Default.Equals,
            opName: WellKnownMemberNames.ExplicitConversionName);
    }

    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        [UnionType<String>]
        [UnionType<Int32>]
        [UnionType<Double>]
        partial class Superset { }
        [UnionType<String>]
        [UnionType<Int32>]
        [Relation<Superset>]
        partial class Union { }
        """)]
    public void SupersetConversions(String source)
    {
        AssertConversionOperator(
            source,
            "Union",
            toPredicate: SymbolEqualityComparer.Default.Equals,
            fromPredicate: (parameter, union) => parameter.Name == "Superset",
            opName: WellKnownMemberNames.ExplicitConversionName);

        AssertConversionOperator(
            source,
            "Union",
            toPredicate: (parameter, union) => parameter.Name == "Superset",
            fromPredicate: SymbolEqualityComparer.Default.Equals,
            opName: WellKnownMemberNames.ImplicitConversionName);
    }

    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        [UnionType<String>]
        [UnionType<Int32>]
        partial class Congruent { }
        [UnionType<String>]
        [UnionType<Int32>]
        [Relation<Congruent>]
        partial class Union { }
        """)]
    public void CongruentConversions(String source)
    {
        AssertConversionOperator(
            source,
            "Union",
            toPredicate: SymbolEqualityComparer.Default.Equals,
            fromPredicate: (parameter, union) => parameter.Name == "Congruent",
            opName: WellKnownMemberNames.ImplicitConversionName);

        AssertConversionOperator(
            source,
            "Union",
            toPredicate: (parameter, union) => parameter.Name == "Congruent",
            fromPredicate: SymbolEqualityComparer.Default.Equals,
            opName: WellKnownMemberNames.ImplicitConversionName);
    }

    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        [UnionType<String>]
        [UnionType<Double>]
        partial class Intersection { }
        [UnionType<String>]
        [UnionType<Int32>]
        [Relation<Intersection>]
        partial class Union { }
        """)]
    public void IntersectionConversions(String source)
    {
        AssertConversionOperator(
            source,
            "Union",
            toPredicate: SymbolEqualityComparer.Default.Equals,
            fromPredicate: (parameter, union) => parameter.Name == "Intersection",
            opName: WellKnownMemberNames.ExplicitConversionName);

        AssertConversionOperator(
            source,
            "Union",
            toPredicate: (parameter, union) => parameter.Name == "Intersection",
            fromPredicate: SymbolEqualityComparer.Default.Equals,
            opName: WellKnownMemberNames.ExplicitConversionName);
    }

    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        [UnionType<Double>]
        partial class Disjunct { }
        [UnionType<String>]
        [UnionType<Int32>]
        [Relation<Disjunct>]
        partial class Union { }
        """)]
    public void DisjunctConversions(String source)
    {
        AssertConversionOperator(
            source,
            "Union",
            toPredicate: SymbolEqualityComparer.Default.Equals,
            fromPredicate: (parameter, union) => parameter.Name == "Disjunct",
            opName: WellKnownMemberNames.ExplicitConversionName,
            assertExists: false);

        AssertConversionOperator(
            source,
            "Union",
            toPredicate: (parameter, union) => parameter.Name == "Disjunct",
            fromPredicate: SymbolEqualityComparer.Default.Equals,
            opName: WellKnownMemberNames.ExplicitConversionName,
            assertExists: false);
    }

    private void AssertConversionOperator(
        String source,
        String unionTypeName,
        Func<ITypeSymbol, INamedTypeSymbol, Boolean> toPredicate,
        Func<ITypeSymbol, INamedTypeSymbol, Boolean> fromPredicate,
        String opName,
        Boolean assertExists = true) =>
        TestUnionType(
            source,
            t =>
            {
                var exists = t.GetMembers()
                    .OfType<IMethodSymbol>()
                    .One(m =>
                    m.IsStatic
                 && m.MethodKind == MethodKind.Conversion
                 && m.Parameters.Length == 1
                 && toPredicate.Invoke(m.ReturnType, t)
                 && fromPredicate.Invoke(m.Parameters[0].Type, t)
                 && m.DeclaredAccessibility == Accessibility.Public
                 && ( m.Name == opName || !assertExists ));

                Assert.Equal(exists, assertExists);
            },
            unionTypeName);
}
