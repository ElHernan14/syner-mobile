using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Syner.Api.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    )
                    .Annotation(
                        "MySql:ValueGenerationStrategy",
                        MySqlValueGenerationStrategy.IdentityColumn
                    ),

                    Nombre = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: false
                    )
                    .Annotation("MySql:CharSet", "utf8mb4"),

                    Descripcion = table.Column<string>(
                        type: "varchar(250)",
                        maxLength: 250,
                        nullable: false
                    )
                    .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                }
            )
            .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Nombre",
                table: "roles",
                column: "Nombre",
                unique: true
            );

            migrationBuilder.Sql("""
                INSERT INTO roles (Nombre, Descripcion)
                VALUES
                    ('admin', 'Administrador con acceso completo al sistema.'),
                    ('usuario', 'Usuario autenticado con acceso a las funcionalidades disponibles.');
                """);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "usuarios",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            )
            .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "RolId",
                table: "usuarios",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.Sql("""
                UPDATE usuarios u
                INNER JOIN roles r
                    ON LOWER(TRIM(u.Rol)) = LOWER(r.Nombre)
                SET u.RolId = r.Id;
                """);

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "usuarios"
            );

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_RolId",
                table: "usuarios",
                column: "RolId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_RolId",
                table: "usuarios",
                column: "RolId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_RolId",
                table: "usuarios"
            );

            migrationBuilder.DropIndex(
                name: "IX_usuarios_RolId",
                table: "usuarios"
            );

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "usuarios",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            )
            .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                UPDATE usuarios u
                INNER JOIN roles r
                    ON u.RolId = r.Id
                SET u.Rol = r.Nombre;
                """);

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "usuarios"
            );

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "usuarios"
            );

            migrationBuilder.DropTable(
                name: "roles"
            );
        }
    }
}