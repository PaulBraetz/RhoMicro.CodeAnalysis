﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

sealed partial class SourceModelBuilder
{
    private Boolean _isInitialized;

    private String? _hint;

    private String? _targetName;
    private String? _targetStructOrClass;
    private String? _targetNamespace;
    private String? _targetAccessibility;
    private String? _containingClassesHead;
    private String? _containingClassesTail;

    public void SetTarget(INamedTypeSymbol symbol)
    {
        _isInitialized = true;

        _hint = $"{symbol.ToHintName()}.g.cs";

        _containingClassesHead = symbol.GetContainingClassHead();
        _containingClassesTail = symbol.GetContainingClassTail();

        _targetName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters));

        _targetStructOrClass = symbol.IsValueType ?
            "struct" :
            "class";
        _targetNamespace = symbol.ContainingNamespace.IsGlobalNamespace ?
            String.Empty :
            $"namespace {symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}{{";
        _targetAccessibility = SyntaxFacts.GetText(symbol.DeclaredAccessibility);
    }

    private String? _conversionOperators;
    public void SetOperators(IEnumerable<ConversionOperatorModel> models)
    {
        _isInitialized = true;

        _conversionOperators = String.Join("\n", models.Select(m => m.SourceText));
    }

    private String? _constructors;
    public void SetConstructors(ConstructorsModel model)
    {
        _isInitialized = true;

        _constructors = model.SourceText;
    }

    private String? _nestedTypes;
    internal void SetNestedTypes(NestedTypesModel model)
    {
        _isInitialized = true;

        _nestedTypes = model.SourceText;
    }

    private String? _valueTypesContainerModel;
    internal void SetValueTypeContainer(ValueTypeContainerModel model)
    {
        _isInitialized = true;

        _valueTypesContainerModel = model.SourceText;
    }

    private String? _fields;
    internal void SetFields(FieldsModel model)
    {
        _isInitialized = true;

        _fields = model.SourceText;
    }

    private String? _toStringFunction;
    public void SetToStringFunction(ToStringFunctionModel model)
    {
        _isInitialized = true;

        _toStringFunction = model.SourceText;
    }

    private String? _interfaceImplementation;
    public void SetInterfaceImplementation(InterfaceImplementationModel model)
    {
        _isInitialized = true;

        _interfaceImplementation = model.SourceText;
    }

    private String? _getHashcodeFunction;
    public void SetGethashcodeFunction(GetHashcodeFunctionModel model)
    {
        _isInitialized = true;

        _getHashcodeFunction = model.SourceText;
    }

    private String? _equalsFunctions;
    public void SetEqualsFunctions(EqualsFunctionsModel model)
    {
        _isInitialized = true;

        _equalsFunctions = model.SourceText;
    }

    private String? _downCastFunction;
    public void SetDownCastFunction(DownCastFunctionModel model)
    {
        _isInitialized = true;

        _downCastFunction = model.SourceText;
    }

    private String? _switchMethod;
    public void SetSwitchMethod(SwitchMethodModel model)
    {
        _isInitialized = true;

        _switchMethod = model.SourceText;
    }

    private String? _matchFunction;
    public void SetMatchFunction(MatchFunctionModel model)
    {
        _isInitialized = true;

        _matchFunction = model.SourceText;
    }

    private String? _isAsFunctions;
    public void SetIsAsProperties(IsAsPropertiesModel model)
    {
        _isInitialized = true;

        _isAsFunctions = model.SourceText;
    }

    private String? _layout;
    public void SetLayout(LayoutModel model)
    {
        _isInitialized = true;

        _layout = model.SourceText;
    }

    private String? _getRepresentedTypeFunction;
    public void SetRepresentedTypesPropertiesFunction(RepresentedTypesPropertiesModel model)
    {
        _isInitialized = true;

        _getRepresentedTypeFunction = model.SourceText;
    }

    private String? _factoryFunctions;
    public void SetFactoryFunctions(FactoryFunctionsModel model)
    {
        _isInitialized = true;

        _factoryFunctions = model.SourceText;
    }

    public Model Build()
    {
        if(!_isInitialized)
        {
            return Model.Instance;
        }

        var builder = new StringBuilder()
            .AppendLine("// <auto-generated>")
            .AppendLine("// This file was generated using the RhoMicro.CodeAnalysis.UnionsGenerator.")
            .AppendLine("// </auto-generated>")
            .AppendLine("#pragma warning disable")
            .AppendLine(_targetNamespace)
            .AppendLine(_containingClassesHead)
            .AppendLine(_layout)
            .Append(_targetAccessibility).Append(" partial ").Append(_targetStructOrClass).Append(' ').AppendLine(_targetName).AppendLine(_interfaceImplementation)
            .Append('{')
            .AppendLine("#region Nested Types")
            .AppendLine(_nestedTypes)
            .AppendLine(_valueTypesContainerModel)
            .AppendLine("#endregion")
            .AppendLine("#region Constructors")
            .Append(_constructors)
            .AppendLine("#endregion")
            .AppendLine("#region Fields")
            .Append(_fields)
            .AppendLine("#endregion")
            .AppendLine("#region Factories")
            .AppendLine(_factoryFunctions)
            .AppendLine("#endregion")
            .AppendLine("#region Methods")
            .AppendLine(_downCastFunction)
            .AppendLine(_switchMethod)
            .AppendLine(_matchFunction)
            .AppendLine(_isAsFunctions)
            .AppendLine(_getRepresentedTypeFunction)
            .AppendLine("#endregion")
            .AppendLine("#region Overrides & Equality")
            .Append(_toStringFunction)
            .Append(_getHashcodeFunction)
            .Append(_equalsFunctions)
            .AppendLine("#endregion")
            .AppendLine("#region Conversion Operators")
            .AppendLine(_conversionOperators)
            .AppendLine("#endregion")
            .AppendLine("}")
            .AppendLine(_containingClassesTail)
            .AppendLine(String.IsNullOrEmpty(_targetNamespace) ? "" : "}");

        var source = builder.ToString();
        var formattedSource = CSharpSyntaxTree.ParseText(source)
                    .GetRoot()
                    .NormalizeWhitespace()
                    .SyntaxTree
                    .GetText()
                    .ToString();

        var hint = _hint ?? Guid.NewGuid().ToString();
        var result = new BuiltModel(formattedSource, hint);

        return result;
    }

    public SourceModelBuilder Clone() => new()
    {
        _isInitialized = _isInitialized,
        _targetName = _targetName,
        _targetStructOrClass = _targetStructOrClass,
        _targetNamespace = _targetNamespace,
        _targetAccessibility = _targetAccessibility,
        _conversionOperators = _conversionOperators,
        _switchMethod = _switchMethod,
        _matchFunction = _matchFunction,
        _downCastFunction = _downCastFunction,
        _equalsFunctions = _equalsFunctions,
        _getHashcodeFunction = _getHashcodeFunction,
        _toStringFunction = _toStringFunction,
        _fields = _fields,
        _constructors = _constructors,
        _nestedTypes = _nestedTypes,
        _interfaceImplementation = _interfaceImplementation,
        _isAsFunctions = _isAsFunctions,
        _layout = _layout,
        _containingClassesHead = _containingClassesHead,
        _containingClassesTail = _containingClassesTail,
        _getRepresentedTypeFunction = _getRepresentedTypeFunction,
        _factoryFunctions = _factoryFunctions,
        _valueTypesContainerModel = _valueTypesContainerModel,
        _hint = _hint
    };
}
