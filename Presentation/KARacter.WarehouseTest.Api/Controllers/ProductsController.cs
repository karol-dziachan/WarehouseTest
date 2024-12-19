using System.Net.Mime;
using KARacter.WarehouseTest.Api.Abstraction;
using KARacter.WarehouseTest.Application.Common.Models;
using KARacter.WarehouseTest.Application.Features.Products.Queries.GetProductDetails;
using KARacter.WarehouseTest.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace KARacter.WarehouseTest.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ProductsController : BaseController
{
    /// <summary>
    /// Gets detailed information about a product by its SKU
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///     GET /api/dataprocessing/products/0001-00017-64898
    /// 
    /// Returns complete product information including:
    /// - Basic product details (name, EAN, producer, category)
    /// - Inventory data (quantity, shipping time and cost)
    /// - Price information (net price and logistic unit price)
    /// - Product image URL
    /// </remarks>
    /// <param name="sku">Product SKU</param>
    /// <response code="200">Product details found and returned</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error</response>
    /// <returns>Detailed product information</returns>
    [HttpGet("products/{sku}")]
    [ProducesResponseType(typeof(Result<ProductDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<ProductDetails>>> GetProductDetails(
        [FromRoute] string sku,
        CancellationToken cancellationToken)
    {
        var query = new GetProductDetailsQuery(sku);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.Success)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Product Not Found",
                Detail = result.Message,
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }
}