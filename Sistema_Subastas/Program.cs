using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Sistema_Subastas.Models;
using Sistema_Subastas.Services;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(600);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Inyeccion a la base de datos
// Configurar la conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("subastaDbConnection");
builder.Services.AddDbContext<subastaDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



builder.Services.AddScoped<EmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Múltiples carpetas estáticas
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "css_E")),
//    RequestPath = "/css_E"
//});

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "css_R")),
//    RequestPath = "/css_R"
//});
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "css_C")),
//    RequestPath = "/css_C"
//});
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "css_C")),
//    RequestPath = "/css_L"
//});

// Sirve archivos de wwwroot
app.UseStaticFiles(); 

// Luego sirve carpetas externas
string[] cssFolders = { "css_E", "css_R", "css_C", "css_L" };
foreach (var folder in cssFolders)
{
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), folder);
    if (Directory.Exists(fullPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(fullPath),
            RequestPath = "/" + folder
        });
    }
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseSession();
app.MapHub<NotificacionHub>("/notificacionHub");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}");

app.Run();
