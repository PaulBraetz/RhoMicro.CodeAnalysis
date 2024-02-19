namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Basic.Reference.Assemblies;

using Microsoft.CodeAnalysis;

public class DiagnosticsLevelTests : TestBase
{
    public DiagnosticsLevelTests() : base(Net60.References.All) { }

    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;

        [UnionType<ErrorCode, MultipleUsersError, User>(Storage = StorageOption.Value)]
        [UnionTypeSettings(Miscellaneous = MiscellaneousSettings.EmitStructuralRepresentation, DiagnosticsLevel = DiagnosticsLevelSettings.Error)]
        readonly partial struct GetUserResult { }

        sealed record User(String Name) { }

        enum ErrorCode
        {
            NotFound,
            Unauthorized
        }

        readonly record struct MultipleUsersError(Int32 Count) { }
        """, "GetUserResult")]
    public void OmitsWarningDiagnostics(String source, String unionTypeName) =>
        TestDiagnostics(source, async compilation =>
        {
            var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
            Assert.All(diagnostics, d => Assert.Equal(DiagnosticSeverity.Error, d.Severity));
        });
}
