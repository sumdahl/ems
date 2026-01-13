using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameRolesToJobRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Departments_DepartmentId",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "JobRoles");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_DepartmentId",
                table: "JobRoles",
                newName: "IX_JobRoles_DepartmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobRoles",
                table: "JobRoles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_JobRoles_RoleId",
                table: "Employees",
                column: "RoleId",
                principalTable: "JobRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobRoles_Departments_DepartmentId",
                table: "JobRoles",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_JobRoles_RoleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_JobRoles_Departments_DepartmentId",
                table: "JobRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobRoles",
                table: "JobRoles");

            migrationBuilder.RenameTable(
                name: "JobRoles",
                newName: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_JobRoles_DepartmentId",
                table: "Roles",
                newName: "IX_Roles_DepartmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Roles_RoleId",
                table: "Employees",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Departments_DepartmentId",
                table: "Roles",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
