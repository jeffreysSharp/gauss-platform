using FluentMigrator;

namespace Gauss.Database.Migrations.Migrations.Identity;

[Migration(202605070001)]
public sealed class M202605070001CreateIdentityRolesAndPermissions : Migration
{
    private const string IdentitySchema = "identity";
    private const string PlatformSchema = "platform";

    private const string PermissionsTable = "Permissions";
    private const string RolesTable = "Roles";
    private const string RolePermissionsTable = "RolePermissions";
    private const string UserRolesTable = "UserRoles";
    private const string UsersTable = "Users";
    private const string TenantsTable = "Tenants";

    public override void Up()
    {
        if (!Schema.Schema(IdentitySchema).Exists())
        {
            Create.Schema(IdentitySchema);
        }

        CreatePermissionsTable();

        CreateRolesTable();

        CreateRolePermissionsTable();

        CreateUserRolesTable();
    }

    public override void Down()
    {
        Delete.Table(UserRolesTable).InSchema(IdentitySchema);
        Delete.Table(RolePermissionsTable).InSchema(IdentitySchema);
        Delete.Table(RolesTable).InSchema(IdentitySchema);
        Delete.Table(PermissionsTable).InSchema(IdentitySchema);
    }

    private void CreatePermissionsTable()
    {
        Create.Table(PermissionsTable)
            .InSchema(IdentitySchema)
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK_Identity_Permissions")
            .WithColumn("Code").AsString(150).NotNullable()
            .WithColumn("Description").AsString(300).NotNullable()
            .WithColumn("IsEnabled").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.UniqueConstraint("UQ_Identity_Permissions_Code")
            .OnTable(PermissionsTable)
            .WithSchema(IdentitySchema)
            .Column("Code");

        Create.Index("IX_Identity_Permissions_IsEnabled")
            .OnTable(PermissionsTable)
            .InSchema(IdentitySchema)
            .OnColumn("IsEnabled");
    }

    private void CreateRolesTable()
    {
        Create.Table(RolesTable)
            .InSchema(IdentitySchema)
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK_Identity_Roles")
            .WithColumn("TenantId").AsGuid().NotNullable()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("CreatedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.ForeignKey("FK_Identity_Roles_Platform_Tenants_TenantId")
            .FromTable(RolesTable).InSchema(IdentitySchema).ForeignColumn("TenantId")
            .ToTable(TenantsTable).InSchema(PlatformSchema).PrimaryColumn("Id");

        Create.UniqueConstraint("UQ_Identity_Roles_TenantId_Name")
            .OnTable(RolesTable)
            .WithSchema(IdentitySchema)
            .Columns("TenantId", "Name");

        Create.Index("IX_Identity_Roles_TenantId")
            .OnTable(RolesTable)
            .InSchema(IdentitySchema)
            .OnColumn("TenantId");

        Create.Index("IX_Identity_Roles_Status")
            .OnTable(RolesTable)
            .InSchema(IdentitySchema)
            .OnColumn("Status");
    }

    private void CreateRolePermissionsTable()
    {
        Create.Table(RolePermissionsTable)
            .InSchema(IdentitySchema)
            .WithColumn("RoleId").AsGuid().NotNullable()
            .WithColumn("PermissionId").AsGuid().NotNullable()
            .WithColumn("PermissionCode").AsString(150).NotNullable()
            .WithColumn("CreatedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.PrimaryKey("PK_Identity_RolePermissions")
            .OnTable(RolePermissionsTable)
            .WithSchema(IdentitySchema)
            .Columns("RoleId", "PermissionId");

        Create.ForeignKey("FK_Identity_RolePermissions_Identity_Roles_RoleId")
            .FromTable(RolePermissionsTable).InSchema(IdentitySchema).ForeignColumn("RoleId")
            .ToTable(RolesTable).InSchema(IdentitySchema).PrimaryColumn("Id");

        Create.ForeignKey("FK_Identity_RolePermissions_Identity_Permissions_PermissionId")
            .FromTable(RolePermissionsTable).InSchema(IdentitySchema).ForeignColumn("PermissionId")
            .ToTable(PermissionsTable).InSchema(IdentitySchema).PrimaryColumn("Id");

        Create.Index("IX_Identity_RolePermissions_PermissionId")
            .OnTable(RolePermissionsTable)
            .InSchema(IdentitySchema)
            .OnColumn("PermissionId");

        Create.Index("IX_Identity_RolePermissions_PermissionCode")
            .OnTable(RolePermissionsTable)
            .InSchema(IdentitySchema)
            .OnColumn("PermissionCode");
    }

    private void CreateUserRolesTable()
    {
        Create.Table(UserRolesTable)
            .InSchema(IdentitySchema)
            .WithColumn("UserId").AsGuid().NotNullable()
            .WithColumn("TenantId").AsGuid().NotNullable()
            .WithColumn("RoleId").AsGuid().NotNullable()
            .WithColumn("AssignedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.PrimaryKey("PK_Identity_UserRoles")
            .OnTable(UserRolesTable)
            .WithSchema(IdentitySchema)
            .Columns("UserId", "TenantId", "RoleId");

        Create.ForeignKey("FK_Identity_UserRoles_Identity_Users_UserId")
            .FromTable(UserRolesTable).InSchema(IdentitySchema).ForeignColumn("UserId")
            .ToTable(UsersTable).InSchema(IdentitySchema).PrimaryColumn("Id");

        Create.ForeignKey("FK_Identity_UserRoles_Platform_Tenants_TenantId")
            .FromTable(UserRolesTable).InSchema(IdentitySchema).ForeignColumn("TenantId")
            .ToTable(TenantsTable).InSchema(PlatformSchema).PrimaryColumn("Id");

        Create.ForeignKey("FK_Identity_UserRoles_Identity_Roles_RoleId")
            .FromTable(UserRolesTable).InSchema(IdentitySchema).ForeignColumn("RoleId")
            .ToTable(RolesTable).InSchema(IdentitySchema).PrimaryColumn("Id");

        Create.Index("IX_Identity_UserRoles_TenantId")
            .OnTable(UserRolesTable)
            .InSchema(IdentitySchema)
            .OnColumn("TenantId");

        Create.Index("IX_Identity_UserRoles_RoleId")
            .OnTable(UserRolesTable)
            .InSchema(IdentitySchema)
            .OnColumn("RoleId");
    }
}
