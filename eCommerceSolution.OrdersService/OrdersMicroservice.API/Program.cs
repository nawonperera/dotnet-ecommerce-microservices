using eCommerce.OrderMicroservice.BusinessLogicLayer;
using eCommerce.OrderMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrderMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;
using Polly;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

//FluentValidations
builder.Services.AddFluentValidationAutoValidation();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddTransient<IUsersMicroservicePolicy, UsersMicroservicePolicy>();
builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();
builder.Services.AddTransient<IPollyPolicies, PollyPolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
})
    //.AddPolicyHandler(
    //    builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy()
    //).AddPolicyHandler(
    //    builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy()
    //).AddPolicyHandler(
    //    builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetTimeoutPolicy()
    //)
    .AddPolicyHandler(
        builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicy>().GetCombinedPolicy()
    );

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
}).AddPolicyHandler(
        builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetFallbackPolicy()
    ).AddPolicyHandler(
        builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetBulkheadIsolationPolicy()
    );

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandleMiddleware();
}

app.UseRouting();

//Cors
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Auth
// app.UseHttpsRedirection(); // Commented out to prevent ERR_SSL_PROTOCOL_ERROR in Docker HTTP
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();

app.Run();
