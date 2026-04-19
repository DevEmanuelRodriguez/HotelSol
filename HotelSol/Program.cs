using HotelSol.Data;
using HotelSol.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;//para autentificar

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
//para sesiones
builder.Services.AddSession();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Login";
});
builder.Services.AddScoped<XmlService>();

builder.Services.AddDbContext<DbHotelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
//Para autentificaciones
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Login}/{action=Index}/{id?}");//inicia en pantalla login


app.Run();
