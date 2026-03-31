using EStoreX.Core.BackgroundJobs.Interfaces;
using EStoreX.Core.BackgroundJobs.Jobs;
using EStoreX.Core.BackgroundJobs.Wrapper;
using EStoreX.Core.Domain.Options;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Account;
using EStoreX.Core.ServiceContracts.Basket;
using EStoreX.Core.ServiceContracts.Categories;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Discount;
using EStoreX.Core.ServiceContracts.Favourites;
using EStoreX.Core.ServiceContracts.Orders;
using EStoreX.Core.ServiceContracts.Products;
using EStoreX.Core.ServiceContracts.Ratings;
using EStoreX.Core.Services.Account;
using EStoreX.Core.Services.Basket;
using EStoreX.Core.Services.Categories;
using EStoreX.Core.Services.Common;
using EStoreX.Core.Services.Favourites;
using EStoreX.Core.Services.Orders;
using EStoreX.Core.Services.Products;
using EStoreX.Core.Services.Ratings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MyDiscountService = EStoreX.Core.Services.Discounts.DiscountService;
//using EStoreX.Core.Services.Common;

[assembly: Microsoft.Extensions.Localization.RootNamespace("EStoreX.Core")]

namespace EStoreX.Core
{
    public static class CoreServiceRegistration
    {
        public static IServiceCollection ConfigureCore(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<ICategoriesService, CategoriesService>();

            services.AddSingleton<IImageService, ImageService>();

            services.AddScoped(typeof(IEntityImageManager<>), typeof(EntityImageManager<>));

            services.AddScoped<IProductsService, ProductsService>();

            services.AddScoped<IBasketService, BasketService>();

            services.AddScoped<IEmailSenderService, EmailSenderService>();

            services.AddTransient<IJwtService, JwtService>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();

            services.AddScoped<PaymentIntentService>();

            services.AddScoped<IPaymentService, PaymentService>();

            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IExportService, ExportService>();

            services.AddScoped<IApiClientService, ApiClientService>();

            services.AddScoped<IUserManagementService, UserManagementService>();

            services.AddScoped<IFavouriteService, FavouriteService>();

            services.AddScoped<IRatingService, RatingService>();

            services.AddScoped<IBrandService, BrandService>();

            services.AddScoped<IDeliveryMethodService, DeliveryMethodService>();

            services.AddScoped<IDiscountService, MyDiscountService>();

            services.AddScoped<IEmailJob, EmailJob>();

            services.AddScoped<IBackgroundJobClientWrapper, BackgroundJobClientWrapper>();

            // JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //ClockSkew = TimeSpan.Zero, // Prevents the default 5-minute clock drift tolerance when validating token expiration
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudiences = configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
                    RoleClaimType = ClaimTypes.Role,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); 
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(ApiResponseFactory.Unauthorized("You are not authorized."));
                        return context.Response.WriteAsync(result);
                    }
                };
            }).AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            }).AddGitHub(options =>
            {
                options.ClientId = configuration["Authentication:Github:ClientId"];
                options.ClientSecret = configuration["Authentication:Github:ClientSecret"];
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("SuperAdminOnly", policy =>
                    policy.RequireRole("SuperAdmin"));

                options.AddPolicy("AdminOrSuperAdmin", policy =>
                    policy.RequireRole("Admin", "SuperAdmin"));

                options.AddPolicy("NotAuthorized", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return !context.User.Identity.IsAuthenticated;
                    });
                });
            });


            services.Configure<StripeSettings>(configuration.GetSection("StripeSetting"));

            return services;
        }
    }
}
