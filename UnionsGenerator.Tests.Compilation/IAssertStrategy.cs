namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

/// <summary>
/// Provides required assertions for bootstrapping generation tests.
/// </summary>
public interface IAssertStrategy
{
    /// <summary>
    /// Asserts the provided value not to be <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public void NotNull<T>(T? value);
    /// <summary>
    /// Asserts the provided value to be <see langword="true"/>.
    /// </summary>
    /// <param name="value"></param>
    public void True(Boolean value);
    /// <summary>
    /// Asserts the provided collection to be empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public void Empty<T>(IReadOnlyCollection<T> value);
}
