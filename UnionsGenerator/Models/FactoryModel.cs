﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

internal readonly struct FactoryModel
{
    private FactoryModel(Boolean requiresGeneration, String name)
    {
        RequiresGeneration = requiresGeneration;
        Name = name;
    }

    public readonly Boolean RequiresGeneration;
    public readonly String Name;

    public static void ConfigureModels(ITypeSymbol target, IEnumerable<RepresentableTypeModel> representableTypes)
    {
        var targetName = target.ToFullOpenString();
        var customFactoryNameMap = target.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.TryGetFirstUnionFactoryAttribute(out _) &&
                m.Parameters.Length == 1 &&
                !m.ReturnsVoid &&
                m.ReturnType.ToFullOpenString() == targetName)
            .Select(m => (Key: m.Parameters[0].Type.ToFullOpenString(), Value: m.Name))
            .GroupBy(t => t.Key)
            .Select(g => g.First())
            .ToDictionary(m => m.Key, m => m.Value);

        foreach(var representableType in representableTypes)
        {
            var representableTypeName = representableType.Names.FullTypeName;
            if(customFactoryNameMap.TryGetValue(representableTypeName, out var factoryName))
            {
                _ = customFactoryNameMap.Remove(representableTypeName);
                representableType.Factory = new(false, factoryName);
            } else
            {
                representableType.Factory = new(true, representableType.Names.GeneratedFactoryName);
            }
        }
    }
}
