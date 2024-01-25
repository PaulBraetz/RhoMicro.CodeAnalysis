namespace RhoMicro.CodeAnalysis.Library.Text;

partial class IndentedStringBuilder
{
    public record OperatorsDecorator(IndentedStringBuilder Builder)
    {
        public static OperatorsDecorator operator +(OperatorsDecorator operators, String value)
        {
            operators.Builder.AppendCore(value);
            return operators;
        }
        public static OperatorsDecorator operator +(OperatorsDecorator operators, Char value)
        {
            operators.Builder.AppendCore(value);
            return operators;
        }
        public static OperatorsDecorator operator +(OperatorsDecorator operators, IIndentedStringBuilderAppendable value)
        {
            operators.Builder.AppendCore(value);
            return operators;
        }
        public override String ToString() => Builder.ToString();
        public virtual Boolean Equals(OperatorsDecorator? other) =>
            other != null &&
            other.Builder.Equals(Builder);
        public override Int32 GetHashCode() => Builder.GetHashCode();
    }
}
