namespace RhoMicro.CodeAnalysis.UtilityGenerators;
using System.Collections.Immutable;

partial class LibraryGenerator
{
    sealed class ImmutableArrayEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
    {
        public ImmutableArrayEqualityComparer(IEqualityComparer<T> elementComparer) => _elementComparer = elementComparer;
        public static readonly ImmutableArrayEqualityComparer<T> Default = new(EqualityComparer<T>.Default);
        private readonly IEqualityComparer<T> _elementComparer;

        public Boolean Equals(ImmutableArray<T> x, ImmutableArray<T> y)
        {
            if(x.Length != y.Length)
            {
                return false;
            }

            if(x.Length == 0)
            {
                return true;
            }

            for(var i = 0; i < x.Length; i++)
            {
                if(!_elementComparer.Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }
        public Int32 GetHashCode(ImmutableArray<T> obj)
        {
            var hashCode = 997021164;
            foreach(var element in obj)
            {
                hashCode = hashCode * -1521134295 + _elementComparer.GetHashCode(element);
            }

            return hashCode;
        }
    }
}
