using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MottuProjeto.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_VM_AREA",
                columns: table => new
                {
                    ID_AREA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_AREA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VM_AREA", x => x.ID_AREA);
                });

            migrationBuilder.CreateTable(
                name: "T_VM_MOTO",
                columns: table => new
                {
                    ID_MOTO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DS_PLACA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    NM_MODELO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ID_AREA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VM_MOTO", x => x.ID_MOTO);
                });

            migrationBuilder.CreateTable(
                name: "T_VM_USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_USUARIO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DS_EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_USERNAME = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false),
                    DS_PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DS_ROLE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_VM_USUARIO", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_VM_USUARIO_DS_EMAIL",
                table: "T_VM_USUARIO",
                column: "DS_EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_VM_USUARIO_DS_USERNAME",
                table: "T_VM_USUARIO",
                column: "DS_USERNAME",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_VM_AREA");

            migrationBuilder.DropTable(
                name: "T_VM_MOTO");

            migrationBuilder.DropTable(
                name: "T_VM_USUARIO");
        }
    }
}
