namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhoMicro.CodeAnalysis;

public partial class EqualityGetHashCodeTests
{
    partial class Union<[UnionType] T>;

    public static Object[][] Int32Data =>
        Enumerable.Range(-25, 25)
        .Append(Int32.MinValue)
        .Append(Int32.MaxValue)
        .Select(i => new Object[] { i })
        .ToArray();

    public static Object?[][] StringData => new Object?[][]{
        ["Parturient, curae; "],
        [ "posuere volutpat consequat nullam mattis " ],
        [ "diam vel nascetur. Velit sit semper euismod mauris cursus auctor " ],
        [ "quis enim habitasse bibendum " ],
        [ "vel habitant. Aliquet himenaeos sociis interdum donec lacus sociosqu eu eu ante posuere. " ],
        [ "Tortor parturient rhoncus " ],
        [ "" ],
        [ null ],
        [ "urna eget himenaeos leo id eu. Rutrum " ],
        [ "sodales dolor viverra himenaeos etiam phasellus elit facilisis nibh praesent mattis odio. Magna fringilla." ]
    };

    [Theory]
    [MemberData(nameof(Int32Data))]
    [MemberData(nameof(StringData))]
    public void InstancesReportSameHashCode<T>(T value)
    {
        var expected = Union<T>.CreateFromT(value).GetHashCode();
        var actual = Union<T>.CreateFromT(value).GetHashCode();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Int32Data))]
    [MemberData(nameof(StringData))]
    public void InstancesAreEqual<T>(T value)
    {
        var expected = Union<T>.CreateFromT(value);
        var actual = Union<T>.CreateFromT(value);
        Assert.Equal(expected, actual);
    }
}
