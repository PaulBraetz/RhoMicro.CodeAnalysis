
namespace RhoMicro.CodeAnalysis.Library.Text
{
    using System.Threading;

    partial class IndentedStringBuilder
    {
        public record Operators(IndentedStringBuilder Builder, CancellationToken CancellationToken)
        {
            public static Operators operator +(Operators operators, String value)
            {
                operators.Builder.AppendCore(value);
                return operators;
            }
            public static Operators operator +(Operators operators, Char value)
            {
                operators.Builder.AppendCore(value);
                return operators;
            }
            public static Operators operator +(Operators operators, IIndentedStringBuilderAppendable value)
            {
                operators.Builder.AppendCore(value, operators.CancellationToken);
                return operators;
            }
            public override String ToString() => Builder.ToString();
            public virtual Boolean Equals(Operators? other) =>
                other != null &&
                other.Builder.Equals(Builder);
            public override Int32 GetHashCode() => Builder.GetHashCode();
        }
    }
}
