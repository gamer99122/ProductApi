using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToResponseDto);
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return product != null ? MapToResponseDto(product) : null;
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                IsActive = productDto.IsActive,
                CreatedDate = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToResponseDto(product);
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(int id, ProductUpdateDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
                return null;

            if (!string.IsNullOrEmpty(productDto.Name))
                product.Name = productDto.Name;

            if (productDto.Description != null)
                product.Description = productDto.Description;

            if (productDto.Price.HasValue)
                product.Price = productDto.Price.Value;

            if (productDto.Stock.HasValue)
                product.Stock = productDto.Stock.Value;

            if (productDto.IsActive.HasValue)
                product.IsActive = productDto.IsActive.Value;

            await _context.SaveChangesAsync();
            return MapToResponseDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            // 軟刪除：設定為不活躍而不是真正刪除
            product.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _context.Products
                .Where(p => p.IsActive &&
                           (p.Name.Contains(searchTerm) ||
                            (p.Description != null && p.Description.Contains(searchTerm))))
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToResponseDto);
        }

        private static ProductResponseDto MapToResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CreatedDate = product.CreatedDate,
                IsActive = product.IsActive
            };
        }
    }
}
