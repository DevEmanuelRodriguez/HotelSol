using System.Diagnostics;
using System.Globalization;
using HotelSol.Data;
using HotelSol.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Login";
});

builder.Services.AddScoped<XmlService>();

builder.Services.AddDbContext<DbHotelContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// CULTURA DECIMALES
var culture = new CultureInfo("en-US");

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
};


// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization(localizationOptions);

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");


// Abrir navegador automático
var url = "http://localhost:5000";

Task.Run(() =>
{
    Thread.Sleep(1500);

    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
});

app.Run(url);