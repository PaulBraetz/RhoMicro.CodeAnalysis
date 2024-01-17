namespace UtilityGenerators.Library.Text.Tests;

using RhoMicro.CodeAnalysis.Library.Text;

public class IndentedStringBuilderTests
{
    [Theory]

    public void AppendsBlocksCorrectly(
        String expected,
        )
    {
        //Arrange
        var builder = new IndentedStringBuilder().Append(precedingText);

        //Act
        _ = builder.OpenBlock(new(
            open,
            close,
            EndsWithNewLine: endsWithNewLine,
            SpaceIfEmpty: spaceIfEmpty,
            StartOnNewLine: startsWithNewLine,
            IncreaseIndentation: increaseIndentation));
        _ = builder.Append(insertedText);
        _ = builder.CloseAllBlocks();
        var actual = builder.ToString();

        //Assert
        Assert.Equal(expected, actual);
    }
}