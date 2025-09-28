using BlogApp.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Repositories;
using BlogApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BlogAppContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BlogAppContext") ?? throw new InvalidOperationException("Connection string 'BlogAppContext' not found.")));


// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register auth service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, OwnershipHandler<Post>>(); // generic handler registrations won't work directly
builder.Services.AddScoped<IAuthorizationHandler, OwnershipHandler<Comment>>();
builder.Services.AddScoped<IAuthorizationHandler, OwnershipHandler<Blog>>();


builder.Services.AddAuthorization(options => {
    options.AddPolicy("OwnerOnly", policy => policy.Requirements.Add(new OwnershipRequirement()));
});

// JWT
builder.Services.AddAuthentication(options =>
{
    // Default scheme for MVC cookie login
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "VerySecretKeyForDev");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});


builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
