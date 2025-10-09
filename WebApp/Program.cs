using DotNetEnv;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using Microsoft.EntityFrameworkCore;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ApplicationDbContext.GetConnectionStringFromENV()));

    builder.Services.AddScoped<CategoryRepository>();
    builder.Services.AddScoped<TransactionRepository>();

    builder.Services.AddScoped<CategoryService>();
    builder.Services.AddScoped<TransactionService>();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
}
var app = builder.Build();

// DB Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
