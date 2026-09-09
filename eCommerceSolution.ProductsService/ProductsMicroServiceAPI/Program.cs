using eCommerce.BussinessLogicLayer;
using eCommerce.DataAccessLayer;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroServiceAPI.Middleware;
using eCommerce.ProductsMicroServiceAPI.APIEndpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBussinessLogicLayer();

builder.Services.AddControllers();

//FluentValidation
builder.Services.AddFluentValidationAutoValidation();

//Add model binder to read values from JSON to enum
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//Add Sawagger Service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder.WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader());
});

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
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();
await app.MapProductAPIEndPoints();

app.Run();
