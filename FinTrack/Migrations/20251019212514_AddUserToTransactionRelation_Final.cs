using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToTransactionRelation_Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Крок 1: Додаємо стовпець, але тимчасово дозволяємо NULL
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Transactions",
                type: "text",
                nullable: true, // Дозволяємо NULL, щоб існуючі рядки не зламали міграцію
                defaultValue: null);

            // Крок 2: Заповнюємо існуючі записи дійсним ID користувача
            // ID користувача: 53ca7f78-772a-47e5-8c77-2a222ac1be3e
            migrationBuilder.Sql("UPDATE \"Transactions\" SET \"UserId\" = '53ca7f78-772a-47e5-8c77-2a222ac1be3e' WHERE \"UserId\" IS NULL;");

            // Крок 3: Робимо стовпець обов'язковим (NOT NULL), оскільки він тепер заповнений
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Transactions",
                type: "text",
                nullable: false, // Тепер це NOT NULL
                oldNullable: true);

            // Крок 4: Додаємо індекс (як було)
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            // Крок 5: Додаємо зовнішній ключ (як було, тепер він спрацює)
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");
        }
    }
}
