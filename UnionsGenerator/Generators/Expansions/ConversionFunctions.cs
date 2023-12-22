﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Collections.Generic;
using System.Text;

sealed class ConversionFunctions(TargetDataModel model) : ExpansionBase(model, Macro.UnionConversion)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        _ = builder *
            "file static class " * Model.ConversionFunctionsTypeName /
            """
            {
                public static readonly global::System.Collections.Concurrent.ConcurrentDictionary<Type, Object> Cache = new();
            }

            """;
    }
}
