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