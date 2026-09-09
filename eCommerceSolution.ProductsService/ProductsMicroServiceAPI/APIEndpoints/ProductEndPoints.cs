using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.DTO;
using FluentValidation;
using FluentValidation.Results;

namespace eCommerce.ProductsMicroServiceAPI.APIEndpoints;

public static class ProductEndPoints
{
    public static async Task<IEndpointRouteBuilder> MapProductAPIEndPoints(this IEndpointRouteBuilder app)
    {
        #region GET /api/products
        app.MapGet("api/products", async (IProductsService productsService) =>
        {
            List<ProductResponse?> products = await productsService.GetProducts();
            //MODIFIED
            return Results.Ok(products);
        });
        #endregion

        #region GET /api/products/search/product-id/00000000-0000-0000-0000-000000000000

        app.MapGet("/api/products/search/products-id/{ProductID:guid}", async (IProductsService productsService, Guid
             ProductID) =>
        {
            ProductResponse? product = await productsService.GetProductByCondition(temp => temp.ProductId == ProductID);
            if (product == null) return Results.NotFound();
            return Results.Ok(product);
        });
        #endregion

        #region GET /api/products/search/xxxxxxxxxxxxxxxxxx
        app.MapGet("/api/products/seearch/{searchString}",async(IProductsService productsService,string searchString) =>
        {
            List<ProductResponse?> productsByProductName = await productsService.GetProductsByCondition(temp=>temp.ProductName!=null && temp.ProductName.Contains(searchString,StringComparison.OrdinalIgnoreCase));

            List<ProductResponse?> productsByCategory = await productsService.GetProductsByCondition(temp=>temp.Category!=null && temp.Category.Contains(searchString,StringComparison.OrdinalIgnoreCase));

            var products = productsByProductName.Union(productsByCategory);

            return Results.Ok(products);
        });
        #endregion

        #region POST /api/products
        app.MapPost("/api/products", async (IProductsService productsService, IValidator<ProductAddRequest> productAddRequestValidator, ProductAddRequest productAddRequest) =>
        {
            //Validate the ProductAddRequest object using Fluent Validation
            ValidationResult validationResult = await productAddRequestValidator.ValidateAsync(productAddRequest);

            //Check the validation result
            if (!validationResult.IsValid)
            {
                // Create a dictionary where:
                // Key   = property name (e.g., "ProductName")
                // Value = array of error messages for that property
                Dictionary<string, string[]> errors =
                    validationResult.Errors // collection of validation failures
                        .GroupBy(temp => temp.PropertyName) // Group errors by the property name
                        // Convert grouped data into a dictionary
                        .ToDictionary(  
                            grp => grp.Key, // Dictionary key → property name
                            // Dictionary value → list of error messages for that property
                            grp => grp
                                    .Select(err => err.ErrorMessage) // extract message
                                    .ToArray() // convert to string[]
                        );

                // Return HTTP 400 response with structured validation errors
                // This automatically formats response like:
                // {
                //   "errors": {
                //      "ProductName": ["Required"],
                //      "Price": ["Must be > 0"]
                //   }
                // }
                return Results.ValidationProblem(errors);
            }

            var addedProductResponse = await productsService.AddProduct(productAddRequest);
            if (addedProductResponse != null)
            {
                return Results.Created($"/api/products/search/product-id/{addedProductResponse.ProductID}", addedProductResponse);
            }
            else
            {
                return Results.Problem("Error in adding product");
            }

        });
        #endregion

        #region PUT /api/products
        app.MapPut("/api/products", async (IProductsService productsService, IValidator<ProductUpdateRequest> productUpdateRequestValidator, ProductUpdateRequest productUpdateRequest) =>
        {
            //Validate the ProductUpdateRequest object using Fluent Validation
            ValidationResult validationResult = await productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

            //Check the validation result
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors
                  .GroupBy(temp => temp.PropertyName)
                  .ToDictionary(grp => grp.Key,
                    grp => grp.Select(err => err.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }


            var updatedProductResponse = await productsService.UpdateProduct(productUpdateRequest);
            if (updatedProductResponse != null)
                return Results.Ok(updatedProductResponse);
            else
                return Results.Problem("Error in updating product");
        });
        #endregion

        #region DELETE /api/products/00000000-0000-0000-0000-000000000000
        app.MapDelete("/api/products/{ProductID:guid}", async (IProductsService productsService, Guid ProductID) =>
        {
            bool isDeleted = await productsService.DeleteProduct(ProductID);
            if (isDeleted)
                return Results.Ok(true);
            else
                return Results.Problem("Error in deleting product");
        });

        #endregion

        return app;
    }
}
