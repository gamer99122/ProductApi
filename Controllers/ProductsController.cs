using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有商品
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得商品列表時發生錯誤");
                return StatusCode(500, "伺服器內部錯誤");
            }
        }

        /// <summary>
        /// 根據ID取得商品
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"找不到ID為 {id} 的商品");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得商品 {ProductId} 時發生錯誤", id);
                return StatusCode(500, "伺服器內部錯誤");
            }
        }

        /// <summary>
        /// 建立新商品
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct(ProductCreateDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立商品時發生錯誤");
                return StatusCode(500, "伺服器內部錯誤");
            }
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, ProductUpdateDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.UpdateProductAsync(id, productDto);
                if (product == null)
                {
                    return NotFound($"找不到ID為 {id} 的商品");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新商品 {ProductId} 時發生錯誤", id);
                return StatusCode(500, "伺服器內部錯誤");
            }
        }

        /// <summary>
        /// 刪除商品
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound($"找不到ID為 {id} 的商品");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除商品 {ProductId} 時發生錯誤", id);
                return StatusCode(500, "伺服器內部錯誤");
            }
        }

        /// <summary>
        /// 搜尋商品
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("搜尋關鍵字不能為空");
                }

                var products = await _productService.SearchProductsAsync(searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋商品時發生錯誤，關鍵字: {SearchTerm}", searchTerm);
                return StatusCode(500, "伺服器內部錯誤");
            }
        }
    }
}
