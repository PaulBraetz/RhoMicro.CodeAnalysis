# RhoMicro.CopyTo

This source code generator for C# generates a `CopyTo` method for copying the contents of public `get`/`set` properties from one instance to another.

## How To Use

Annotate your type with the `GenerateCopyTo` Attribute found in the `RhoMicro.CodeAnalysis.CopyToGenerator` namespace:

```cs
using RhoMicro.GenerateCopyToGenerator;

[GenerateCopyTo]
partial class MyType
{
    public String Prop1 { get; set; }
    public Int32 Prop2 { get; set; }
}
```

The generator will generate a partial implementation like this:
```cs
partial class MyType
{
    public void CopyTo(MyType target)
    {
        target.Prop1 = Prop1;
        target.Prop2 = Prop2;
    }
}
```

## Features And Restrictions

The generator is able to copy:
- other copyable types
- instances of IEnumerable<> whose item type is also copyable
- using assignment for all other properties

The generator is limited to:
- public read/write properties
- reference types