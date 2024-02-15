using RhoMicro.CodeAnalysis;
#pragma warning disable

internal class Program
{
    private static void Main(String[] _0)
    { }
}

[UnionType<ErrorCode, MultipleUsersError, User>]
readonly partial struct GetUserResult;

sealed record User(String Name);

enum ErrorCode
{
    NotFound,
    Unauthorized
}

readonly record struct MultipleUsersError(Int32 Count);