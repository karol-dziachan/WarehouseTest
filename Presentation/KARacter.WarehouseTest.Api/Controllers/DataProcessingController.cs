using KARacter.WarehouseTest.Application.Features.DataProcessing.Commands;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using KARacter.WarehouseTest.Api.Abstraction;

namespace KARacter.WarehouseTest.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class DataProcessingController : BaseController
{
    /// <summary>
    /// Processes data from CSV files by downloading and importing them into the database.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/dataprocessing/import
    ///     {
    ///         "productsUrl": "https://example.com/products.csv",
    ///         "inventoryUrl": "https://example.com/inventory.csv",
    ///         "pricesUrl": "https://example.com/prices.csv"
    ///     }
    /// 
    /// The endpoint performs the following operations:
    /// 1. Downloads the CSV files from provided URLs
    /// 2. Processes Products.csv:
    ///    - Filters out wire products
    ///    - Saves products with 24h shipping time
    /// 3. Processes Inventory.csv:
    ///    - Saves inventory data for products with 24h shipping
    /// 4. Processes Prices.csv:
    ///    - Saves all price data
    /// </remarks>
    /// <param name="command">URLs for the CSV files to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Data processing completed successfully</response>
    /// <response code="400">Invalid request or validation errors</response>
    /// <response code="422">CSV processing or data import failed</response>
    /// <response code="500">Internal server error</response>
    /// <returns>Result of the data processing operation</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(DataProcessingCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DataProcessingCommandResult>> ImportData(
        [FromBody] DataProcessingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Data Processing Failed",
                Detail = result.Message,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        return Ok(result);
    }
}