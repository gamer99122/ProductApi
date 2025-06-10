# ASP.NET Core Web API 連接 MSSQL 完整範例

## 1. 建立專案並安裝套件

首先建立新的 Web API 專案：
```bash
dotnet new webapi -n ProductApi
cd ProductApi
```

安裝必要的 NuGet 套件：
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

## 2. 建立資料模型 (Models/Product.cs)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public int Stock { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
    }
}
```

## 3. 建立資料庫上下文 (Data/ApplicationDbContext.cs)

```csharp
using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<Product> Products { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 設定資料表名稱
            modelBuilder.Entity<Product>().ToTable("Products");
            
            // 設定索引
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();
                
            // 種子資料
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "筆記型電腦", Description = "高效能筆記型電腦", Price = 25000, Stock = 10 },
                new Product { Id = 2, Name = "滑鼠", Description = "無線光學滑鼠", Price = 500, Stock = 50 },
                new Product { Id = 3, Name = "鍵盤", Description = "機械式鍵盤", Price = 1200, Stock = 30 }
            );
        }
    }
}
```

## 4. 建立 DTO 類別 (DTOs/ProductDto.cs)

```csharp
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
```

## 5. 建立服務介面和實作 (Services/IProductService.cs 和 ProductService.cs)

```csharp
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

// Services/ProductService.cs
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
```

## 6. 建立控制器 (Controllers/ProductsController.cs)

```csharp
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
```

## 7. 設定 Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductApi.Data;
using ProductApi.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 註冊服務
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 註冊 Swagger 服務
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Product API", 
        Version = "v1",
        Description = "商品管理 API"
    });
    
    // 如果有 XML 註解檔案，可以加入以下設定
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

// 註冊資料庫上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊自訂服務
builder.Services.AddScoped<IProductService, ProductService>();

// 設定 CORS (如果需要)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 設定 HTTP 請求管線
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        c.RoutePrefix = string.Empty; // 設定 Swagger UI 為根路徑
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 確保資料庫已建立
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Console.WriteLine("資料庫連接成功！");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"資料庫連接失敗：{ex.Message}");
    }
}

Console.WriteLine("API 已啟動，請訪問 https://localhost:5001 查看 Swagger UI");
app.Run();
```

## 8. 設定 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductApiDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## 9. 執行資料庫遷移（詳細說明）

### 什麼是資料庫遷移？
資料庫遷移是 Entity Framework Core 用來管理資料庫結構變更的機制。它會根據你的模型類別自動產生 SQL 指令來建立或更新資料庫。

### 步驟 1：新增遷移
```bash
dotnet ef migrations add InitialCreate
```

**這個指令的作用：**
- `dotnet ef`：使用 Entity Framework Core 工具
- `migrations add`：新增一個遷移檔案
- `InitialCreate`：遷移的名稱（你可以自訂）

**執行後會發生什麼：**
- 在專案中建立 `Migrations` 資料夾
- 產生三個檔案：
  - `20240101000000_InitialCreate.cs`（實際時間戳會不同）
  - `20240101000000_InitialCreate.Designer.cs`
  - `ApplicationDbContextModelSnapshot.cs`

**如果遇到錯誤：**
```bash
# 如果出現 "Unable to create an object of type 'ApplicationDbContext'"
# 需要先安裝 EF Core 工具：
dotnet tool install --global dotnet-ef

# 或更新工具：
dotnet tool update --global dotnet-ef
```

### 步驟 2：更新資料庫
```bash
dotnet ef database update
```

**這個指令的作用：**
- 讀取遷移檔案中的指令
- 連接到資料庫（根據 appsettings.json 中的連接字串）
- 執行 SQL 指令來建立資料表和結構
- 插入種子資料

**執行成功後你會看到：**
```
Build started...
Build succeeded.
Applying migration '20240101000000_InitialCreate'.
Done.
```

**資料庫中會建立：**
- `Products` 資料表
- `__EFMigrationsHistory` 資料表（追蹤遷移歷史）
- 預設的三筆商品資料

## 10. 執行專案（詳細說明）

### 基本啟動
```bash
dotnet run
```

**這個指令會：**
1. 編譯專案
2. 檢查相依性
3. 啟動 Web 伺服器
4. 顯示應用程式 URL

**你會看到類似的輸出：**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]  
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shutdown.
資料庫連接成功！
API 已啟動，請訪問 https://localhost:5001 查看 Swagger UI
```

### 進階啟動選項

**指定環境：**
```bash
# 開發環境（預設）
dotnet run --environment Development

# 正式環境
dotnet run --environment Production

# 測試環境
dotnet run --environment Staging
```

**指定埠號：**
```bash
# 使用特定埠號
dotnet run --urls "https://localhost:7001;http://localhost:7000"
```

**監看模式（開發時建議使用）：**
```bash
# 檔案變更時自動重啟
dotnet watch run
```

### 驗證是否成功啟動

**1. 瀏覽器測試：**
- 開啟瀏覽器，訪問 `https://localhost:5001`
- 應該會看到 Swagger UI 介面
- 可以看到所有的 API 端點

**2. API 測試：**
```bash
# 使用 curl 測試（如果有安裝）
curl -X GET "https://localhost:5001/api/products" -H "accept: application/json"

# 或使用 PowerShell
Invoke-RestMethod -Uri "https://localhost:5001/api/products" -Method Get
```

**3. 檢查資料庫：**
- 使用 SQL Server Management Studio 或 Visual Studio 的 SQL Server 物件總管
- 連接到 `(localdb)\mssqllocaldb`
- 查看 `ProductApiDb` 資料庫
- 確認 `Products` 資料表存在且有資料

### 常見問題和解決方案

**問題 1：埠號被占用**
```
Error: Unable to start Kestrel. System.IO.IOException: Failed to bind to address https://127.0.0.1:5001
```
**解決方案：**
```bash
# 使用不同的埠號
dotnet run --urls "https://localhost:6001;http://localhost:6000"
```

**問題 2：資料庫連接失敗**
```
資料庫連接失敗：A network-related or instance-specific error occurred
```
**解決方案：**
1. 確認 SQL Server LocalDB 已安裝
2. 檢查 appsettings.json 中的連接字串
3. 嘗試手動建立資料庫：
```bash
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

**問題 3：SSL 憑證警告**
瀏覽器可能顯示 SSL 憑證不受信任的警告。
**解決方案：**
```bash
# 信任開發憑證
dotnet dev-certs https --trust
```

### 完整的執行流程範例

```bash
# 1. 確保在專案根目錄
cd ProductApi

# 2. 還原套件（如果需要）
dotnet restore

# 3. 建立遷移
dotnet ef migrations add InitialCreate

# 4. 更新資料庫
dotnet ef database update

# 5. 啟動專案（監看模式）
dotnet watch run
```

成功啟動後，你就可以：
- 透過 Swagger UI 測試 API
- 使用 Postman 或其他工具發送 HTTP 請求
- 開始開發前端應用程式來呼叫這些 API

## API 端點說明：

- `GET /api/products` - 取得所有商品
- `GET /api/products/{id}` - 取得特定商品
- `POST /api/products` - 建立新商品
- `PUT /api/products/{id}` - 更新商品
- `DELETE /api/products/{id}` - 刪除商品
- `GET /api/products/search?searchTerm=關鍵字` - 搜尋商品

## 測試範例：

使用 POST 建立商品：
```json
{
  "name": "智慧型手機",
  "description": "最新款智慧型手機",
  "price": 15000,
  "stock": 25,
  "isActive": true
}
```