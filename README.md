# ASP.NET Core Web API �s�� MSSQL ����d��

## 1. �إ߱M�רæw�ˮM��

�����إ߷s�� Web API �M�סG
```bash
dotnet new webapi -n ProductApi
cd ProductApi
```

�w�˥��n�� NuGet �M��G
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

## 2. �إ߸�Ƽҫ� (Models/Product.cs)

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

## 3. �إ߸�Ʈw�W�U�� (Data/ApplicationDbContext.cs)

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
            
            // �]�w��ƪ�W��
            modelBuilder.Entity<Product>().ToTable("Products");
            
            // �]�w����
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();
                
            // �ؤl���
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "���O���q��", Description = "���į൧�O���q��", Price = 25000, Stock = 10 },
                new Product { Id = 2, Name = "�ƹ�", Description = "�L�u���Ƿƹ�", Price = 500, Stock = 50 },
                new Product { Id = 3, Name = "��L", Description = "������L", Price = 1200, Stock = 30 }
            );
        }
    }
}
```

## 4. �إ� DTO ���O (DTOs/ProductDto.cs)

```csharp
using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "�ӫ~�W�٬�����")]
        [StringLength(100, ErrorMessage = "�ӫ~�W�٤���W�L100�Ӧr��")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "�ӫ~�y�z����W�L500�Ӧr��")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "���欰����")]
        [Range(0, double.MaxValue, ErrorMessage = "���楲���j��0")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "�w�s������")]
        [Range(0, int.MaxValue, ErrorMessage = "�w�s���ର�t��")]
        public int Stock { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class ProductUpdateDto
    {
        [StringLength(100, ErrorMessage = "�ӫ~�W�٤���W�L100�Ӧr��")]
        public string? Name { get; set; }
        
        [StringLength(500, ErrorMessage = "�ӫ~�y�z����W�L500�Ӧr��")]
        public string? Description { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "���楲���j��0")]
        public decimal? Price { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "�w�s���ର�t��")]
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

## 5. �إߪA�Ȥ����M��@ (Services/IProductService.cs �M ProductService.cs)

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
                
            // �n�R���G�]�w�������D�Ӥ��O�u���R��
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

## 6. �إ߱�� (Controllers/ProductsController.cs)

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
        /// ���o�Ҧ��ӫ~
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
                _logger.LogError(ex, "���o�ӫ~�C��ɵo�Ϳ��~");
                return StatusCode(500, "���A���������~");
            }
        }
        
        /// <summary>
        /// �ھ�ID���o�ӫ~
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"�䤣��ID�� {id} ���ӫ~");
                }
                
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "���o�ӫ~ {ProductId} �ɵo�Ϳ��~", id);
                return StatusCode(500, "���A���������~");
            }
        }
        
        /// <summary>
        /// �إ߷s�ӫ~
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
                _logger.LogError(ex, "�إ߰ӫ~�ɵo�Ϳ��~");
                return StatusCode(500, "���A���������~");
            }
        }
        
        /// <summary>
        /// ��s�ӫ~
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
                    return NotFound($"�䤣��ID�� {id} ���ӫ~");
                }
                
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��s�ӫ~ {ProductId} �ɵo�Ϳ��~", id);
                return StatusCode(500, "���A���������~");
            }
        }
        
        /// <summary>
        /// �R���ӫ~
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound($"�䤣��ID�� {id} ���ӫ~");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�R���ӫ~ {ProductId} �ɵo�Ϳ��~", id);
                return StatusCode(500, "���A���������~");
            }
        }
        
        /// <summary>
        /// �j�M�ӫ~
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("�j�M����r���ର��");
                }
                
                var products = await _productService.SearchProductsAsync(searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�j�M�ӫ~�ɵo�Ϳ��~�A����r: {SearchTerm}", searchTerm);
                return StatusCode(500, "���A���������~");
            }
        }
    }
}
```

## 7. �]�w Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductApi.Data;
using ProductApi.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ���U�A��
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ���U Swagger �A��
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Product API", 
        Version = "v1",
        Description = "�ӫ~�޲z API"
    });
    
    // �p�G�� XML �����ɮסA�i�H�[�J�H�U�]�w
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

// ���U��Ʈw�W�U��
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���U�ۭq�A��
builder.Services.AddScoped<IProductService, ProductService>();

// �]�w CORS (�p�G�ݭn)
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

// �]�w HTTP �ШD�޽u
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        c.RoutePrefix = string.Empty; // �]�w Swagger UI ���ڸ��|
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// �T�O��Ʈw�w�إ�
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Console.WriteLine("��Ʈw�s�����\�I");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"��Ʈw�s�����ѡG{ex.Message}");
    }
}

Console.WriteLine("API �w�ҰʡA�гX�� https://localhost:5001 �d�� Swagger UI");
app.Run();
```

## 8. �]�w appsettings.json

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

## 9. �����Ʈw�E���]�Բӻ����^

### ����O��Ʈw�E���H
��Ʈw�E���O Entity Framework Core �ΨӺ޲z��Ʈw���c�ܧ󪺾���C���|�ھڧA���ҫ����O�۰ʲ��� SQL ���O�ӫإߩΧ�s��Ʈw�C

### �B�J 1�G�s�W�E��
```bash
dotnet ef migrations add InitialCreate
```

**�o�ӫ��O���@�ΡG**
- `dotnet ef`�G�ϥ� Entity Framework Core �u��
- `migrations add`�G�s�W�@�ӾE���ɮ�
- `InitialCreate`�G�E�����W�١]�A�i�H�ۭq�^

