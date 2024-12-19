using FluentValidation;

namespace KARacter.WarehouseTest.Application.Features.DataProcessing.Commands;

public sealed class DataProcessingCommandValidator : AbstractValidator<DataProcessingCommand>
{
    public DataProcessingCommandValidator()
    {
        RuleFor(x => x.ProductsUrl)
            .NotEmpty().WithMessage("Products URL is required")
            .Must(BeValidUrl).WithMessage("Products URL must be a valid URL");

        RuleFor(x => x.InventoryUrl)
            .NotEmpty().WithMessage("Inventory URL is required")
            .Must(BeValidUrl).WithMessage("Inventory URL must be a valid URL");

        RuleFor(x => x.PricesUrl)
            .NotEmpty().WithMessage("Prices URL is required")
            .Must(BeValidUrl).WithMessage("Prices URL must be a valid URL");
    }

    private static bool BeValidUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out _);
} 