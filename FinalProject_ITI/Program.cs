using FinalProject_ITI.Helpers;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using FinalProject_ITI.Repositories.Interfaces;
using FinalProject_ITI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Mscc.GenerativeAI;
using System.Security.Claims;
using System.Text;

namespace FinalProject_ITI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<EmbeddingService>();
        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

        builder.Services.AddDbContext<ITIContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("finProCS"),
    sqlServerOptions => sqlServerOptions.UseNetTopologySuite()));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ITIContext>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped(typeof(IBazarBrandRepository<>), typeof(BazarBrandRepository<>));

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        });
      
        builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
              options.TokenValidationParameters = new TokenValidationParameters
              {
                  ValidateIssuer = true,
                  ValidateAudience = true,
                  ValidateLifetime = true,
                  ValidateIssuerSigningKey = true,
                  ValidIssuer = "http://localhost:5066/",
                  ValidAudience = "any",
                  IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("hfsbvdfjknsfkns@44&&%%$$dcskln1548vkls2sdbfbdnklf554d$$##")
                  ),
              };
          });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });


        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient<ChatService>(); // ????? ??????

        builder.Services.AddSingleton<GoogleAI>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var key = cfg["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("Gemini API Key not configured. Set Gemini:ApiKey in config or env.");
            return new GoogleAI(key);
        });
        builder.Services.AddScoped<EmbeddingService>();

        var app = builder.Build();


        await SeedRoles.SeedRolesAndAdminAsync(app);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseStaticFiles();




        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
