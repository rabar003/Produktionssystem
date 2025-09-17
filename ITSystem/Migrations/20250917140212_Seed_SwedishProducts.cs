using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSystem.Migrations
{
    /// <inheritdoc />
    public partial class Seed_SwedishProducts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Byt engelska → svenska (behåller samma Id => ordrar funkar)
            migrationBuilder.Sql("UPDATE Products SET Name='Bärbar dator', Description='Bärbar speldator', Price=15000 WHERE Name='Laptop';");
            migrationBuilder.Sql("UPDATE Products SET Name='Smartphone', Description='Senaste modellen', Price=8000 WHERE Name='Smartphone';");
            migrationBuilder.Sql("UPDATE Products SET Name='Surfplatta', Description='10-tum surfplatta', Price=5000 WHERE Name='Tablet';");
            migrationBuilder.Sql("UPDATE Products SET Name='Hörlurar', Description='Aktiv brusreducering', Price=1500 WHERE Name='Headphones';");

            // 2) Ta bort dubbletter av de fyra ovan – behåll minsta Id och ta inte bort produkter som används i orderrader
            migrationBuilder.Sql(@"
        WITH Keep AS (
          SELECT MIN(Id) AS KeepId, Name
          FROM Products
          WHERE Name IN ('Bärbar dator','Smartphone','Surfplatta','Hörlurar')
          GROUP BY Name
        )
        DELETE P
        FROM Products P
        LEFT JOIN Keep K ON P.Name = K.Name AND P.Id = K.KeepId
        WHERE P.Name IN ('Bärbar dator','Smartphone','Surfplatta','Hörlurar')
          AND K.KeepId IS NULL
          AND P.Id NOT IN (SELECT DISTINCT ProductId FROM OrderItems);
    ");

            // 3) Lägg till resterande sex produkter om de saknas
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Products WHERE Name='Smartklocka')            INSERT INTO Products(Name,Description,Price) VALUES ('Smartklocka','Tränings- och hälsospårning',2500);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Products WHERE Name='Mekaniskt tangentbord')  INSERT INTO Products(Name,Description,Price) VALUES ('Mekaniskt tangentbord','RGB, blå brytare',1299);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Products WHERE Name='Gamingmus')              INSERT INTO Products(Name,Description,Price) VALUES ('Gamingmus','Ergonomisk, 6 knappar',599);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Products WHERE Name='4K-skärm')               INSERT INTO Products(Name,Description,Price) VALUES ('4K-skärm','27-tum 4K IPS',4490);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Products WHERE Name='Extern SSD')             INSERT INTO Products(Name,Description,Price) VALUES ('Extern SSD','1 TB USB-C NVMe',1790);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Products WHERE Name='Webbkamera')             INSERT INTO Products(Name,Description,Price) VALUES ('Webbkamera','1080p med mikrofon',799);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rulla tillbaka insatta 6 (enkelt)
            migrationBuilder.Sql("DELETE FROM Products WHERE Name IN ('Smartklocka','Mekaniskt tangentbord','Gamingmus','4K-skärm','Extern SSD','Webbkamera');");

            // (Valfritt) Byt tillbaka svenska → engelska
            migrationBuilder.Sql("UPDATE Products SET Name='Laptop', Description='Gaming Laptop', Price=15000 WHERE Name='Bärbar dator';");
            migrationBuilder.Sql("UPDATE Products SET Name='Tablet', Description='10 inch Tablet', Price=5000 WHERE Name='Surfplatta';");
            migrationBuilder.Sql("UPDATE Products SET Description='Latest Smartphone' WHERE Name='Smartphone';");
            migrationBuilder.Sql("UPDATE Products SET Name='Headphones', Description='Noise Cancelling', Price=1500 WHERE Name='Hörlurar';");
        }

    }
}
