using AutoFixture;
using AutoMapper;
using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrderMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrderMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrderMicroservice.DataAccessLayer.Entities;
using eCommerce.OrderMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using MongoDB.Driver;
using Xunit;

namespace eCommerce.OrderMicroservice.OrdersUnitTests;

public class OrdersServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrdersRepository> _ordersRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<OrderAddRequest>> _orderAddRequestValidatorMock;
    private readonly Mock<IValidator<OrderItemAddRequest>> _orderItemAddRequestValidatorMock;
    private readonly Mock<IValidator<OrderUpdateRequest>> _orderUpdateRequestValidatorMock;
    private readonly Mock<IValidator<OrderItemUpdateRequest>> _orderItemUpdateRequestValidatorMock;
    
    // Mocks for HttpClients
    private readonly Mock<HttpMessageHandler> _usersHttpMessageHandlerMock;
    private readonly Mock<HttpMessageHandler> _productsHttpMessageHandlerMock;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    
    private readonly UsersMicroserviceClient _usersClient;
    private readonly ProductsMicroserviceClient _productsClient;
    private readonly OrdersService _ordersService;

    public OrdersServiceTests()
    {
        _fixture = new Fixture();
        _ordersRepositoryMock = new Mock<IOrdersRepository>();
        _mapperMock = new Mock<IMapper>();
        _orderAddRequestValidatorMock = new Mock<IValidator<OrderAddRequest>>();
        _orderItemAddRequestValidatorMock = new Mock<IValidator<OrderItemAddRequest>>();
        _orderUpdateRequestValidatorMock = new Mock<IValidator<OrderUpdateRequest>>();
        _orderItemUpdateRequestValidatorMock = new Mock<IValidator<OrderItemUpdateRequest>>();
        
        _usersHttpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _productsHttpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _distributedCacheMock = new Mock<IDistributedCache>();

        // Set up distributed cache to return null (cache miss)
        _distributedCacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _distributedCacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var usersHttpClient = new HttpClient(_usersHttpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _usersClient = new UsersMicroserviceClient(
            usersHttpClient, 
            new Mock<ILogger<UsersMicroserviceClient>>().Object, 
            _distributedCacheMock.Object);

        var productsHttpClient = new HttpClient(_productsHttpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _productsClient = new ProductsMicroserviceClient(
            productsHttpClient, 
            new Mock<ILogger<ProductsMicroserviceClient>>().Object, 
            _distributedCacheMock.Object);

        _ordersService = new OrdersService(
            _ordersRepositoryMock.Object,
            _mapperMock.Object,
            _orderAddRequestValidatorMock.Object,
            _orderItemAddRequestValidatorMock.Object,
            _orderUpdateRequestValidatorMock.Object,
            _orderItemUpdateRequestValidatorMock.Object,
            _usersClient,
            _productsClient);
    }

    [Fact]
    public async Task AddOrder_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        OrderAddRequest? request = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _ordersService.AddOrder(request!));
    }

    [Fact]
    public async Task AddOrder_InvalidRequest_ThrowsArgumentException()
    {
        // Arrange
        var request = _fixture.Create<OrderAddRequest>();
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Property", "Error") });
        _orderAddRequestValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _ordersService.AddOrder(request));
        Assert.Contains("Error", ex.Message);
    }

    [Fact]
    public async Task AddOrder_ValidRequest_ReturnsOrderResponse()
    {
        // Arrange
        var orderItemAddRequest = _fixture.Create<OrderItemAddRequest>();
        var request = _fixture.Build<OrderAddRequest>()
            .With(r => r.OrderItems, new List<OrderItemAddRequest> { orderItemAddRequest })
            .Create();
            
        var order = _fixture.Create<Order>();
        var orderResponse = _fixture.Create<OrderResponse>();
        
        var productDto = new ProductDTO(orderItemAddRequest.ProductID, "Product 1", "Category 1", 10, 100);
        var userDto = new UserDTO(request.UserID, "John Doe", "john@example.com", "Male");

        // Validators
        _orderAddRequestValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _orderItemAddRequestValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<OrderItemAddRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // HttpClients Mocks
        _productsHttpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/gateway/products/search/product-id/{orderItemAddRequest.ProductID}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(productDto))
            });

        _usersHttpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/gateway/users/{request.UserID}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(userDto))
            });

        // Mapper
        _mapperMock.Setup(m => m.Map<Order>(request)).Returns(order);
        _mapperMock.Setup(m => m.Map<OrderResponse>(order)).Returns(orderResponse);

        // Repository
        _ordersRepositoryMock.Setup(r => r.AddOrder(It.IsAny<Order>())).ReturnsAsync(order);

        // Act
        var result = await _ordersService.AddOrder(request);

        // Assert
        Assert.NotNull(result);
        _ordersRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Order>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteOrder_ExistingOrder_ReturnsTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = _fixture.Create<Order>();
        
        _ordersRepositoryMock.Setup(r => r.GetOrderByCondition(It.IsAny<FilterDefinition<Order>>()))
            .ReturnsAsync(order);
        _ordersRepositoryMock.Setup(r => r.DeleteOrder(orderId))
            .ReturnsAsync(true);

        // Act
        var result = await _ordersService.DeleteOrder(orderId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteOrder_NonExistingOrder_ReturnsFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        _ordersRepositoryMock.Setup(r => r.GetOrderByCondition(It.IsAny<FilterDefinition<Order>>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _ordersService.DeleteOrder(orderId);

        // Assert
        Assert.False(result);
    }
}
