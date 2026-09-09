using AutoMapper;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;
using eCommerce.BussinessLogicLayer.RabbitMQ;


namespace eCommerce.BusinessLogicLayer.Services;

public class ProductsService : IProductsService
{
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productsRepository;
    private readonly IRabbitMQPublisher _rabbitMQPublisher;

    public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator, IValidator<ProductUpdateRequest> productUpdateRequestValidator, IMapper mapper, IProductsRepository productsRepository, IRabbitMQPublisher rabbitMQPublisher)
    {
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _mapper = mapper;
        _productsRepository = productsRepository;
        _rabbitMQPublisher = rabbitMQPublisher;
    }
    public async Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest)
    {
        if(productAddRequest == null)
        {
            throw new ArgumentNullException(nameof(productAddRequest));
        }

        //Validate the product using Fluent Validation
        ValidationResult validationResult = await _productAddRequestValidator.ValidateAsync(productAddRequest);

        // Check the validation result
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp=>temp.ErrorMessage));//Error1, Error2, ...
            throw new ArgumentException(errors);
        }

        //Attempt to add product
        Product productInput = _mapper.Map<Product>(productAddRequest); //Map productAddRequest into 'Product' type (it invokes ProductAddRequestToProductMappingProfile)
        Product? addedProduct = await _productsRepository.AddProduct(productInput);

        if(addedProduct == null)
        {
            return null;
        }

        ProductResponse addedProductResponse = _mapper.Map<ProductResponse>(addedProduct);//Map addedProduct into 'ProductRepsonse' type (it invokes ProductToProductResponseMappingProfile)

        return addedProductResponse;

    }

    public async Task<bool> DeleteProduct(Guid productID)
    {
        Product? existingProduct = await _productsRepository.GetProductByCondition(temp => temp.ProductId == productID);

        if (existingProduct == null)
        {
            return false;
        }

        //Attempt to delete product
        bool isDeleted = await _productsRepository.DeleteProduct(productID);

        //TO DO: Add code for posting a message to the message queue that announces the consumers about the deleted product details
        if (isDeleted)
        {
            //Publish message od product.delete
            ProductDeletionMessage message = new ProductDeletionMessage(existingProduct.ProductId, existingProduct.ProductName);

            //string routingKey = "product.delete";
            var headers = new Dictionary<string, object>()
            {
                {"event", "product.delete"},
                {"RowCount", 1}
            };


            _rabbitMQPublisher.Publish(headers, message);
        }


        return isDeleted;
    }

    public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        Product? product = await _productsRepository.GetProductByCondition(conditionExpression);

        ProductResponse? productsResponse = _mapper.Map<ProductResponse?>(product); //Invokes ProductToProductResponseMappingProfile

        return productsResponse;
    }

    public async Task<List<ProductResponse?>> GetProducts()
    {
        IEnumerable<Product?> products = await _productsRepository.GetProducts();

        IEnumerable<ProductResponse?> productsResponses = _mapper.Map<IEnumerable<ProductResponse?>>(products); //Invokes ProductToProductResponseMappingProfile

        return productsResponses.ToList();
    }

    public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        IEnumerable<Product?> products = await _productsRepository.GetProductsByCondition(conditionExpression);

        IEnumerable<ProductResponse?> productsResponses = _mapper.Map< IEnumerable<ProductResponse?>>(products); //Invokes ProductToProductResponseMappingProfile

        return productsResponses.ToList();
    }

    public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
    {
        Product? existingProduct = await _productsRepository.GetProductByCondition(temp => temp.ProductId == productUpdateRequest.ProductID);

        if (existingProduct == null)
        {
            throw new ArgumentException("Invalid Product ID");
        }

        //Validate the product using Fluent Validation
        ValidationResult validationResult = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

        // Check the validation result
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage)); //Error1, Error2, ...
            throw new ArgumentException(errors);
        }

        //Map from ProductUpdateRequest to Product type
        Product product = _mapper.Map<Product>(productUpdateRequest); //Invokes ProductUpdateRequestToProductMappingProfile

        //Check if the product name is changed
        //bool isProductNameChanged = productUpdateRequest.ProductName != existingProduct.ProductName;

        Product? updateProduct = await _productsRepository.UpdateProduct(product);

        
            //string routingKey = "product.update.name";

            var headers = new Dictionary<string, object>()
            {
                {"event", "product.update"},
                {"RowCount", 1}
            };

            //var message = new ProductNameUpdateMessage(product.ProductId,product.ProductName);
            _rabbitMQPublisher.Publish<Product>(headers, product);
        

        ProductResponse? updatedProductResponse = _mapper.Map<ProductResponse>(updateProduct);

        return updatedProductResponse;
    }
}
