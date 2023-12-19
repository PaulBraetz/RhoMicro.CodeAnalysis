namespace RhoMicro.CodeAnalysis.UtilityGenerators;

using Microsoft.CodeAnalysis;

partial class LibraryGenerator
{
    readonly struct ArgInfo : IEquatable<ArgInfo>
    {
        public readonly String Type;

        private ArgInfo(String type) => Type = type;

        public static ArgInfo Create(IParameterSymbol parameter) =>
            new(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        public override Boolean Equals(Object? obj) => obj is ArgInfo info && Equals(info);
        public Boolean Equals(ArgInfo other) => Type == other.Type;

        public override Int32 GetHashCode()
        {
            var hashCode = -1979447941;
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Type);
            return hashCode;
        }

        public static Boolean operator ==(ArgInfo left, ArgInfo right) => left.Equals(right);
        public static Boolean operator !=(ArgInfo left, ArgInfo right) => !(left == right);
    }
}
