using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using GourmetGallery.Infrastructure;
using GourmetGallery.Infrastructure.Repositories;
using GourmeyGalleryApp.Infrastructure;
using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Repositories.RecipeRepository;
using GourmeyGalleryApp.Repositories.UserRepo;
using GourmeyGalleryApp.Services;
using GourmeyGalleryApp.Services.EmailService;
using GourmeyGalleryApp.Services.RecipeService;
using GourmeyGalleryApp.Services.UserService.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Azure;
using GourmeyGalleryApp.Repositories.CategoryRepository;
using GourmeyGalleryApp.Services.CategoryService;
using GourmeyGalleryApp.Repositories.BadgeRepository;
using GourmeyGalleryApp.Services.BadgeService;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Polly;
using GourmeyGalleryApp.Utils.FactoryPolicies;
using StackExchange.Redis;
using GourmeyGalleryApp.Services.NewsletterService;
using Hangfire;
using GourmeyGalleryApp.Shared.SignalRHub;
using Microsoft.AspNetCore.SignalR;
using GourmeyGalleryApp.Services.NotificationService;



var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// builder.WebHost.UseUrls("http://*:80");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddDbContext<GourmetGalleryContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));
//options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<GourmetGalleryContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(1); // Set token expiration to 1 hours
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
});

// Polly rate limiter for requests 
builder.Services.AddSingleton<IAsyncPolicyFactory>(serviceProvider =>
{
    return new AsyncPolicyFactory(new Dictionary<string, IAsyncPolicy>
    {
        { "ResendEmailPolicy", Policy.RateLimitAsync(1, TimeSpan.FromMinutes(15)) }, // 1 request per 15 minutes
        { "MarkAsHelpfulPolicy", Policy.RateLimitAsync(1, TimeSpan.FromSeconds(7)) }, // 1 requests per 7 seconds
        { "NotificationMarkAsReadPolicy", Policy.RateLimitAsync(1, TimeSpan.FromSeconds(3)) }, // 1 requests per 3 seconds
    });
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<BlobStorageService>(sp =>
        new BlobStorageService(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<NewsletterService>();
builder.Services.AddSingleton<JobScheduler>(); // Register the JobScheduler class
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use 'Always' for production
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddJwtBearer(jwt =>
{
    jwt.MapInboundClaims = false;
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]);
    jwt.RequireHttpsMetadata = true;
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role"
    };
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    googleOptions.CallbackPath = "/signin-google"; // Set the callback URL
    googleOptions.Events.OnCreatingTicket = async context =>
    {
        // Retrieve UserManager from the service provider
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

        var email = context.Principal.FindFirstValue(ClaimTypes.Email);
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // Google guarantees the email is verified
            };

            await userManager.CreateAsync(user);
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://gourmetgallery.azurewebsites.net", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Allow cookies for SignalR or other uses
    });
});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin", builder => builder
//        .WithOrigins("https://localhost:4200")
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .AllowCredentials());
//});

//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//    var redisConnectionString = builder.Configuration["RedisCacheSettings:ConnectionString"];
//    if (string.IsNullOrEmpty(redisConnectionString))
//    {
//        throw new ArgumentNullException("Redis connection string is not configured");
//    }

//    return ConnectionMultiplexer.Connect(redisConnectionString);
//});

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();

builder.Services.AddSignalR( options =>
{
    options.EnableDetailedErrors = false;
    options.KeepAliveInterval = TimeSpan.FromSeconds(60);
})
      .AddAzureSignalR(options =>
      {
          options.ConnectionString = builder.Configuration["AzureSignalR:ConnectionString"];

      });


builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GourmetGallery API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new string[] {}
        }
    });
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureBlobStorage:ConnectionString:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureBlobStorage:ConnectionString:queue"], preferMsi: true);
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if(app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHangfireDashboard();
app.MapHangfireDashboard();

var jobScheduler = app.Services.GetRequiredService<JobScheduler>();
jobScheduler.ConfigureRecurringJobs();

app.UseHttpsRedirection();

//app.UseCors("AllowSpecificOrigin");

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub").RequireCors("AllowSpecificOrigin");

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GourmetGalleryContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations.");
    }
}

app.Run();
