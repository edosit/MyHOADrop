using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation.AspNetCore;
using MyHOADrop.Data;
using MyHOADrop.Models;
using MyHOADrop.Validators;
using MyHOADrop.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure EF Core and Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2) Configure Identity (Individual Accounts)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3) Configure MVC + FluentValidation
builder.Services
    .AddControllersWithViews()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<UploadViewModelValidator>();
        fv.DisableDataAnnotationsValidation = true;
    });

// 4) Add Razor Pages (required for Identity UI)
builder.Services.AddRazorPages();

// 5) Register custom services
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

// 6) Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint(); // shows EF migration errors, etc.
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // <� make sure Authentication comes before Authorization
app.UseAuthorization();

// 7) Configure endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();        // <� maps Identity�s Razor Pages and any other *.cshtml pages

app.Run();
