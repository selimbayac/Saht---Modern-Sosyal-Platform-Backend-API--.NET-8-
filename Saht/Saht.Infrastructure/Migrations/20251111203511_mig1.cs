using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saht.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blogs_CreatedAt",
                table: "Blogs");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_OwnerId",
                table: "Blogs");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_Slug",
                table: "Blogs");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_BlogId_CreatedAt",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "BlogId",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "BlogPosts");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "BlogPosts",
                newName: "EditedAt");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "BlogPosts",
                newName: "Body");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Blogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BlogPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorId",
                table: "BlogPosts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorId_CreatedAt",
                table: "BlogPosts",
                columns: new[] { "AuthorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CreatedAt",
                table: "BlogPosts",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Follows_FollowerId",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_AuthorId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_AuthorId_CreatedAt",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_CreatedAt",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BlogPosts");

            migrationBuilder.RenameColumn(
                name: "EditedAt",
                table: "BlogPosts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "BlogPosts",
                newName: "Content");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blogs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Blogs",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "BlogId",
                table: "BlogPosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "BlogPosts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CreatedAt",
                table: "Blogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_OwnerId",
                table: "Blogs",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_Slug",
                table: "Blogs",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_BlogId_CreatedAt",
                table: "BlogPosts",
                columns: new[] { "BlogId", "CreatedAt" });
        }
    }
}
