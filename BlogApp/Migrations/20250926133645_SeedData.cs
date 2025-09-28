using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlogApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "hashedpassword1", "alice" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "hashedpassword2", "bob" }
                });

            migrationBuilder.InsertData(
                table: "blogs",
                columns: new[] { "Id", "Description", "IsOpen", "OwnerId", "Title" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), "A blog about Alice's adventures.", true, new Guid("11111111-1111-1111-1111-111111111111"), "Alice's Adventures" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "A blog about Bob's thoughts.", true, new Guid("22222222-2222-2222-2222-222222222222"), "Bob's Thoughts" }
                });

            migrationBuilder.InsertData(
                table: "posts",
                columns: new[] { "Id", "BlogId", "Body", "CreatedAt", "OwnerId", "Title" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("44444444-4444-4444-4444-444444444444"), "This is the body of Alice's first post.", new DateTime(2025, 9, 26, 13, 36, 44, 888, DateTimeKind.Utc).AddTicks(3735), new Guid("11111111-1111-1111-1111-111111111111"), "Alice's First Post" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("66666666-6666-6666-6666-666666666666"), "This is the body of Bob's first post.", new DateTime(2025, 9, 26, 13, 36, 44, 888, DateTimeKind.Utc).AddTicks(3738), new Guid("22222222-2222-2222-2222-222222222222"), "Bob's First Post" }
                });

            migrationBuilder.InsertData(
                table: "comments",
                columns: new[] { "Id", "Body", "CreatedAt", "OwnerId", "PostId" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Great post, Alice!", new DateTime(2025, 9, 26, 13, 36, 44, 888, DateTimeKind.Utc).AddTicks(3756), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Thanks for sharing, Bob!", new DateTime(2025, 9, 26, 13, 36, 44, 888, DateTimeKind.Utc).AddTicks(3760), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("55555555-5555-5555-5555-555555555555") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "comments",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "comments",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "posts",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "posts",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "blogs",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "blogs",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
