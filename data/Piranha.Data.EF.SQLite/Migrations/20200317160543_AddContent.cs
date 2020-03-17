using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Piranha.Data.EF.SQLite.Migrations
{
    public partial class AddContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Piranha_Content",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TypeId = table.Column<string>(maxLength: 64, nullable: false),
                    EnableComments = table.Column<bool>(nullable: false),
                    CloseCommentsAfterDays = table.Column<int>(nullable: false),
                    Route = table.Column<string>(maxLength: 256, nullable: true),
                    RedirectUrl = table.Column<string>(maxLength: 256, nullable: true),
                    RedirectType = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    Published = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Content", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentGroups",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    TypeName = table.Column<string>(maxLength: 255, nullable: false),
                    AssemblyName = table.Column<string>(maxLength: 255, nullable: false),
                    Title = table.Column<string>(maxLength: 128, nullable: false),
                    IsRoutedContent = table.Column<bool>(nullable: false),
                    IsPrimaryContent = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_Languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 64, nullable: false),
                    Slug = table.Column<string>(maxLength: 64, nullable: false),
                    Culture = table.Column<string>(maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContentId = table.Column<Guid>(nullable: false),
                    TypeId = table.Column<string>(maxLength: 256, nullable: false),
                    RegionId = table.Column<string>(maxLength: 64, nullable: false),
                    FieldId = table.Column<string>(maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentFields_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContentId = table.Column<Guid>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentRevisions_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentGroupTypes",
                columns: table => new
                {
                    GroupId = table.Column<string>(maxLength: 64, nullable: false),
                    TypeId = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentGroupTypes", x => new { x.GroupId, x.TypeId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentGroupTypes_Piranha_ContentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Piranha_ContentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentTranslations",
                columns: table => new
                {
                    ContentId = table.Column<Guid>(nullable: false),
                    LanguageId = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 128, nullable: false),
                    NavigationTitle = table.Column<string>(maxLength: 128, nullable: true),
                    Slug = table.Column<string>(maxLength: 128, nullable: false),
                    MetaTitle = table.Column<string>(maxLength: 128, nullable: true),
                    MetaKeywords = table.Column<string>(maxLength: 128, nullable: true),
                    MetaDescription = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentTranslations", x => new { x.ContentId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentTranslations_Piranha_Content_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Piranha_Content",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentTranslations_Piranha_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Piranha_Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piranha_ContentFieldTranslations",
                columns: table => new
                {
                    FieldId = table.Column<Guid>(nullable: false),
                    LanguageId = table.Column<Guid>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piranha_ContentFieldTranslations", x => new { x.FieldId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_Piranha_ContentFieldTranslations_Piranha_ContentFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Piranha_ContentFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Piranha_ContentFieldTranslations_Piranha_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Piranha_Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentFields_ContentId_RegionId_FieldId_SortOrder",
                table: "Piranha_ContentFields",
                columns: new[] { "ContentId", "RegionId", "FieldId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentFieldTranslations_LanguageId",
                table: "Piranha_ContentFieldTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentRevisions_ContentId",
                table: "Piranha_ContentRevisions",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentTranslations_LanguageId",
                table: "Piranha_ContentTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_ContentTranslations_Slug",
                table: "Piranha_ContentTranslations",
                column: "Slug",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Piranha_ContentFieldTranslations");

            migrationBuilder.DropTable(
                name: "Piranha_ContentGroupTypes");

            migrationBuilder.DropTable(
                name: "Piranha_ContentRevisions");

            migrationBuilder.DropTable(
                name: "Piranha_ContentTranslations");

            migrationBuilder.DropTable(
                name: "Piranha_ContentFields");

            migrationBuilder.DropTable(
                name: "Piranha_ContentGroups");

            migrationBuilder.DropTable(
                name: "Piranha_Languages");

            migrationBuilder.DropTable(
                name: "Piranha_Content");
        }
    }
}
