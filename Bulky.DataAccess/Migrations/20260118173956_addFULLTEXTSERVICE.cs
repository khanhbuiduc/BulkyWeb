using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulky.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addFULLTEXTSERVICE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Tạo Full-Text Catalog (nơi lưu trữ FTS indexes)
            // suppressTransaction: true - Chạy ngoài transaction
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ftCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;
                END
            ", suppressTransaction: true);

            // Bước 2: Tạo Full-Text Index trên bảng Products
            // Index 4 cột: Title, Author, ISBN, Description
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Products'))
                BEGIN
                    CREATE FULLTEXT INDEX ON Products(Title, Author, ISBN, Description)
                    KEY INDEX PK_Products
                    ON ftCatalog
                    WITH STOPLIST = SYSTEM;
                END
            ", suppressTransaction: true);

            // Bước 3 (Optional): Populate index ngay lập tức
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Products'))
                BEGIN
                    ALTER FULLTEXT INDEX ON Products START FULL POPULATION;
                END
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Xóa Full-Text Index trước
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Products'))
                BEGIN
                    DROP FULLTEXT INDEX ON Products;
                END
            ", suppressTransaction: true);

            // Rollback: Xóa Full-Text Catalog sau
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ftCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG ftCatalog;
                END
            ", suppressTransaction: true);
        }
    }
}