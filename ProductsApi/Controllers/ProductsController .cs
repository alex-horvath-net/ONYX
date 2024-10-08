using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductServices.CreateProduct;
using ProductServices.ReadProducts;

namespace ProductsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ICreateProductService createPproduct, IReadProductsService readProducts) : ControllerBase {

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck() {
        return Ok("Service is up and running.");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken token) {
        var response = await createPproduct.Execute(request, token);
        return response.Issues.Any() ? BadRequest(response.Issues) : Ok(response.Product);
    }

    [HttpGet("colour/{Colour}")]
    [HttpGet()]
    [Authorize]
    public async Task<IActionResult> GetAllProducts([FromRoute] ReadProductsRequest request, CancellationToken token) {
        var response = await readProducts.Execute(request, token);
        return response.Issues.Any() ? BadRequest(response.Issues) : Ok(response.Products);
    }
}
