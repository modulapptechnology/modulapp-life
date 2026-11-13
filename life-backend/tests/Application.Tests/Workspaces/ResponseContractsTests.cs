using System.Reflection;
using Application.Users;
using Application.Workspaces;

namespace Application.Tests.Workspaces;

public class ResponseContractsTests
{
    private static readonly string[] _sensitiveFragments = ["hash", "password", "secret", "salt", "signingkey"];

    private static IEnumerable<Type> ResponseTypes()
    {
        return typeof(LoginResponse).Assembly.GetExportedTypes()
            .Where(type => type.Name.EndsWith("Response", StringComparison.Ordinal)
                || type.Name.EndsWith("Summary", StringComparison.Ordinal));
    }

    [Fact]
    public void ResponseTypes_Always_ExposeNoHashesPasswordsOrSecrets()
    {
        var types = ResponseTypes();

        var sensitive = types
            .SelectMany(type => type.GetProperties(), (type, property) => $"{type.Name}.{property.Name}")
            .Where(name => _sensitiveFragments.Any(fragment =>
                name.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.Empty(sensitive);
    }

    [Fact]
    public void WorkspaceMemberSummary_Always_ExposesOnlyThePublicFields()
    {
        var type = typeof(WorkspaceMemberSummary);

        var fields = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .Order()
            .ToArray();

        Assert.Equal(["AvatarUrl", "Name", "Role", "UserId"], fields);
    }
}
