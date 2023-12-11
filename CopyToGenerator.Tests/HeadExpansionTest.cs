namespace RhoMicro.CodeAnalysis.CopyToGenerator.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library.Tests;
using RhoMicro.CodeAnalysis.CopyToGenerator.ExpansionProviders;

public class HeadExpansionTest
{
    [Theory]
    [InlineData(
        """
        [GenerateCopyTo]
        partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        sealed partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        abstract partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        internal partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        internal sealed partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        internal abstract partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        public partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        public sealed partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        public abstract partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        private partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        private sealed partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    [InlineData(
        """
        [GenerateCopyTo]
        private abstract partial class Dto;
        """,
        """
        partial class Dto
        {
        """)]
    public void ProvidesExpectedExpansion(String sourceText, String expected)
    {
        ExpansionProviderFixture.Create(
            sourceText,
            expected,
            (s, m) =>
            {
                var node = s.GetRoot()
                    .ChildNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Single();
                var symbol = m.GetDeclaredSymbol(node) as INamedTypeSymbol
                    ?? throw new Exception("Unable to locate required model symbol.");

                var model = Model.Create(symbol);
                var result = new HeadExpansion(model);

                return result;
            })
            .Assert();
    }
}