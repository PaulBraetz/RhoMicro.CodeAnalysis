namespace RhoMicro.CodeAnalysis.DslGenerator;

using System;

readonly struct Unit : IEquatable<Unit>
{
    public static Unit Instance { get; }

    public override Boolean Equals(Object? obj) => obj is Unit;
    public Boolean Equals(Unit _) => true;
    public override Int32 GetHashCode() => 0;

    public static Boolean operator ==(Unit _0, Unit _1) => true;
    public static Boolean operator !=(Unit _0, Unit _1) => false;
}
