-- Tworzenie tabeli Products
CREATE TABLE Products (
    SKU NVARCHAR(75) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    EAN NVARCHAR(13),
    ProducerName NVARCHAR(255),
    Category NVARCHAR(255),
    DefaultImage NVARCHAR(MAX),
    IsWire BIT NOT NULL DEFAULT 0
);

-- Tworzenie tabeli Inventory
CREATE TABLE Inventory (
    SKU NVARCHAR(75) PRIMARY KEY,
    Unit NVARCHAR(50),
    Quantity DECIMAL(18,2),
    ShippingTime NVARCHAR(50),
    ShippingCost DECIMAL(18,2),
    CONSTRAINT FK_Inventory_Products FOREIGN KEY (SKU) REFERENCES Products(SKU)
);

-- Tworzenie tabeli Prices
CREATE TABLE Prices (
    SKU NVARCHAR(75) PRIMARY KEY,
    NetPrice DECIMAL(18,2),
    LogisticUnitNetPrice DECIMAL(18,2),
    CONSTRAINT FK_Prices_Products FOREIGN KEY (SKU) REFERENCES Products(SKU)
);

-- Indeksy dla poprawy wydajności
CREATE INDEX IX_Products_SKU ON Products(SKU);
CREATE INDEX IX_Inventory_SKU ON Inventory(SKU);
CREATE INDEX IX_Prices_SKU ON Prices(SKU);

-- Indeksy nieklastrowe dla usprawnienia wyszukiwania
CREATE NONCLUSTERED INDEX IX_Products_IsWire_ShippingTime 
ON Products(IsWire)
INCLUDE (Name, EAN, ProducerName, Category, DefaultImage);

CREATE NONCLUSTERED INDEX IX_Inventory_ShippingTime 
ON Inventory(ShippingTime)
INCLUDE (Unit, Quantity, ShippingCost);
