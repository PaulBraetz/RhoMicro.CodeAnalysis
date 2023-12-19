namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;

/// <summary>
/// Represents an action appending to an expanding macro string builder.
/// </summary>
/// <typeparam name="TMacro">The type of macro to support.</typeparam>
/// <param name="builder">The builder to append to.</param>
/// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
delegate void Appendix<TMacro>(IExpandingMacroStringBuilder<TMacro> builder, CancellationToken cancellationToken);

/// <summary>
/// Represents an action appending to an expanding macro string builder.
/// </summary>
/// <typeparam name="TMacro">The type of macro to support.</typeparam>
/// <typeparam name="TModel">The type of model to pass to the appendix.</typeparam>
/// <param name="builder">The builder to append to.</param>
/// <param name="model">The model to pass to the appendix.</param>
/// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
delegate void Appendix<TMacro, TModel>(IExpandingMacroStringBuilder<TMacro> builder, TModel model, CancellationToken cancellationToken);

/// <summary>
/// Contains factory and extension methods for expanding macro string builders.
/// </summary>
static partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Creates a new expanding macro string builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="providers">An optional collection of expansion providers to initialize the builder with.</param>
    /// <returns>A new expanding macro string builder.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Create<TMacro>(params IMacroExpansion<TMacro>[] providers) =>
        providers.Aggregate(Create<TMacro>(), static (b, w) => b.Receive(w, default));
    /// <summary>
    /// Creates a new expanding macro string builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <returns>A new expanding macro string builder.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Create<TMacro>() => Impl<TMacro>.Create();
    /// <summary>
    /// Applies an error intercepting proxy to an expanding macro string builder.
    /// Upon the diagnostics accumulator provided reporting errors, no further appendices 
    /// will be passed through to the underlying builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TModel">The type of model diagnosed by the diagnostics.</typeparam>
    /// <param name="builder">The builder to apply error interception to.</param>
    /// <param name="diagnostics">The diagnostics based on which to intercept appendices.</param>
    /// <returns>A new reference to the proxy, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> InterceptErrors<TMacro, TModel>(this IExpandingMacroStringBuilder<TMacro> builder, IDiagnosticsAccumulator<TModel> diagnostics) =>
        ErrorProxy<TMacro, TModel>.Apply(builder, diagnostics);
    /// <summary>
    /// Applies a formatting decorator to an expanding macro string builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to apply formatting to.</param>
    /// <param name="options">The options to use when formatting.</param>
    /// <returns>A new reference to the decorator, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Format<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        CSharpBuilderFormattingOptions? options = null)
    {
        options ??= CSharpBuilderFormattingOptions.Default;

        if(builder is CSharpFormatter<TMacro> decorator)
        {
            return decorator.WithOptions(options.Value);
        }

        return new CSharpFormatter<TMacro>(builder, options.Value);
    }

    /// <summary>
    /// Registers an expansion provider to the builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to register the provider to.</param>
    /// <param name="macro">The macro to expansion text for using the provider registered.</param>
    /// <param name="strategy">The strategy using which to write expansion text to a string builder.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Receive<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        TMacro macro,
        Action<IExpandingMacroStringBuilder<TMacro>, CancellationToken> strategy,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Receive(MacroExpansion.Create(macro, strategy), cancellationToken);
    }
    /// <summary>
    /// Registers a collection of expansion providers to the builder.
    /// </summary>
    /// <param name="builder">The builder to append expansion providers to.</param>
    /// <param name="providers">The providers to register.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Receive<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        IEnumerable<IMacroExpansion<TMacro>> providers,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = providers ?? throw new ArgumentNullException(nameof(providers));

        foreach(var provider in providers)
        {
            builder = builder.Receive(provider, cancellationToken);
        }

        return builder;
        ;
    }
    /// <summary>
    /// Appends an appendix to an expanding macro string builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append the appendix to.</param>
    /// <param name="appendix">The appendix to append to the builder.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Append<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        Appendix<TMacro> appendix,
        CancellationToken cancellationToken)
    {
        appendix.Invoke(builder, cancellationToken);
        return builder;
    }
    /// <summary>
    /// Appends an appendix to an expanding macro string builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TModel">The type of model to pass to the appendix.</typeparam>
    /// <param name="builder">The builder to append to.</param>
    /// <param name="model">The model to pass to the appendix.</param>
    /// <param name="appendix">The appendix to append to the builder.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> Append<TMacro, TModel>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        Appendix<TMacro, TModel> appendix,
        TModel model,
        CancellationToken cancellationToken)
    {
        appendix.Invoke(builder, model, cancellationToken);
        return builder;
    }
    /// <summary>
    /// Creates a decorator around the provided builder enabling the use of operators instead of append, append macro and receive.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to decorate.</param>
    /// <param name="cancellationToken">
    /// The cancellation token used to signal expansion to cancel.
    /// This token will be ambiently passed to calls backing the operators provided.
    /// </param>
    /// <returns>The new decorator.</returns>
    public static OperatorsDecorator<TMacro> WithOperators<TMacro>(this IExpandingMacroStringBuilder<TMacro> builder, CancellationToken cancellationToken) =>
        OperatorsDecorator<TMacro>.Create(builder, cancellationToken);
    /// <summary>
    /// Creates a decorator around the provided builder enabling the use of operators instead of append, append macro and receive.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <typeparam name="TModel">The type of model to support appendices for.</typeparam>
    /// <param name="builder">The builder to decorate.</param>
    /// <param name="cancellationToken">
    /// The cancellation token used to signal expansion to cancel.
    /// This token will be ambiently passed to calls backing the operators provided.
    /// </param>
    /// <returns>The new decorator.</returns>
    public static OperatorsDecorator<TMacro, TModel> WithOperators<TMacro, TModel>(this IExpandingMacroStringBuilder<TMacro> builder, CancellationToken cancellationToken) =>
        OperatorsDecorator<TMacro, TModel>.Create(builder, cancellationToken);
    #region Append Value
    /// <summary>
    /// Appends to the builder a single newline character.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append to.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendLine<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Append('\n');
    }
    /// <summary>
    /// Appends a generator marker attribute, as well as a pragma for dsabling all warnings to the builder.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append the header to.</param>
    /// <param name="generatorName">The name of the generator, for adding a meaningful comment.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendHeader<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        String generatorName)
        => builder.AppendLine("// <auto-generated>")
        .Append("// This file was last generated by the ").Append(generatorName).Append(" on ").AppendLine(DateTimeOffset.Now.ToString())
        .AppendLine("// </auto-generated>")
        .AppendLine("#pragma warning disable");
    /// <summary>
    /// Appends to the builder a string value, followed by a single newline character.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append to.</param>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendLine<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        String value)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Append(value).AppendLine();
    }

    /// <summary>
    /// Appends to the builder a character value, followed by a single newline character.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append to.</param>
    /// <param name="value">The value to append.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendLine<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        Char value)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.Append(value).AppendLine();
    }

    /// <summary>
    /// Using an expanding macro string builder, appends the elements of an enumeration, inserting
    /// the specified separator between the elements.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append the separated elements to.</param>
    /// <param name="separator">The string to use as a separator.</param>
    /// <param name="values">An enumeration containing values to append.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        String separator,
        IEnumerable<T> values)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = values ?? throw new ArgumentNullException(nameof(values));

        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        _ = builder.Append(iterator.Current?.ToString() ?? String.Empty);

        while(iterator.MoveNext())
        {
            _ = builder.Append(separator)
                .Append(iterator.Current?.ToString() ?? String.Empty);
        }

        return builder;
    }
    /// <summary>
    /// Using an expanding macro string builder, appends the elements of an enumeration, inserting
    /// the specified separator between the elements.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append the separated elements to.</param>
    /// <param name="separator">The character to use as a separator.</param>
    /// <param name="values">An enumeration containing values to append.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        Char separator,
        IEnumerable<T> values)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = values ?? throw new ArgumentNullException(nameof(values));

        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        _ = builder.Append(iterator.Current?.ToString() ?? String.Empty);

        while(iterator.MoveNext())
        {
            _ = builder.Append(separator)
                .Append(iterator.Current?.ToString() ?? String.Empty);
        }

        return builder;
    }
    /// <summary>
    /// Applies an aggregation function over an enumeration. 
    /// An expanding macro string builder is used as the initial accumulator value.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to aggregate elements in.</param>
    /// <param name="values">The elements to aggregate in the builder.</param>
    /// <param name="aggregation">The aggregation function to apply.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        IEnumerable<T> values,
        Func<IExpandingMacroStringBuilder<TMacro>, T, CancellationToken, IExpandingMacroStringBuilder<TMacro>> aggregation,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return values.Aggregate(builder, (b, e) => aggregation.Invoke(b, e, cancellationToken));
    }

    /// <summary>
    /// Applies an aggregation function over an enumeration, separating 
    /// aggregation results with the separator specified.
    /// An expanding macro string builder is used as the initial accumulator value.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to aggregate elements in.</param>
    /// <param name="separator">The string to use as a separator.</param>
    /// <param name="values">The elements to aggregate in the builder.</param>
    /// <param name="aggregation">The aggregation function to apply.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        String separator,
        IEnumerable<T> values,
        Func<IExpandingMacroStringBuilder<TMacro>, T, CancellationToken, IExpandingMacroStringBuilder<TMacro>> aggregation,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = values ?? throw new ArgumentNullException(nameof(values));
        _ = aggregation ?? throw new ArgumentNullException(nameof(aggregation));

        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        builder = aggregation.Invoke(builder, iterator.Current, cancellationToken);

        while(iterator.MoveNext())
        {
            _ = builder.Append(separator);
            builder = aggregation.Invoke(builder, iterator.Current, cancellationToken);
        }

        return builder;
    }
    /// <summary>
    /// Applies an aggregation function over an enumeration, separating 
    /// aggregation results with the separator specified.
    /// An expanding macro string builder is used as the initial accumulator value.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to aggregate elements in.</param>
    /// <param name="separator">The character to use as a separator.</param>
    /// <param name="values">The elements to aggregate in the builder.</param>
    /// <param name="aggregation">The aggregation function to apply.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        Char separator,
        IEnumerable<T> values,
        Func<IExpandingMacroStringBuilder<TMacro>, T, CancellationToken, IExpandingMacroStringBuilder<TMacro>> aggregation,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = values ?? throw new ArgumentNullException(nameof(values));
        _ = aggregation ?? throw new ArgumentNullException(nameof(aggregation));

        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        builder = aggregation.Invoke(builder, iterator.Current, cancellationToken);

        while(iterator.MoveNext())
        {
            _ = builder.Append(separator);
            builder = aggregation.Invoke(builder, iterator.Current, cancellationToken);
        }

        return builder;
    }
    #endregion
    #region Append Macro
    /// <summary>
    /// Appends all values from an enumeration type macro to an expanding macro string builder.
    /// The macro values will be appended in ascending order.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append macros to.</param>
    /// <param name="comparer">The comparer to use when ordering macro values.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendMacros<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        IComparer<TMacro> comparer,
        CancellationToken cancellationToken = default)
        where TMacro : struct, Enum
    {
        var result = Enum.GetValues(typeof(TMacro))
            .OfType<TMacro>()
            .OrderBy(m => m, comparer)
            .Aggregate(builder, (b, p) => b.AppendMacro(p, cancellationToken));

        return result;
    }
    /// <summary>
    /// Appends all values from an enumeration type macro to an expanding macro string builder.
    /// The macro values will be appended in ascending order.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append macros to.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendMacros<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        CancellationToken cancellationToken = default)
        where TMacro : struct, Enum => builder.AppendMacros(Comparer<TMacro>.Default, cancellationToken);
    /// <summary>
    /// Appends to the builder a macro, followed by a single newline character.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append to.</param>
    /// <param name="macro">The macro to append.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendMacroLine<TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        TMacro macro,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return builder.AppendMacro(macro, cancellationToken).AppendLine();
    }

    /// <summary>
    /// Using an expanding macro string builder, appends the elements of an enumeration, inserting
    /// the specified separator between the elements.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to append the separated elements to.</param>
    /// <param name="separator">The macro to use as a separator.</param>
    /// <param name="values">An enumeration containing macros to append.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendMacroJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        TMacro separator,
        IEnumerable<T> values,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = values ?? throw new ArgumentNullException(nameof(values));

        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        _ = builder.Append(iterator.Current?.ToString() ?? String.Empty);

        while(iterator.MoveNext())
        {
            _ = builder.AppendMacro(separator, cancellationToken)
                .Append(iterator.Current?.ToString() ?? String.Empty);
        }

        return builder;
    }
    /// <summary>
    /// Applies an aggregation function over an enumeration, separating 
    /// aggregation results with the separator specified.
    /// An expanding macro string builder is used as the initial accumulator macro.
    /// </summary>
    /// <typeparam name="T">The type of element contained in the enumeration.</typeparam>
    /// <typeparam name="TMacro">The type of macro to support.</typeparam>
    /// <param name="builder">The builder to aggregate elements in.</param>
    /// <param name="separator">The macro to use as a separator.</param>
    /// <param name="values">The elements to aggregate in the builder.</param>
    /// <param name="aggregation">The aggregation function to apply.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the source generation to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further method calls.</returns>
    public static IExpandingMacroStringBuilder<TMacro> AppendMacroJoin<T, TMacro>(
        this IExpandingMacroStringBuilder<TMacro> builder,
        TMacro separator,
        IEnumerable<T> values,
        Func<IExpandingMacroStringBuilder<TMacro>, T, CancellationToken, IExpandingMacroStringBuilder<TMacro>> aggregation,
        CancellationToken cancellationToken = default)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        _ = values ?? throw new ArgumentNullException(nameof(values));
        _ = aggregation ?? throw new ArgumentNullException(nameof(aggregation));

        using var iterator = values.GetEnumerator();

        if(iterator == null || !iterator.MoveNext())
        {
            return builder;
        }

        builder = aggregation.Invoke(builder, iterator.Current, cancellationToken);

        while(iterator.MoveNext())
        {
            _ = builder.AppendMacro(separator, cancellationToken);
            builder = aggregation.Invoke(builder, iterator.Current, cancellationToken);
        }

        return builder;
    }
    #endregion
}
