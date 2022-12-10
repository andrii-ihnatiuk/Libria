using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Libria.Models;
using Libria.Data;
using Libria.Services;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddHttpLogging(options =>
{
	options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders;
	options.RequestHeaders.Add("X-Forwarded-For");
	options.RequestHeaders.Add("X-Forwarded-Proto");
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

builder.Services.AddControllersWithViews();

var app = builder.Build();

// This is recommended to use when a reverse proxy is used
app.UseForwardedHeaders();

app.UseHttpLogging();

app.Use(async (context, next) =>
{
	// Connection: RemoteIp
	app.Logger.LogInformation("Request RemoteIp: {RemoteIpAddress}",
		context.Connection.RemoteIpAddress);

	await next(context);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

// We don't need it because nginx proxy_pass by http
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
