﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;

internal static class ConstantSources
{
    public const String EditorBrowsableNever = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]";
    public const String Util =
    """
    // <auto-generated>
    // This file was generated using the RhoMicro.CodeAnalysis.UnionsGenerator.
    // </auto-generated>
    #nullable enable
    #pragma warning disable

    namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generated;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System;

    internal static class Util
    {
        private readonly static ConcurrentDictionary<Type, String> _cache = new();
        public static String GetFullString(Type type) => _cache.GetOrAdd(type, ValueFactory);
        static String ValueFactory(Type type)
        {
            var result = getString(type, new());

            return result;

            static String getString(Type type, StringBuilder builder)
            {
                var unboundTransitiveParameters = 0;
                var transitiveParameters = new List<(String Format, Type Argument)>();
                append(type, builder, transitiveParameters, ref unboundTransitiveParameters);
                var result = builder.ToString();

                for(var i = 0; i < transitiveParameters.Count; i++)
                {
                    _ = builder.Clear();
                    var (format, argument) = transitiveParameters[i];
                    var replacement = getString(argument!, builder);
                    result = result.Replace(format, replacement);
                }

                return result;

                static void append(
                    Type type,
                    StringBuilder builder,
                    List<(String Format, Type Argument)> transitiveArgumentsMap,
                    ref Int32 unboundTransitiveParameters)
                {
                    if(type.IsGenericTypeParameter)
                    {
                        var format = $"{{{Guid.NewGuid()}}}";
                        _ = builder.Append(format);
                        transitiveArgumentsMap.Add((format, null));
                        unboundTransitiveParameters++;
                        return;
                    } else if(type.DeclaringType != null)
                    {
                        append(type.DeclaringType, builder, transitiveArgumentsMap, ref unboundTransitiveParameters);
                        _ = builder.Append('.');
                    } else if(type.Namespace != null)
                    {
                        _ = builder.Append(type.Namespace)
                                .Append('.');
                    }

                    var tickIndex = type.Name.IndexOf('`');
                    _ = tickIndex != -1 ?
                        builder.Append(type.Name.AsSpan(0, tickIndex)) :
                        builder.Append(type.Name);

                    var arguments = type.GetGenericArguments();
                    var inflectionPoint = unboundTransitiveParameters;
                    if(arguments.Length > 0 && unboundTransitiveParameters > 0)
                    {
                        for(; unboundTransitiveParameters > 0;)
                        {
                            unboundTransitiveParameters--;
                            var (format, _) = transitiveArgumentsMap[unboundTransitiveParameters];
                            transitiveArgumentsMap[unboundTransitiveParameters] = (format, arguments[unboundTransitiveParameters]);
                        }
                    }

                    if(arguments.Length > inflectionPoint)
                    {
                        _ = builder.Append('<');
                        append(arguments[inflectionPoint], builder, transitiveArgumentsMap, ref unboundTransitiveParameters);

                        for(var i = inflectionPoint + 1; i < type.GenericTypeArguments.Length; i++)
                        {
                            _ = builder.Append(", ");
                            append(arguments[i], builder, transitiveArgumentsMap, ref unboundTransitiveParameters);
                        }

                        _ = builder.Append('>');
                    }
                }
            }
        }

        public static TTo UnsafeConvert<TFrom, TTo>(in TFrom from)
        {
            var copy = from;

            return global::System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref copy);
        }
    }
    """;
    public const String InvalidTagStateThrow = "throw new global::System.InvalidOperationException(\"Unable to determine the represented type or value. The union type was likely not initialized correctly.\")";
}