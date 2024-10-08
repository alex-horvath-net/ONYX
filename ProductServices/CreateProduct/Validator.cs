using FluentValidation;

namespace ProductServices.CreateProduct;
public class Validator(IValidator<CreateProductRequest> fluentValidator) : IValidator {
    public async Task<List<string>> Validate(CreateProductRequest request, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        var result = await fluentValidator.ValidateAsync(request, token);
        return result.Errors.Select(x => x.ErrorMessage).ToList();
    }
}

public class FluentValidator : AbstractValidator<CreateProductRequest> {
    public FluentValidator(IRepository repository) {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("Request must be provided.");

        RuleFor(x => x.Name)
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters long.");

        RuleFor(x => x.Name)
            .MustAsync(repository.NameIsUnique)
            .WithMessage("Name must be unique.");
    }
}
