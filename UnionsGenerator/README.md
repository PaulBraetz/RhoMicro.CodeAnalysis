# Unions

Read about union types here: https://en.wikipedia.org/wiki/Union_type

## Table of Contents

1. [Features](#Features)
2. [Alternative Union Type Implementations](#Alternative-Union-Type-Implementations)
3. [Installation](#Installation)
4. [How To Use](#How-To-Use)
5. [Contrived Example](#Contrived-Example)
6. [Why Not OneOf?](#Why-Not-OneOf?)

## Features

- generate rich examination and conversion api
- automatic relation type detection (congruency, superset, subset, intersection)
- generate conversion operators
- generate meaningful api names like `myUnion.IsResult` or `MyUnion.CreateFromResult(result)`
- generate the most efficient impementation for your usecase and optimize against boxing or size constraints

## Alternative Union Type Implementations
 
- [OneOf](https://github.com/mcintyre321/OneOf)
- [ValueVariant](https://github.com/hikarin522/ValueVariant)
- [DiscriminatedUnion](https://github.com/sdedalus/DiscriminatedUnion)
- [CSharpDiscriminatedUnion](https://github.com/Galad/CSharpDiscriminatedUnion)
- [UnionType](https://github.com/Cricle/UnionType)
- [Funcky Discriminated Union](https://github.com/polyadic/funcky-discriminated-union)
- [N.SourceGenerators.UnionTypes](https://github.com/Ne4to/N.SourceGenerators.UnionTypes)

## Installation

Package Reference:
```
	<ItemGroup>
	  <PackageReference Include="RhoMicro.CodeAnalysis.UnionsGenerator" Version="*">
	    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	    <PrivateAssets>all</PrivateAssets>
	  </PackageReference>
	</ItemGroup>
```
CLI:
```
dotnet add package RhoMicro.CodeAnalysis.UnionsGenerator
```

*Note: The source code generator will generate C#8 compliant code.*

## How To Use

Annotate your union type with the `UnionType` attribute:
```cs
[UnionType(typeof(String))]
[UnionType(typeof(Double))]
readonly partial struct Union;
```

Use your union type:
```cs
Union u = "Hello, World!"; //implicitly converted
u = 32; //implicitly converted
u = false; //CS0029	Cannot implicitly convert type 'bool' to 'Union'
```

### Available attributes and instructions:

#### `UnionTypeAttribute`

- `representableType`: Instruct the generator on the kind of representable type:
```cs
[UnionType(representableType: typeof(String))]
[UnionType(representableType: typeof(Double))]
readonly partial struct Union;
```

- `genericRepresentableTypeName`: use `nameof(T)` to use generic parameters as representable types (this will likely change in the future):
```cs
[UnionType(genericRepresentableTypeName:nameof(T))]
readonly partial struct Result<T>;
```

- `Alias`: define aliae for generated members, e.g.: 
```cs
Names n = "John";
if(n.IsSingleName)
{
    //...
} else if(n.IsMultipleNames)
{
    //...
}
[UnionType(typeof(List<String>), Alias = "MultipleNames")]
[UnionType(typeof(String), Alias = "SingleName")]
readonly partial struct Names;
```

- `Options`: define miscellaneous behaviour for the represented type:
```cs
/*
Instructs the generator to emit a superset conversion operator implementation even
the representable type is a generic type parameter. By default, it is omitted because of possible
unification for certain generic arguments.
*/
[UnionType(genericRepresentableTypeName:nameof(T), Alias = "Result", Options = UnionTypeOptions.SupersetOfParameter)]
readonly partial struct Result<T>;
```
```cs
/*
Instructs the generator to emit a superset conversion operator implementation even
the representable type is a generic type parameter. By default, it is omitted because of possible
unification for certain generic arguments.
*/
[UnionType(typeof(Int32), Options = UnionTypeOptions.ImplicitConversionIfSolitary)]
readonly partial struct Union;
```

- `Storage`: optimize the generated storage implementation for the representable type against boxing or size constraints:
<details>
<summary>
Available Storage Options: Auto, Reference, Value, Field
</summary>

```cs
public enum StorageOption
{
    // The generator will automatically decide on a storage strategy.

    // If the representable type is known to be a value type,
    // this will store values of that type inside a shared value type container.
    // Boxing will not occur.

    // If the representable type is known to be a reference type,
    // this will store values of that type inside a shared reference type container.

    // If the representable type is neither known to be a reference type
    // nor a value type, this option will cause values of that type to 
    // be stored inside a shared reference type container.
    // If the representable type is a generic type parameter,
    // boxing will occur for value type arguments to that parameter.
    Auto,

    // The generator will always store values of the representable type
    // inside a shared reference type container.

    // If the representable type is known to be a value type,
    // boxing will occur.

    // If the representable type is a generic type parameter,
    // boxing will occur for value type arguments to that parameter.
    Reference,

    // The generator will attempt to store values of the representable type
    // inside a value type container.

    // If the representable type is known to be a value type,
    // this will store values of that type inside a shared value type container.
    // Boxing will not occur.

    // If the representable type is known to be a reference type,
    // this will store values of that type inside a shared reference type container.
    // Boxing will not occur.

    // If the representable type is neither known to be a reference type
    // nor a value type, this option will cause values of that type to 
    // be stored inside a shared value type container.
    // If the representable type is a generic type parameter,
    // an exception of type TypeLoadException will occur for
    // reference type arguments to that parameter.
    Value,

    // The generator will attempt to store values of the representable type
    // inside a dedicated container for that type.

    // If the representable type is known to be a value type,
    // this will store values of that type inside a dedicated 
    // value type container.
    // Boxing will not occur.

    // If the representable type is known to be a reference type,
    // this will store values of that type inside a 
    // dedicated reference type container.

    // If the representable type is neither known to be a reference type
    // nor a value type, this option will cause values of that type to 
    // be stored inside a dedicated strongly typed container.
    // Boxing will not occur.
    Field
}
```

</details>

#### `UnionTypeSettingsAttribute`

This attribute may target either a union type or an assembly. When targeting a union type, it defines settings specific to that type. If, however, the attribute is annotating an assembly, it supplies the default settings for every union type in that assembly.

- `ConstructorAccessibility`: define the accessibility of generated constructors:
```cs
public enum ConstructorAccessibilitySetting
{
    // Generated constructors should always be private, unless
    // no conversion operators are generated for the type they
    // accept. This would be the case for interface types or
    // supertypes of the target union.
    PublicIfInconvertible,
    // Generated constructors should always be private.
    Private,
    // Generated constructors should always be public
    Public
}
```

- `DiagnosticsLevel`: define the reporting of diagnostics:
```cs
[Flags]
public enum DiagnosticsLevelSettings
{
    // Instructs the analyzer to report info diagnostics.
    Info = 0x01,
    // Instructs the analyzer to report warning diagnostics.
    Warning = 0x02,
    // Instructs the analyzer to report error diagnostics.
    Error = 0x04,
    // Instructs the analyzer to report all diagnostics.
    All = Info + Warning + Error
}
```

- `ToStringSetting`: define how implementations of `ToString` should be generated:
```cs
public enum ToStringSetting
{
    // The generator will emit an implementation that returns detailed information, including:
    // - the name of the union type
    // - a list of types representable by the union type
    // - an indication of which type is being represented by the instance
    // - the value currently being represented by the instance
    Detailed,
    // The generator will not generate an implementation of ToString.
    None,
    // The generator will generate an implementation that returns the result of
    // calling ToString on the currently represented value.
    Simple
}
```

- `Layout`: generate a layout attribute for size optimization
```cs
public enum LayoutSetting
{
    // Generate an annotation optimized for size.
    Small,
    // Do not generate any annotations.
    Auto
}
```


- Generic Names: define how generic type parameter names should be generated:
```cs
// Gets or sets the name of the generic parameter for generic Is, As and factory methods. 
// Set this property in order to avoid name collisions with generic union type parameters
public String GenericTValueName { get; set; }
// Gets or sets the name of the generic parameter for the Match method. 
// Set this property in order to avoid name collisions with generic union type parameters
public String MatchTypeName { get; set; }
```

#### `RelationAttribute`

This attribute defines a relation between the targeted union type the supplied type. The following relations are available:
- `None`
- `Congruent`
- `Superset`
- `Subset`
- `Intersection`

The generator will automatically detect the relation between two union types. The only requirement is for one of the two types to be annotated with the `RelationAttribute`:
```cs
[UnionType(typeof(DateTime))]
[UnionType(typeof(String))]
[UnionType(typeof(Double))]
[Relation(typeof(CongruentUnion))]
[Relation(typeof(SubsetUnion))]
[Relation(typeof(SupersetUnion))]
[Relation(typeof(IntersectionUnion))]
readonly partial struct Union;

[UnionType(typeof(Double))]
[UnionType(typeof(DateTime))]
[UnionType(typeof(String))]
sealed partial class CongruentUnion;

[UnionType(typeof(DateTime))]
[UnionType(typeof(String))]
partial class SubsetUnion;

[UnionType(typeof(DateTime))]
[UnionType(typeof(String))]
[UnionType(typeof(Double))]
[UnionType(typeof(Int32))]
partial struct SupersetUnion;

[UnionType(typeof(Int16))]
[UnionType(typeof(String))]
[UnionType(typeof(Double))]
[UnionType(typeof(List<Byte>))]
partial class IntersectionUnion;
```

## Contrived Example

In our imaginary usecase, a user shall be retrieved from the infrastructure via a name query. The following types will be found throughout the example:

```cs
sealed record User(String Name);

enum ErrorCode
{
    NotFound,
    Unauthorized
}

readonly record struct MultipleUsersError(Int32 Count);
```

The `User` type represents a user. The `ErrorCode` represents an error that does not contain additional information, like `MultipleUsersError` does. It represents multiple users having been found while only one was requested.

We define a union type to represent our imaginary query:

```cs
[UnionType(typeof(ErrorCode))]
[UnionType(typeof(MultipleUsersError))]
[UnionType(typeof(User))]
readonly partial struct GetUserResult;
```
Instances of `GetUserResult` can represent *either* an instance of `ErrorCode`, `MultipleUsersError` or `User`.

It will be used in a service façade like so:
```cs
interface IUserService
{
    GetUserResult GetUserByName(String name);
}
```
A repository abstracts over the underlying infrastructure:
```cs
interface IUserRepository
{
    IQueryable<User> UsersByName(String name);
}
```
Access violations would be communicated through the repository using the following exception type: 
```cs
sealed class UnauthorizedDatabaseAccessException : Exception;
```
An implementation of the `IUserService` is provided as follows:
```cs
sealed class UserService : IUserService
{
    public UserService(IUserRepository repository) => _repository = repository;

    private readonly IUserRepository _repository;

    public GetUserResult GetUserByName(String name)
    {
        IQueryable<User> users;
        try
        {
            users = _repository.UsersByName(name);
        } catch(UnauthorizedDatabaseAccessException)
        {
            return ErrorCode.Unauthorized;
        }

        var reifiedUsers = users.ToArray();
        if(reifiedUsers.Length == 0)
        {
            return ErrorCode.NotFound;
        } else if(reifiedUsers.Length > 1)
        {
            return new MultipleUsersError(reifiedUsers.Length);
        }

        return reifiedUsers[0];
    }
}
```
As you can see, possible representations of `GetUserResult` are implicitly converted and returned by the service. Users of `OneOf` will be familiar with this.

On the consumer side of this api, a generated `Match` function helps with transforming the union instance to another type:

```cs
sealed class UserModel
{
    public UserModel(IUserService service) => _service = service;

    private readonly IUserService _service;

    public String ErrorMessage { get; private set; } = String.Empty;
    public User? User { get; private set; }
    public void SetUser(String name)
    {
        var getUserResult = _service.GetUserByName(name);
        User = getUserResult.Match(
            HandleErrorCode,
            HandleMultipleResult,
            user => user);
    }
    private User? HandleErrorCode(ErrorCode code)
    {
        ErrorMessage = code switch
        {
            ErrorCode.NotFound => "The user could not be located.",
            ErrorCode.Unauthorized => "You are not authorized to access users.",
            _ => throw new NotImplementedException()
        };
        return null;
    }
    private User? HandleMultipleResult(MultipleUsersError result)
    {
        ErrorMessage = $"{result.Count} users have been located. The name was not precise enough.";
        return null;
    }
}
```

Here is a list of some generated members on the `GetUserResult` union type (implementations have been elided):
```cs
/*Factories*/
public static GetUserResult Create(ErrorCode value);
public static GetUserResult Create(MultipleUsersResult value);
public static GetUserResult Create(User value);
public static Boolean TryCreate<TValue>(TValue value, out GetUserResult instance);
public static GetUserResult Create<TValue>(TValue value);

/*Handling Methods*/
public void Switch(
    Action<ErrorCode> onErrorCode, 
    Action<MultipleUsersResult> onMultipleUsersResult,
    Action<User> onUser);

public TResult Match<TResult>(
    Func<ErrorCode, TResult> onErrorCode,
    Func<MultipleUsersResult, TResult> onMultipleUsersResult,
    Func<User, TResult> onUser);

/*Casting & Conversion*/
public Boolean Is<TValue>();

public Boolean Is(Type type);

public TValue As<TValue>();

public Boolean IsErrorCode;
public ErrorCode AsErrorCode;

public Boolean IsMultipleUsersResult;
public MultipleUsersResult AsMultipleUsersResult;

public Boolean IsUser;
public User AsUser;

public Type GetRepresentedType();

public static implicit operator GetUserResult(ErrorCode value);
public static explicit operator ErrorCode(GetUserResult union);

public static implicit operator GetUserResult(MultipleUsersResult value);
public static explicit operator MultipleUsersResult(GetUserResult union);

public static implicit operator GetUserResult(User value);
public static explicit operator User(GetUserResult union);
```

## Why Not OneOf?

Here are some issues that I have with `OneOf` that this generator aims to solve:
- the internal tag field is a byte instead of an int
- by default, not every representable type has a dedicated field generated for it
- generic value type unions are possible:
```cs
[UnionType(typeof(String))]
[UnionType(nameof(T), Alias = "Result")]
readonly partial struct Result<T>;
```
with helpful members like `IsResult` being generated for you.
- type order does not matter; convert to equivalent unions with ease:
```cs
Union u = DateTime.Now;
//Output: Union(<DateTime> | Double | String){23/11/2023 17:58:58}
Console.WriteLine(u);
EquivalentUnion eu = u.DownCast<EquivalentUnion>();
//Output: EquivalentUnion(<DateTime> | Double | String){23/11/2023 17:58:58}
Console.WriteLine(eu);

[UnionType(typeof(DateTime))]
[UnionType(typeof(String))]
[UnionType(typeof(Double))]
readonly partial struct Union;

[UnionType(typeof(Double))]
[UnionType(typeof(DateTime))]
[UnionType(typeof(String))]
sealed partial class EquivalentUnion;
```
- avoid `OneOf` invading your apis, instead rely on dedicated domain names for your union types
- use custom generated members with meaningful names for access to union data:
```cs
var r = Result<String>.CreateFromResult("Hello, World!");
if(r.IsErrorMessage)
{
    //handle error
} else if(r.IsResult)
{
    //handle result
}

//alternatively:
r.Switch(
    onErrorMessage: m =>/*handle error*/,
    onResult: r =>/*handle result*/);

[UnionType(typeof(String), Alias = "ErrorMessage")]
[UnionType(nameof(T), Alias = "Result")]
readonly partial struct Result<T>;
```
*Note that for generic union types, there will be no conversion operators generated for representable parameter types. That is why we are using the generated `CreateFromResult` factory method here.*
