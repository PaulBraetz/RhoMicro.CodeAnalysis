namespace UnionsGenerator.Tests;

using Buildalyzer;

using System.Text.RegularExpressions;

[TestClass]
public partial class UnitTest1
{
    [GeneratedRegex(@"(?<=.*)(\d+)\.(\d+)\.(\d+).*(?=\.nupkg)")]
    private static partial Regex GetVersionPattern();
    [GeneratedRegex(@"(.*)(?=\.(\d+)\.(\d+)\.(\d+).*\.nupkg)")]
    private static partial Regex GetNamePattern();

    [TestMethod]
    public void TestMethod1()
    {
        var manager = new AnalyzerManager();
        //warning system version can't parse preview versions
        var nameVersionMap = Directory.GetFiles("nuget")
            .Select(static f => (version: GetVersionPattern().Match(f).Value, name: GetNamePattern().Match(f).Value))
            .Select(static t => (success: Version.TryParse(t.version, out var version), version, t.name))
            .Where(static t => t.success)
            .GroupBy(static t => t.name)
            .Select(static g => g.OrderByDescending(static t => t.version).First())
            .ToDictionary(static t => t.name, static t => t.version!);

        var analyzer = manager.GetProject(Path.Combine("Projects", "Executable", "testApp.csproj"));
        analyzer.SetGlobalProperty("GeneratorPackageVersion", nameVersionMap["RhoMicro.CodeAnalysis.UnionsGenerator"].ToString());
        analyzer.SetGlobalProperty("AbstractionsPackageVersion", nameVersionMap["RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions"].ToString());
    }
}