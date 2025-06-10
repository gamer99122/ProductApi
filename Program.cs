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