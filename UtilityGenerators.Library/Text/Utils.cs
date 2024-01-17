namespace RhoMicro.CodeAnalysis.Library.Text;

using Microsoft.CodeAnalysis;

static partial class Utils
{
    public static String GetTypeModifiers(INamedTypeSymbol symbol) =>
        symbol.IsRecord
            ? symbol.IsReferenceType
                ? symbol.IsSealed ? "sealed record" : "record"
                : symbol.IsReadOnly ? "readonly record struct" : "record struct"
            : symbol.IsReferenceType
                ? symbol.IsSealed ? "sealed class" : "class"
                : symbol.IsReadOnly
                            ? symbol.IsRefLikeType ? "readonly ref struct" : "readonly struct"
                            : symbol.IsRefLikeType ? "ref struct" : "struct";
}
