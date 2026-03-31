using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.Sql(@"
            //    IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'ProductCatalog') 
            //    CREATE FULLTEXT CATALOG ProductCatalog;
            //", suppressTransaction: true);

            //migrationBuilder.Sql(@"
            //    IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Products'))
            //    CREATE FULLTEXT INDEX ON Products(Name LANGUAGE 1033, Description LANGUAGE 1033)
            //    KEY INDEX PK_Products
            //    ON ProductCatalog
            //    WITH CHANGE_TRACKING AUTO;
            //", suppressTransaction: true);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.Sql("DROP FULLTEXT INDEX ON Products;", suppressTransaction: true);
            //migrationBuilder.Sql("DROP FULLTEXT CATALOG ProductCatalog;", suppressTransaction: true);
        }
    }
}
