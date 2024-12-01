
using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaiGiuaKy.Models.MoMo;
using BaiGiuaKy.Service;
using Microsoft.AspNetCore.Authentication.Google;
using System.Configuration;
using BaiGiuaKy.Repositories.BaiGiuaKy.Repositories;
using BaiGiuaky.Hubs;
using BaiGiuaky.Service.Vnpay;
using BaiGiuaky.Libraries;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
		.AddDefaultTokenProviders()
		.AddDefaultUI()
		.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();
builder.Services.AddScoped<IDiscountRepository, EFDiscountRepository>();
var configuration = builder.Configuration;
builder.Services.AddAuthentication().AddGoogle(googleoptions =>
{
	googleoptions.ClientId = configuration["Authentication:Google:ClientId"];
	googleoptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];

}).AddFacebook(FacebookOptions =>
{
    FacebookOptions.AppId = configuration["Authentication:Facebook:AppId"];
    FacebookOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"];
	FacebookOptions.CallbackPath = "/signin-facebook";
});

builder.Services.AddHttpClient();

builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<ChatHub>("/chathub");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute(
		name: "admin",
		pattern: "{area:exists}/{controller=Category}/{action=Index}/{id?}"
	);

	endpoints.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}"
	);
});

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Product}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
// Định nghĩa EmailSender Service

