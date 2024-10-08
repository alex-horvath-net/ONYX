using FluentValidation;

namespace ProductServices.ReadProducts;
public class Validator(IValidator<ReadProductsRequest> fluentValidator) : IValidator {
    public async Task<List<string>> Validate(ReadProductsRequest request, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        var result = await fluentValidator.ValidateAsync(request);
        return result.Errors.Select(x => x.ErrorMessage).ToList();
    }
}

public class FluentValidator : AbstractValidator<ReadProductsRequest> {
    public FluentValidator() {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("Request must be provided.");

        RuleFor(x => x.Colour)
            .MinimumLength(3)
            .When(x => !string.IsNullOrEmpty(x.Colour))
            .WithMessage("Colour must be at least 3 character long, if it is provided.");
    }
}
