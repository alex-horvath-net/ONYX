using System.Text;
using Common.Infrastucture;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProductServices.CreateProduct;
using ProductServices.ReadProducts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddCommonInfrastucture(builder.Configuration)
    .AddCreateProductService()
    .AddReadProductsService();

//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options => {
//        options.Authority = "https://your-auth-server"; // Mock authority for example
//        options.TokenValidationParameters = new TokenValidationParameters {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecureKey")),
//            ValidateIssuer = false,
//            ValidateAudience = false
//        };
//    });


var azureAdSettings = builder.Configuration.GetSection("AzureAd");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = $"{azureAdSettings["Instance"]}{azureAdSettings["TenantId"]}";  // Azure AD authority
        options.Audience = azureAdSettings["Audience"];  // Your API's audience (the registered API URI)

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,  // Validate that the token is from a trusted issuer (Azure AD)
            ValidateAudience = true,  // Ensure that the token is valid for this API
            ValidateLifetime = true,  // Ensure that the token is not expired
            ValidateIssuerSigningKey = true,  // Ensure that the token is signed by a trusted party
            // The signing key is managed by Azure AD, so you don't need to configure it manually
        };

        // Ensure that the token is sent over HTTPS
        options.RequireHttpsMetadata = true;
    });
builder.Services.AddAuthorization();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