**�����|�o�ͤ���G**
- �b�M�פ��إ� `Migrations` ��Ƨ�
- ���ͤT���ɮסG
  - `20240101000000_InitialCreate.cs`�]��ڮɶ��W�|���P�^
  - `20240101000000_InitialCreate.Designer.cs`
  - `ApplicationDbContextModelSnapshot.cs`

**�p�G�J����~�G**
```bash
# �p�G�X�{ "Unable to create an object of type 'ApplicationDbContext'"
# �ݭn���w�� EF Core �u��G
dotnet tool install --global dotnet-ef

# �Χ�s�u��G
dotnet tool update --global dotnet-ef
```

### �B�J 2�G��s��Ʈw
```bash
dotnet ef database update
```

**�o�ӫ��O���@�ΡG**
- Ū���E���ɮפ������O
- �s�����Ʈw�]�ھ� appsettings.json �����s���r��^
- ���� SQL ���O�ӫإ߸�ƪ�M���c
- ���J�ؤl���

**���榨�\��A�|�ݨ�G**
```
Build started...
Build succeeded.
Applying migration '20240101000000_InitialCreate'.
Done.
```

**��Ʈw���|�إߡG**
- `Products` ��ƪ�
- `__EFMigrationsHistory` ��ƪ�]�l�ܾE�����v�^
- �w�]���T���ӫ~���

## 10. ����M�ס]�Բӻ����^

### �򥻱Ұ�
```bash
dotnet run
```

**�o�ӫ��O�|�G**
1. �sĶ�M��
2. �ˬd�̩ۨ�
3. �Ұ� Web ���A��
4. ������ε{�� URL

**�A�|�ݨ���������X�G**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]  
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shutdown.
��Ʈw�s�����\�I
API �w�ҰʡA�гX�� https://localhost:5001 �d�� Swagger UI
```

### �i���Ұʿﶵ

**���w���ҡG**
```bash
# �}�o���ҡ]�w�]�^
dotnet run --environment Development

# ��������
dotnet run --environment Production

# ��������
dotnet run --environment Staging
```

**���w�𸹡G**
```bash
# �ϥίS�w��
dotnet run --urls "https://localhost:7001;http://localhost:7000"
```

**�ʬݼҦ��]�}�o�ɫ�ĳ�ϥΡ^�G**
```bash
# �ɮ��ܧ�ɦ۰ʭ���
dotnet watch run
```

### ���ҬO�_���\�Ұ�

**1. �s�������աG**
- �}���s�����A�X�� `https://localhost:5001`
- ���ӷ|�ݨ� Swagger UI ����
- �i�H�ݨ�Ҧ��� API ���I

**2. API ���աG**
```bash
# �ϥ� curl ���ա]�p�G���w�ˡ^
curl -X GET "https://localhost:5001/api/products" -H "accept: application/json"

# �Ψϥ� PowerShell
Invoke-RestMethod -Uri "https://localhost:5001/api/products" -Method Get
```

**3. �ˬd��Ʈw�G**
- �ϥ� SQL Server Management Studio �� Visual Studio �� SQL Server �����`��
- �s���� `(localdb)\mssqllocaldb`
- �d�� `ProductApiDb` ��Ʈw
- �T�{ `Products` ��ƪ�s�b�B�����

### �`�����D�M�ѨM���

**���D 1�G�𸹳Q�e��**
```
Error: Unable to start Kestrel. System.IO.IOException: Failed to bind to address https://127.0.0.1:5001
```
**�ѨM��סG**
```bash
# �ϥΤ��P����
dotnet run --urls "https://localhost:6001;http://localhost:6000"
```

**���D 2�G��Ʈw�s������**
```
��Ʈw�s�����ѡGA network-related or instance-specific error occurred
```
**�ѨM��סG**
1. �T�{ SQL Server LocalDB �w�w��
2. �ˬd appsettings.json �����s���r��
3. ���դ�ʫإ߸�Ʈw�G
```bash
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

**���D 3�GSSL ����ĵ�i**
�s�����i����� SSL ���Ҥ����H����ĵ�i�C
**�ѨM��סG**
```bash
# �H���}�o����
dotnet dev-certs https --trust
```

### ���㪺����y�{�d��

```bash
# 1. �T�O�b�M�׮ڥؿ�
cd ProductApi

# 2. �٭�M��]�p�G�ݭn�^
dotnet restore

# 3. �إ߾E��
dotnet ef migrations add InitialCreate

# 4. ��s��Ʈw
dotnet ef database update

# 5. �ҰʱM�ס]�ʬݼҦ��^
dotnet watch run
```

���\�Ұʫ�A�A�N�i�H�G
- �z�L Swagger UI ���� API
- �ϥ� Postman �Ψ�L�u��o�e HTTP �ШD
- �}�l�}�o�e�����ε{���өI�s�o�� API

## API ���I�����G

- `GET /api/products` - ���o�Ҧ��ӫ~
- `GET /api/products/{id}` - ���o�S�w�ӫ~
- `POST /api/products` - �إ߷s�ӫ~
- `PUT /api/products/{id}` - ��s�ӫ~
- `DELETE /api/products/{id}` - �R���ӫ~
- `GET /api/products/search?searchTerm=����r` - �j�M�ӫ~

## ���սd�ҡG

�ϥ� POST �إ߰ӫ~�G
```json
{
  "name": "���z�����",
  "description": "�̷s�ڴ��z�����",
  "price": 15000,
  "stock": 25,
  "isActive": true
}
```