// Services/IProductService.cs
using ProductApi.DTOs;

namespace ProductApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
        Task<ProductResponseDto?> GetProductByIdAsync(int id);
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto);
        Task<ProductResponseDto?> UpdateProductAsync(int id, ProductUpdateDto productDto);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string searchTerm);
    }
}
