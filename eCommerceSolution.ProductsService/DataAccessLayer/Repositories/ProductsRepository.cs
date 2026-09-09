using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.Context;
using eCommerce.DataAccessLayer.RepositoryContracts;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace eCommmerce.DataAccessLayer.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Product?> AddProduct(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProduct(Guid productID)
    {
        Product? existingProduct = await _dbContext.Products.FirstOrDefaultAsync(product => product.ProductId == productID);
        if (existingProduct == null)
        {
            return false;
        }
        _dbContext.Products.Remove(existingProduct);
        int numberOfRowsAffected = await _dbContext.SaveChangesAsync();
        return numberOfRowsAffected > 0;
    }

    public async Task<Product?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(conditionExpression);
    }

    public async Task<IEnumerable<Product>> GetProducts()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<IEnumerable<Product?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        return await _dbContext.Products.Where(conditionExpression).ToListAsync();
    }

    public async Task<Product?> UpdateProduct(Product product)
    {
        Product? existingProduct = await _dbContext.Products.FirstOrDefaultAsync(product => product.ProductId == product.ProductId);
        if (existingProduct == null)
        {
            return null;
        }

        existingProduct.ProductName = product.ProductName;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.QuantityInStock = product.QuantityInStock;
        existingProduct.Category = product.Category;

        await _dbContext.SaveChangesAsync();
        return existingProduct;
    }
}
