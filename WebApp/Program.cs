using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Libria.Models;
using Libria.Data;
using Libria.Services;
using Libria.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Get path to the folder with application secrets
string? secrets_path = Environment.GetEnvironmentVariable("LIBRIA_SECRETS_PATH");
secrets_path ??= ""; // At last try to get configuration from current directory
builder.Configuration.AddJsonFile($"{secrets_path}appsettings_ext.json", false, true);

// Add services to the container.
builder.Services.AddDbContext<LibriaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<LibriaDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<UkrainianIdentityErrorDescriber>();

// Configuring Email
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));

builder.Services.AddTransient<IUserCartService, UserCartService>();

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


var app = builder.Build();

// This is recommended to use when a reverse proxy is used
app.UseForwardedHeaders();

app.Use((context, next) =>
{
	var result = context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var value);
	if (result == true && value.ToString().ToUpper().Equals("HTTPS"))
	{
		context.Request.Scheme = "https";
	}
    app.Logger.LogInformation($"{result}; {value}");
	return next(context);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
