using Microsoft.Data.SqlClient;

namespace KARacter.WarehouseTest.Application.Common.Interfaces.Factories;

public interface ISqlConnectionFactory
{
    /// <summary>
    /// W .NET 7 lepszym wyborem jest Microsoft.Data.SqlClient zamiast starszego System.Data.SqlClient. Oto główne powody:
    /// Microsoft.Data.SqlClient to aktywnie rozwijana biblioteka, która:
    /// Otrzymuje regularne aktualizacje bezpieczeństwa
    /// Wspiera najnowsze funkcje SQL Server
    /// Jest zalecana przez Microsoft dla nowych projektów
    /// </summary>
    SqlConnection CreateConnection();
}