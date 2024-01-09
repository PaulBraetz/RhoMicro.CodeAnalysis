namespace TestApp;

using RhoMicro.CodeAnalysis;

using System.Text.Json;

internal class Program
{
    private static void Main(String[] _0)
    {

    }

    private static void Test(Union before)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        Console.WriteLine("Before: " + before);
        var serialized = JsonSerializer.Serialize(before, options);
        Console.WriteLine(serialized);
        var after = JsonSerializer.Deserialize<Union>(serialized, options);
        Console.WriteLine("After: " + after);
    }
}
[UnionType(typeof(Int32))]
[UnionType(typeof(String))]
[UnionTypeSettings(GenerateJsonConverter = true)]
sealed partial class Union;
