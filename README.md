# KARacter.WarehouseTest

## Opis rozwiązania
Tutaj ten zapis: x.csv Available columns rozumiem jako kolumny, które są dostępne w pliku csv - czyli np. pomimo tego, ze w pliku x.csv jest kolumna Shipping to z niej nie korzystam, bo nie ma jej wymienionej w specyfikacji API.

np. Dostępne kolumny:
- ID - unikalny identyfikator produktu
- SKU - unikalny identyfikator produktu nadany przez magazyn 
- name - nazwa produktu
- EAN - numer produktu
- producer_name - nazwa dostawcy
- category - kategoria produktu
- is_wire - określa czy produkt jest przewodem (wartość 1 oznacza tak)
- available - określa czy produkt jest dostępny do zamówienia (wartość 1 oznacza tak)
- is_vendor - określa czy produkt jest wysyłany przez dostawcę czy magazyn. Wartość 0 oznacza wysyłkę z magazynu, 1 oznacza wysyłkę przez dostawcę
- default_image - adres URL do zdjęcia produktu

### Minimalizacja zasobów
Postawiłem na minimalizację zasobów, więc z góry utworzyłem tabele, które zawierają tylko te kolumny, które są wymagane do wykonania zadania.

## Database

Pełny SQL dla bazy danych znajduje się w pliku `DDL.sql` w folderze `Database/KARacter.WarehouseTest.Database`.

Zapisanie pełnych danych do bazy danych można wykonać za pomocą poniższego skryptu:
```sql
-- NIE UŻYWAĆ
-- Tworzenie tabeli Products
CREATE TABLE Products (
    ID INT PRIMARY KEY,
    SKU NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    EAN NVARCHAR(13),
    ProducerName NVARCHAR(255),
    Category NVARCHAR(255),
    IsWire BIT NOT NULL DEFAULT 0,
    Available BIT NOT NULL DEFAULT 0,
    IsVendor BIT NOT NULL DEFAULT 0,
    DefaultImage NVARCHAR(MAX)
);

-- Tworzenie tabeli Inventory
CREATE TABLE Inventory (
    ProductId INT NOT NULL,
    SKU NVARCHAR(50) NOT NULL,
    Unit NVARCHAR(50),
    Quantity DECIMAL(18,2),
    Manufacturer NVARCHAR(255),
    ShippingTime NVARCHAR(50),
    ShippingCost DECIMAL(18,2),
    CONSTRAINT FK_Inventory_Products FOREIGN KEY (ProductId) REFERENCES Products(ID),
    CONSTRAINT FK_Inventory_SKU FOREIGN KEY (SKU) REFERENCES Products(SKU)
);

-- Tworzenie tabeli Prices
CREATE TABLE Prices (
    InternalId INT PRIMARY KEY IDENTITY(1,1),
    SKU NVARCHAR(50) NOT NULL,
    NetPrice DECIMAL(18,2),
    DiscountedNetPrice DECIMAL(18,2),
    VATRate DECIMAL(5,2),
    LogisticUnitNetPrice DECIMAL(18,2),
    CONSTRAINT FK_Prices_Products FOREIGN KEY (SKU) REFERENCES Products(SKU)
);
```

Dla przeczytania danych potrzebnych do wykonania zadania można uprościć go do

```sql
-- Tworzenie tabeli Products
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
```

## Architektura i Technologie

### Clean Architecture
Projekt został zbudowany w oparciu o zasady Clean Architecture, z podziałem na warstwy:
- **Core**
  - **Domain**: Encje, modele domenowe, interfejsy repozytoriów
  - **Application**: Logika biznesowa, CQRS (Commands/Queries), walidatory
- **Infrastructure**: Implementacje repozytoriów, serwisy, dostęp do bazy danych
- **Presentation**: API, kontrolery, middleware

### Wzorce i Praktyki
- **CQRS**: Separacja operacji odczytu i zapisu
- **Mediator Pattern**: Obsługa komunikacji między komponentami
- **Repository Pattern**: Abstrakcja dostępu do danych
- **DI (Dependency Injection)**: Luźne powiązania między komponentami

### Technologie
- **.NET 7**
- **ASP.NET Core Web API**
- **Dapper**: ORM do dostępu do bazy danych
- **MediatR**: Implementacja CQRS i Mediator Pattern
- **FluentValidation**: Walidacja modeli
- **CsvHelper**: Parsowanie plików CSV
- **SQL Server**: Baza danych

### Endpointy API

#### 1. Import Danych
```http
POST /api/v1/dataprocessing/import
Content-Type: application/json

{
    "productsUrl": "https://example.com/products.csv",
    "inventoryUrl": "https://example.com/inventory.csv",
    "pricesUrl": "https://example.com/prices.csv"
}
```

Endpoint pobiera i przetwarza dane z plików CSV:
- Filtruje produkty niebędące kablami
- Zapisuje produkty z czasem dostawy 24h
- Zapisuje stany magazynowe
- Zapisuje ceny produktów

#### 2. Szczegóły Produktu
```http
GET /api/v1/products/{sku}
```

Zwraca pełne informacje o produkcie:
```json
{
    "success": true,
    "message": "Operation completed successfully",
    "data": {
        "sku": "0001-00017-64898",
        "name": "Product Name",
        "ean": "1234567890123",
        "producerName": "Producer",
        "category": "Category",
        "quantity": 100,
        "shippingTime": "24h",
        "shippingCost": 15.99,
        "netPrice": 199.99,
        "logisticUnitNetPrice": 189.99,
        "defaultImage": "https://example.com/image.jpg"
    }
}
```

### Obsługa Błędów
- Globalna obsługa wyjątków przez middleware
- Szczegółowe komunikaty błędów
- Logowanie błędów
- Standardowe kody HTTP odpowiedzi

### Wydajność
- Zoptymalizowana struktura bazy danych
- Indeksy dla często używanych kolumn
- Asynchroniczne operacje
- Minimalizacja liczby zapytań do bazy

### Bezpieczeństwo
- Walidacja danych wejściowych
- Sanityzacja ścieżek plików
- Bezpieczne parsowanie CSV
- Obsługa limitów rozmiaru plików

### Testy
Projekt zawiera:
- Testy jednostkowe (xUnit)
- Testy integracyjne
- Testy end-to-end
- Mockowanie zależności

### Uruchomienie Projektu
1. Wymagania:
   - .NET 7 SDK
   - SQL Server
   - Visual Studio 2022 lub VS Code

2. Konfiguracja:
   ```bash
   # Klonowanie repozytorium
   git clone git@github.com:karol-dziachan/WarehouseTest.git
   
   # Przejście do katalogu projektu
   cd ./Presentation/KARacter.WarehouseTest.Api
   
   # Restore pakietów
   dotnet restore
   
   # Uruchomienie aplikacji
   dotnet run --project Presentation/KARacter.WarehouseTest.Api
   ```

3. Konfiguracja w appsettings.json:
   ```json
   {
     "ConnectionStrings": {
       "KARacter.WarehouseTestDb": "set your connection string here"
     }
   }
   ```

### Monitorowanie i Logowanie
- Strukturalne logowanie z Serilog
- Śledzenie wydajności
- Metryki aplikacji
- Logowanie zdarzeń biznesowych