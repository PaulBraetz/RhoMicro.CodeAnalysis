namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;
using System;
using System.Collections.Generic;

sealed class XunitAssertStrategy : IAssertStrategy
{
    private XunitAssertStrategy() { }
    public static XunitAssertStrategy Instance { get; } = new();
    public void Empty<T>(IReadOnlyCollection<T> value) => Assert.Empty(value);
    public void NotNull<T>(T? value) => Assert.NotNull(value);
    public void True(Boolean value) => Assert.True(value);
}