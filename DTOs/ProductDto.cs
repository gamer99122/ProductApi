using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "商品名稱為必填")]
        [StringLength(100, ErrorMessage = "商品名稱不能超過100個字元")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "商品描述不能超過500個字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "價格為必填")]
        [Range(0, double.MaxValue, ErrorMessage = "價格必須大於0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "庫存為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "庫存不能為負數")]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ProductUpdateDto
    {
        [StringLength(100, ErrorMessage = "商品名稱不能超過100個字元")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "商品描述不能超過500個字元")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "價格必須大於0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "庫存不能為負數")]
        public int? Stock { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
