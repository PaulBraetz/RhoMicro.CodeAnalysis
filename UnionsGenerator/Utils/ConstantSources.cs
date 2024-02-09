namespace RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System;

internal static partial class ConstantSources
{
    public static String GeneratedCode = $"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"RhoMicro.CodeAnalysis.UnionsGenerator\", \"{typeof(ConstantSources).Assembly.GetName().Version}\")]";
    public static String Util =
    $$"""
    namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generated
    {
        using System.Collections.Concurrent;
        using System.Collections.Generic;
        using System.Text;
        using System;

        [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
        file sealed class GeneratedUnionTypeMarkerAttribute : Attribute;

        [System.CodeDom.Compiler.GeneratedCodeAttribute("RhoMicro.CodeAnalysis.UnionsGenerator", "13.0.0.963")]
        file static class Util
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
                    var transitiveParameters = new List<(String Format, Type? Argument)>();
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
                        List<(String Format, Type? Argument)> transitiveArgumentsMap,
                        ref Int32 unboundTransitiveParameters)
                    {
    #if NETSTANDARD2_0
                        if(type.IsGenericParameter && type.DeclaringMethod is null)
    #else
                        if(type.IsGenericTypeParameter)
    #endif
                        {
                            var format = $"{Guid.NewGuid()}";
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
    #if NETSTANDARD2_0
                            builder.Append(type.Name.Substring(0, tickIndex)) :
    #else
                            builder.Append(type.Name.AsSpan(0, tickIndex)) :
    #endif
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
            public static System.Boolean IsMarked(Type type) => type.CustomAttributes.Any(a => a.AttributeType.FullName == "RhoMicro.CodeAnalysis.UnionsGenerator.Generated.GeneratedUnionTypeMarkerAttribute");

            private static readonly System.Collections.Concurrent.ConcurrentDictionary<(Type, Type), Object> _conversionImplementations = new();
            public static TTo UnsafeConvert<TFrom, TTo>(in TFrom from)
            {
                var impl = (System.Func<TFrom, TTo>)_conversionImplementations.GetOrAdd((typeof(TFrom), typeof(TTo)), k =>
                {
                    var param = System.Linq.Expressions.Expression.Parameter(k.Item1);
                    var castExpr = System.Linq.Expressions.Expression.Convert(param, k.Item2);
                    var lambda = System.Linq.Expressions.Expression.Lambda(castExpr, param).Compile();

                    return lambda;
                });
                var result = impl.Invoke(from);

                return result;
            }
        }
    }
    """;
    public const String MarkerAttribute = "[" + MarkerAttributeMetadataName + "]";
    public const String MarkerAttributeMetadataName = "RhoMicro.CodeAnalysis.UnionsGenerator.Generated.GeneratedUnionTypeMarkerAttribute";
    public const String EditorBrowsableNever = "[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]";
    public const String InvalidTagStateThrow = "throw new System.InvalidOperationException(\"Unable to determine the represented type or value. The union type was likely not initialized correctly.\")";
}