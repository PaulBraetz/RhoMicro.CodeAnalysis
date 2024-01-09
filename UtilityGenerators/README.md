# What is this?

This project contains generators and analyzers that help writing generators and analyzers.

Currently, there are two generators contained within:
- [`LibraryGenerator`](#RhoMicro.CodeAnalysis.LibraryGenerator)
    - provides expanding macro string builders, diagnostics accumulators and more
- [`AttributeFactoryGenerator`](#RhoMicro.CodeAnalysis.AttributeFactoryGenerator)
    - automates parsing instructions from `AttributeData`

# Installation

```xml
<PackageReference Include="RhoMicro.CodeAnalysis.UtilityGenerators" Version="*">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

# LibraryGenerator

Generates types that help with source code generators & analyzers.
A more detailed section is coming soon.

# AttributeFactoryGenerator

Are you creating source generators for C#? Are you using attributes to allow your consumers to instruct your generator? Are you dissatisfied with the amount of boilerplate you have to write in order to extract those instructions from the roslyn api? 

Then this project could be of use to you!

## Key Features & Limitations
- Generate Factory for parsing attribute instance from `AttributeData`
- Generate helper functions for retrieving instances of your attribute from an `IEnumerable<AttributeData>`
- Parse type properties as `ITypeSymbol`s
- Generate the attribute source text itself (no requirement for second attribute library)

## How to use

Add a compilation flag to your generator project:
```xml
<PropertyGroup>
    <DefineConstants>$(DefineConstants);GENERATOR</DefineConstants>
</PropertyGroup>
```

Declare an attribute like so:
```cs
[AttributeUsage(AttributeTargets.Class)]
#if GENERATOR
[RhoMicro.CodeAnalysis.GenerateFactory]
#endif
public partial class TestGeneratorTargetAttribute : Attribute
{
#if GENERATOR
    [ExcludeFromFactory]
    private TestGeneratorTargetAttribute(System.Object typeSymbolContainer) =>
        _typeSymbolContainer = typeSymbolContainer;
#endif
    public TestGeneratorTargetAttribute(String name, Int32[] ages)
    {
        Name = name;
        Ages = ages;
    }
    public TestGeneratorTargetAttribute(Type type) => Type = type;

    public String Name { get; }
    public Int32[] Ages { get; }
    public Type? Type { get; set; }
}
```

The constructor enclosed in the preprocessor condition is required in order to construct an instance when the consumer made use of a constructor taking at least one parameter of type `Type`.

For every constructor that takes at least one parameter of type `Type`, an equivalent factory constructor is required. These are expected to take an instance of `Object` instead of the type and assign it to the generated helper field.

This way, a generated helper property of type `ITypeSymbol` may be used to retrieve the type used by the consumer in their `typeof` expression.


Use the generated factory and helper methods like so:
```cs
ImmutableArray<AttributeData> attributes = 
    symbol.GetAttributes();

IEnumerable<TestGeneratorTargetAttribute> allParsed =
    attributes.OfTestGeneratorTargetAttribute();

TestGeneratorTargetAttribute singleParsed =
    TestGeneratorTargetAttribute.TryCreate(attributes[0], out var a) ? 
    a : 
    null;

singleParsed = 
    symbol.TryGetFirstTestGeneratorTargetAttribute(out var a) ? 
    a : 
    null;

context.RegisterPostInitializationOutput(
    c => c.AddSource($"{nameof(TestGeneratorTargetAttribute)}.g.cs", TestGeneratorTargetAttribute.SourceText));
```
The generated extension method `OfTestGeneratorTargetAttribute` will return all instances of `TestGeneratorTargetAttribute` found in the symbols list of attributes.

The generated extension method `TryGetFirstTestGeneratorTargetAttribute` attempts to retrieve the first instance of `TestGeneratorTargetAttribute` found on the symbol.

The generated extension method `ForTestGeneratorTargetAttribute` makes use of the efficient `FAWMN` api.
