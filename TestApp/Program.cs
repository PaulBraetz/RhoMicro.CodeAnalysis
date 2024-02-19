using RhoMicro.CodeAnalysis;

internal class Program
{
    private static void Main(String[] _0)
    { }
}

[UnionType<ErrorCode, MultipleUsersError, User>(Storage = StorageOption.Value)]
[UnionTypeSettings(Miscellaneous = MiscellaneousSettings.EmitStructuralRepresentation)]
readonly partial struct GetUserResult;

sealed record User(String Name);

enum ErrorCode
{
    NotFound,
    Unauthorized
}

readonly record struct MultipleUsersError(Int32 Count);