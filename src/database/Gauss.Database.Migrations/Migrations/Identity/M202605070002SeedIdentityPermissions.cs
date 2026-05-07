using FluentMigrator;

namespace Gauss.Database.Migrations.Migrations.Identity;

[Migration(202605070002)]
public sealed class M202605070002SeedIdentityPermissions : Migration
{
    private const string IdentitySchema = "identity";
    private const string PermissionsTable = "Permissions";

    private static readonly DateTimeOffset CreatedAtUtc =
        new(2026, 05, 07, 12, 0, 0, TimeSpan.Zero);

    public override void Up()
    {
        InsertPermission(
            Guid.Parse("8d1b2f8a-14e3-4a2f-9a76-0a2f7f0b1c01"),
            "Identity.Users.Read",
            "Allows reading identity users.");

        InsertPermission(
            Guid.Parse("6c0e8d61-0d84-4f7a-8c8d-95c0428e7e02"),
            "Identity.Users.Manage",
            "Allows managing identity users.");

        InsertPermission(
            Guid.Parse("4e62d99b-6a9d-4f57-9d11-9d5b2b4e6a03"),
            "Identity.Roles.Read",
            "Allows reading identity roles.");

        InsertPermission(
            Guid.Parse("9b7d9e5f-81de-4ef9-9a63-0c2a2e6f4d04"),
            "Identity.Roles.Manage",
            "Allows managing identity roles.");

        InsertPermission(
            Guid.Parse("e6cbf8f0-1e77-4d62-a36e-75b29141f705"),
            "Identity.Permissions.Read",
            "Allows reading identity permissions.");

        InsertPermission(
            Guid.Parse("2b841c5e-63d4-4a96-8d93-4a0e20f27a06"),
            "Identity.Tenant.Read",
            "Allows reading tenant information.");

        InsertPermission(
            Guid.Parse("7f0f64d4-55c1-4f5c-a89b-bc3f3cfe7107"),
            "Identity.Tenant.Manage",
            "Allows managing tenant information.");
    }

    public override void Down()
    {
        DeletePermission("Identity.Users.Read");
        DeletePermission("Identity.Users.Manage");
        DeletePermission("Identity.Roles.Read");
        DeletePermission("Identity.Roles.Manage");
        DeletePermission("Identity.Permissions.Read");
        DeletePermission("Identity.Tenant.Read");
        DeletePermission("Identity.Tenant.Manage");
    }

    private void InsertPermission(
    Guid id,
    string code,
    string description)
    {
        if (Schema.Schema(IdentitySchema).Table(PermissionsTable).Exists())
        {
            Execute.Sql($"""
            IF NOT EXISTS (
                SELECT 1
                FROM [identity].[Permissions]
                WHERE [Code] = '{code}'
                  AND [IsDeleted] = 0
            )
            BEGIN
                INSERT INTO [identity].[Permissions]
                (
                    [Id],
                    [Code],
                    [Description],
                    [IsEnabled],
                    [CreatedAtUtc],
                    [UpdatedAtUtc],
                    [IsDeleted]
                )
                VALUES
                (
                    '{id}',
                    '{code}',
                    '{description}',
                    1,
                    '{CreatedAtUtc:O}',
                    NULL,
                    0
                );
            END
            """);
        }
    }

    private void DeletePermission(string code)
    {
        Delete.FromTable(PermissionsTable)
            .InSchema(IdentitySchema)
            .Row(new
            {
                Code = code
            });
    }
}
