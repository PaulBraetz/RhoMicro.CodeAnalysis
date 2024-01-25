using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;

using System.Text;

internal class Program
{
    private static void Main(String[] _0)
    {

    }

    static void TestIntersectionMapping()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            interface T
            {
                public string M<S, U>(System.Int32 a1, System.String a2, out int a3, ref byte a4, params object[] a5);
            }
            """);
        var model = CSharpCompilation.Create(
            "TestAssembly",
            new[] { tree },
            new[] { MetadataReference.CreateFromFile(typeof(Object).Assembly.Location) }).GetSemanticModel(tree);
        var declaration = tree.GetRoot().ChildNodes().OfType<InterfaceDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(declaration)!;
        var method = symbol.GetMembers().OfType<IMethodSymbol>().Single();
        Console.WriteLine(GetSignatureString(method));
    }
    private static readonly SymbolDisplayFormat _fullStringFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
                    .WithMiscellaneousOptions(
                    /*
                        get rid of special types

                             10110
                        NAND 00100
                          => 10010

                             10110
                          &! 00100
                          => 10010

                             00100
                           ^ 11111
                          => 11011

                             10110
                           & 11011
                          => 10010
                    */
                    SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions &
                    (SymbolDisplayMiscellaneousOptions.UseSpecialTypes ^ (SymbolDisplayMiscellaneousOptions)Int32.MaxValue))
                    .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters);
    static String GetSignatureString(IMethodSymbol symbol)
    {
        var builder = new StringBuilder();
        AppendSignature(builder, symbol);
        var result = builder.ToString();

        return result;
    }
    static void AppendSignature(StringBuilder builder, IMethodSymbol symbol)
    {
        if(symbol.DeclaredAccessibility != Accessibility.Public ||
           symbol.IsStatic)
        {
            return;
        }

        if(symbol.ReturnsByRef)
        {
            _ = builder.Append("ref ");
        }

        if(symbol.ReturnsByRefReadonly)
        {
            _ = builder.Append("ref readonly ");
        }

        _ = builder.Append(symbol.ReturnType.ToDisplayString(_fullStringFormat))
            .Append(' ')
            .Append(symbol.ContainingType.ToDisplayString(_fullStringFormat))
            .Append('.')
            .Append(symbol.Name);

        if(symbol.TypeParameters.Length > 0)
        {
            _ = builder.Append('<').Append(symbol.TypeParameters[0].Name);
            for(var i = 1; i < symbol.TypeParameters.Length; i++)
            {
                var parameter = symbol.TypeParameters[i];
                _ = builder.Append(", ").Append(parameter.Name);
            }

            _ = builder.Append('>');
        }

        _ = builder.Append('(');
        if(symbol.Parameters.Length == 0)
        {
            _ = builder.Append(')');
            return;
        }

        AppendParameter(builder, symbol.Parameters[0]);
        for(var i = 1; i < symbol.Parameters.Length; i++)
        {
            _ = builder.Append(", ");
            var parameter = symbol.Parameters[i];
            AppendParameter(builder, parameter);
        }

        _ = builder.Append(')');
    }
    static void AppendParameter(StringBuilder builder, IParameterSymbol symbol)
    {
        if(symbol.IsParams)
        {
            _ = builder.Append("params ")
                .Append(symbol.Type.ToDisplayString(_fullStringFormat))
                .Append(' ')
                .Append(symbol.Name);

            return;
        }

        var refString = symbol.RefKind switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            _ => String.Empty
        };

        _ = builder.Append(refString)
            .Append(symbol.Type.ToDisplayString(_fullStringFormat))
            .Append(' ')
            .Append(symbol.Name);

        if(symbol.HasExplicitDefaultValue)
        {
            _ = builder.Append(" = ");
            Char? quote = symbol.Type.MetadataName == "System.String" ?
                '"' :
                symbol.Type.MetadataName == "System.Char" ?
                '\'' :
                null;
            _ = builder.Append(quote).Append(symbol.ExplicitDefaultValue ?? "null").Append(quote);
        }
    }
}

#pragma warning disable
interface T
{
    public string M<S, U>(System.Int32 a1, System.String a2, out int a3, ref byte a4, params object[] a5);
}
class S : T
{
    global::System.String global::T.M<S, U>(global::System.Int32 a1, global::System.String a2, out global::System.Int32 a3, ref global::System.Byte a4, params global::System.Object[] a5)
    {
        throw new NotImplementedException();
    }
}