using MediatR;
using Microsoft.Extensions.Logging;
using KARacter.WarehouseTest.Application.Common.Interfaces.Repositories;
using KARacter.WarehouseTest.Application.Common.Models;
using KARacter.WarehouseTest.Domain.Entities;

namespace KARacter.WarehouseTest.Application.Features.Products.Queries.GetProductDetails;

public sealed class GetProductDetailsQueryHandler 
    : IRequestHandler<GetProductDetailsQuery, Result<ProductDetails>>
{
    private readonly ILogger<GetProductDetailsQueryHandler> _logger;
    private readonly IProductRepository _productRepository;

    public GetProductDetailsQueryHandler(
        ILogger<GetProductDetailsQueryHandler> logger,
        IProductRepository productRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetails>> Handle(
        GetProductDetailsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting product details for SKU: {SKU}", request.SKU);

            var productDetails = await _productRepository.GetDetailsBySkuAsync(request.SKU, cancellationToken);
            
            if (productDetails == null)
            {
                _logger.LogWarning("Product with SKU {SKU} not found", request.SKU);
                return Result<ProductDetails>.Failed($"Product with SKU {request.SKU} not found");
            }

            _logger.LogInformation("Successfully retrieved details for product with SKU: {SKU}", request.SKU);
            return Result<ProductDetails>.Succeeded(productDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product details for SKU: {SKU}", request.SKU);
            return Result<ProductDetails>.Failed($"Error retrieving product details: {ex.Message}");
        }
    }
} 