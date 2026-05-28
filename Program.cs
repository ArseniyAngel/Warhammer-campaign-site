using Microsoft.EntityFrameworkCore;
using CampaignApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Строка подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Регистрация контекста БД (Лекция 4)
builder.Services.AddDbContext<CampaignContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Если неавторизованный лезет куда нельзя, сервер вернет 401 ошибку вместо редиректа
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

// Добавление поддержки контроллеров (Лекция 9)
builder.Services.AddControllers();

// Настройка Swagger для удобного тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Настройка конвейера Middleware (Лекция 1 и Лекция 9)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Страница Swagger будет доступна по адресу /swagger
}

// !!! ВАЖНО: Включаем маршрутизацию в конвейере запросов !!!
app.UseRouting();
app.UseAuthentication(); // 1. Кто ты такой?
app.UseAuthorization();
// Инициализация базы данных стартовыми фракциями и регионами
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CampaignContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла ошибка при заполнении базы данных.");
    }
}
app.UseDefaultFiles(); 

// 2. Разрешает раздачу файлов (css, js, html) из папки wwwroot
app.UseStaticFiles();
// Привязываем контроллеры к маршрутам
app.MapControllers();

// Запуск веб-сервера
app.Run();