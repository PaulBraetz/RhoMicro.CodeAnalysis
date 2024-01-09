namespace RhoMicro.CodeAnalysis.DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ParserTests
{
    public void ParsesCorrectly(String source, Object expectedWeak, String[] expectedDiagnosticIds)
    {
        var expected = (RuleList)expectedWeak;
    }
}
