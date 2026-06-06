using ExamenAzure.Data;
using ExamenAzure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Підключення Application Insights (Етап 5)
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
});

// 2. Додавання MVC контролерів та представлень
builder.Services.AddControllersWithViews();

// 3. Реєстрація SQL Server з EF Core (Етап 2)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Реєстрація власного BlobService (Етап 3)
builder.Services.AddScoped<IBlobService, BlobService>();

var app = builder.Build();

// Виконання міграцій (опціонально для автоматизації при старті)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Створить базу та таблиці, якщо їх немає
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}");
    //.WithStaticAssets();


app.Run();
