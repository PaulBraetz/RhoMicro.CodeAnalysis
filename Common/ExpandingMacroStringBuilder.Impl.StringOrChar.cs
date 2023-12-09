namespace RhoMicro.CodeAnalysis.Common;

using System;

partial class ExpandingMacroStringBuilder
{
    partial class Impl<TMacro>
    {
        readonly struct StringOrChar
        {
            public readonly Char Char;
            public readonly String? String;
            public readonly Boolean IsChar;

            public StringOrChar(Char @char)
            {
                Char = @char;
                String = null;
                IsChar = true;
            }
            public StringOrChar(String @string)
            {
                Char = default;
                String = @string;
                IsChar = false;
            }

            public static implicit operator StringOrChar(String @string) => new(@string);
            public static implicit operator StringOrChar(Char @char) => new(@char);

            public override String ToString() => IsChar ? Char.ToString() : String!;
        }
    }
}
