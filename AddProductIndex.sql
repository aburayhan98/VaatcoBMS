

CREATE INDEX IX_Products_Name_IsActive
    ON dbo.Products (IsActive, Name)
    INCLUDE (Code, PackSize, Price, StockQuantity, StockStatus);

CREATE INDEX IX_Products_Code
    ON dbo.Products (Code)
    INCLUDE (Name, PackSize, Price, StockQuantity, StockStatus, IsActive);

    /*SELECT name, type_desc FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Products');
EXEC sp_helpindex 'dbo.Products';*/