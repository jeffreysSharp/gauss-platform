using FluentMigrator;

namespace Gauss.Database.Migrations.Migrations.Platform;

[Migration(202604300001)]
public sealed class M202604300001CreatePlatformTenants : Migration
{
    private const string SchemaName = "platform";
    private const string TableName = "Tenants";

    public override void Up()
    {
        if (!Schema.Schema(SchemaName).Exists())
        {
            Create.Schema(SchemaName);
        }

        Create.Table(TableName)
            .InSchema(SchemaName)
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey("PK_Platform_Tenants")
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Slug").AsString(100).NotNullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("CreatedAtUtc").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAtUtc").AsDateTimeOffset().Nullable()
            .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.UniqueConstraint("UQ_Platform_Tenants_Slug")
            .OnTable(TableName)
            .WithSchema(SchemaName)
            .Column("Slug");

        Create.Index("IX_Platform_Tenants_Status")
            .OnTable(TableName)
            .InSchema(SchemaName)
            .OnColumn("Status");
    }

    public override void Down()
    {
        Delete.Table(TableName).InSchema(SchemaName);
    }
}
