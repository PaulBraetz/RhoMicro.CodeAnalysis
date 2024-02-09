namespace RhoMicro.CodeAnalysis.UnionsGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Text;

internal static class Throw
{
    public static void ArgumentNull<T>(T value, String valueName)
        where T : class =>
        _ = value ?? throw new ArgumentNullException(valueName);
}
