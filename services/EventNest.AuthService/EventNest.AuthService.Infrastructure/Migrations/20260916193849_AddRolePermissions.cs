using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventNest.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_role_id",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_role_permissions_role_permission",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_name" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO role_permissions (id, role_id, permission_name, created_at)
                SELECT gen_random_uuid(), r.id, v.permission_name, NOW()
                FROM roles r
                JOIN (VALUES
                  ('User','Events.View'), ('User','Tags.View'), ('User','RSVPs.View'),
                  ('User','RSVPs.Create'), ('User','RSVPs.Edit'), ('User','RSVPs.Cancel'),
                  ('Organizer','Events.View'), ('Organizer','Events.Create'), ('Organizer','Events.Edit'),
                  ('Organizer','Tags.View'), ('Organizer','Tags.Create'), ('Organizer','RSVPs.View'), ('Organizer','RSVPs.Manage'),
                  ('Moderator','Events.View'), ('Moderator','Events.Create'), ('Moderator','Events.Edit'),
                  ('Moderator','Tags.View'), ('Moderator','Tags.Create'), ('Moderator','RSVPs.View'),
                  ('Moderator','RSVPs.Manage'), ('Moderator','Users.View'),
                  ('Admin','Events.View'), ('Admin','Events.Create'), ('Admin','Events.Edit'), ('Admin','Events.Delete'),
                  ('Admin','Tags.View'), ('Admin','Tags.Create'), ('Admin','Tags.Edit'), ('Admin','Tags.Delete'),
                  ('Admin','RSVPs.View'), ('Admin','RSVPs.Create'), ('Admin','RSVPs.Edit'), ('Admin','RSVPs.Manage'),
                  ('Admin','RSVPs.Cancel'), ('Admin','Users.View'), ('Admin','Users.Manage'),
                  ('SuperAdmin','Events.View'), ('SuperAdmin','Events.Create'), ('SuperAdmin','Events.Edit'), ('SuperAdmin','Events.Delete'),
                  ('SuperAdmin','Tags.View'), ('SuperAdmin','Tags.Create'), ('SuperAdmin','Tags.Edit'), ('SuperAdmin','Tags.Delete'),
                  ('SuperAdmin','RSVPs.View'), ('SuperAdmin','RSVPs.Create'), ('SuperAdmin','RSVPs.Edit'), ('SuperAdmin','RSVPs.Manage'),
                  ('SuperAdmin','RSVPs.Cancel'), ('SuperAdmin','Users.View'), ('SuperAdmin','Users.Manage')
                ) AS v(role_name, permission_name) ON v.role_name = r.name
                ON CONFLICT (role_id, permission_name) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions");
        }
    }
}
