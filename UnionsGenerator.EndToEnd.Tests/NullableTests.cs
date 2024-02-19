namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;

using System;

public partial class NullableTests
{
    [UnionType<Boolean?>]
    readonly partial struct NullableBoolUnion;
}
