namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;

public partial class GeneratorTests
{
    [Theory]
    [InlineData(
        """
        [UnionType<string>]
        partial class Union;
        """,
        """        
        #region Scoped Data
        file static class Union_ScopedData
        {
        	public static System.Collections.Concurrent.ConcurrentDictionary<Type, Object> Cache { get; } = new();
        	public static System.Collections.Generic.HashSet<Type> RepresentableTypes { get; } = 
        	new ()
        	{
        		typeof(System.String)
        	}
        	;
        }
        #endregion
        """)]
    [InlineData(
        """
        partial class Union<[UnionType] T>;
        """,
        """        
        #region Scoped Data
        file static class Union_of_T_ScopedData<T>
        {
        	public static System.Collections.Concurrent.ConcurrentDictionary<Type, Object> Cache { get; } = new();
        	public static System.Collections.Generic.HashSet<Type> RepresentableTypes { get; } = 
        	new ()
        	{
        		typeof(T)
        	}
        	;
        }
        #endregion
        """)]
    [InlineData(
        """
        [UnionType<int>]
        partial class Union<[UnionType] T>;
        """,
        """        
        #region Scoped Data
        file static class Union_of_T_ScopedData<T>
        {
        	public static System.Collections.Concurrent.ConcurrentDictionary<Type, Object> Cache { get; } = new();
        	public static System.Collections.Generic.HashSet<Type> RepresentableTypes { get; } = 
        	new ()
        	{
        		typeof(System.Int32),
        		typeof(T)
        	}
        	;
        }
        #endregion
        """)]
    [InlineData(
        """
        [UnionType<int>]
        [UnionType<string>]
        partial class Union<[UnionType] T>;
        """,
        """        
        #region Scoped Data
        file static class Union_of_T_ScopedData<T>
        {
        	public static System.Collections.Concurrent.ConcurrentDictionary<Type, Object> Cache { get; } = new();
        	public static System.Collections.Generic.HashSet<Type> RepresentableTypes { get; } = 
        	new ()
        	{
        		typeof(System.Int32),
        		typeof(System.String),
        		typeof(T)
        	}
        	;
        }
        #endregion
        """)]
    public void ScopedData(String source, String expected)
    {
        //Arrange
        var declaredSource = "using RhoMicro.CodeAnalysis;" + source;
        var appendable = AppendableSourceText.Create(declaredSource, "Union");
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        var builder = new IndentedStringBuilder(IndentedStringBuilderOptions.Default with
        {
            NewLine = Environment.NewLine
        });
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

        //Act
        var actual = builder.Append(appendable.ScopedData).ToString();

        //Assert
        Assert.Equal(expected?.Trim('\r','\n'), actual.Trim('\r', '\n'));
    }
}
