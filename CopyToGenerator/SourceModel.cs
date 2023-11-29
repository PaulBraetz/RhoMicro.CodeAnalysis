namespace RhoMicro.CodeAnalysis.CopyToGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;

readonly struct SourceModel(String source, INamedTypeSymbol symbol, String areValueEqualSource) : IEquatable<SourceModel>
{
    public readonly String Source = source;
    public readonly String AreValueEqualSource=areValueEqualSource;
    public readonly INamedTypeSymbol Symbol = symbol;

    public SourceModel WithSource(String source) => new(source, Symbol, AreValueEqualSource);
    public SourceModel WithAreValueEqualSource(String areValueEqualSource) => new(Source, Symbol, areValueEqualSource);
    public override Boolean Equals(Object? obj) => obj is SourceModel model && Equals(model);
    public Boolean Equals(SourceModel other) => Source == other.Source && AreValueEqualSource == other.AreValueEqualSource;

    public override Int32 GetHashCode()
    {
        var hashCode = 793934499;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Source);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(AreValueEqualSource);
        return hashCode;
    }

    public static Boolean operator ==(SourceModel left, SourceModel right) => left.Equals(right);
    public static Boolean operator !=(SourceModel left, SourceModel right) => !(left == right);
}
