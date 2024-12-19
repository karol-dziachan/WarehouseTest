using MediatR;
using KARacter.WarehouseTest.Application.Common.Models;
using KARacter.WarehouseTest.Domain.Entities;

namespace KARacter.WarehouseTest.Application.Features.Products.Queries.GetProductDetails;

public sealed record GetProductDetailsQuery(string SKU) : IRequest<Result<ProductDetails>>; 