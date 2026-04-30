using FluentMigrator;

namespace Gauss.Database.Migrations.Migrations.Identity;

[Migration(202604300002)]
public sealed class M202604300002CreateIdentityUsers : Migration
{
    private const string IdentitySchema = "identity";
    private const string PlatformSchema = "platform";
    private const string UsersTable = "Users";
    private const string TenantsTable = "Tenants";

    public override void Up()
    {
        if (!Schema.Schema(IdentitySchema).Exists())
        {
            Create.Schema(IdentitySchema);
        }

        Create.Table(UsersTable)
            .InSchema(IdentitySchema)
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK_Identity_Users")
            .WithColumn("TenantId").AsGuid().NotNullable()
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Email").AsString(254).NotNullable()
            .WithColumn("NormalizedEmail").AsString(254).NotNullable()
            .WithColumn("PasswordHash").AsString(500).NotNullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("RegisteredAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("EmailConfirmedAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("LastLoginAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("LockedUntilUtc").AsDateTimeOffset().Nullable()
            .WithColumn("CreatedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.ForeignKey("FK_Identity_Users_Platform_Tenants_TenantId")
            .FromTable(UsersTable).InSchema(IdentitySchema).ForeignColumn("TenantId")
            .ToTable(TenantsTable).InSchema(PlatformSchema).PrimaryColumn("Id");

        Create.UniqueConstraint("UQ_Identity_Users_TenantId_NormalizedEmail")
            .OnTable(UsersTable)
            .WithSchema(IdentitySchema)
            .Columns("TenantId", "NormalizedEmail");

        Create.Index("IX_Identity_Users_TenantId")
            .OnTable(UsersTable)
            .InSchema(IdentitySchema)
            .OnColumn("TenantId");

        Create.Index("IX_Identity_Users_Status")
            .OnTable(UsersTable)
            .InSchema(IdentitySchema)
            .OnColumn("Status");
    }

    public override void Down()
    {
        Delete.Table(UsersTable).InSchema(IdentitySchema);
    }
}
