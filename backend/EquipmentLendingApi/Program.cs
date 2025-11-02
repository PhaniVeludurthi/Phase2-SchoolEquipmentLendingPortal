using EquipmentLendingApi.Data;
using EquipmentLendingApi.Filters;
using EquipmentLendingApi.Middleware;
using EquipmentLendingApi.Model;
using EquipmentLendingApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting Equipment Lending API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/equipment-lending-.log", rollingInterval: RollingInterval.Day));


    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"))
        .AddPolicy("StaffOrAdmin", policy => policy.RequireRole("staff", "admin"));

    builder.Services.AddValidatorsFromAssemblyContaining<UserRegisterDtoValidator>();

    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation(config =>
    {
        config.OverrideDefaultResultFactoryWith<CustomValidationResultFactory>();
    });

    // Add FluentValidation automatic validation
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Equipment Lending Portal API",
            Version = "v1",
            Description = "API for managing school equipment lending with JWT authentication. " +
                         "This API provides endpoints for user authentication, equipment management, " +
                         "and request handling for lending school equipment.",
            Contact = new OpenApiContact
            {
                Name = "Equipment Lending Team",
                Email = "support@school.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License"
            }
        });

        // Include XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                          "Enter your token in the text input below.\r\n\r\n" +
                          "Example: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                In = ParameterLocation.Header
            },
            new List<string>()
        }
        });

    });

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    var allowedOrigins = builder.Configuration["AllowedOrigins"];
    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            builder => builder
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader());
    });
    builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Add Serilog request logging
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Equipment Lending API v1");
        options.DocumentTitle = "Equipment Lending Portal API";
        options.RoutePrefix = "swagger";

        // Enable authorization persistence
        options.EnablePersistAuthorization();
    });

    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        async Task SeedEquipmentDataAsync(AppDbContext db, ILogger<Program> logger)
        {
            try
            {
                // Check if any equipment already exists
                var equipmentCount = await db.Equipment.CountAsync();
                if (equipmentCount > 0)
                {
                    logger.LogInformation("Equipment seed data already exists ({Count} items). Skipping seed.", equipmentCount);
                    return;
                }

                logger.LogInformation("No equipment found. Seeding equipment data...");

                var seedEquipment = new List<Equipment>
                {
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Laptop - Dell Inspiron 15",
                        Category = "Computers",
                        Quantity = 20,
                        AvailableQuantity = 20,
                        Description = "15.6-inch laptop with Intel i5 processor, 8GB RAM, 256GB SSD. Perfect for programming and general computing tasks.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Laptop - MacBook Pro 13",
                        Category = "Computers",
                        Quantity = 15,
                        AvailableQuantity = 15,
                        Description = "13-inch MacBook Pro with M1 chip, 8GB RAM, 256GB SSD. Ideal for design and development work.",
                        Condition = "Excellent",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Projector - Epson PowerLite",
                        Category = "AV Equipment",
                        Quantity = 10,
                        AvailableQuantity = 10,
                        Description = "High-brightness projector with 1080p resolution. Suitable for presentations and classroom use.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Camera - Canon EOS Rebel T7",
                        Category = "Photography",
                        Quantity = 8,
                        AvailableQuantity = 8,
                        Description = "24.1MP DSLR camera with 18-55mm lens kit. Great for photography classes and events.",
                        Condition = "Excellent",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Microphone - Blue Yeti USB",
                        Category = "Audio",
                        Quantity = 12,
                        AvailableQuantity = 12,
                        Description = "USB condenser microphone with multiple pattern selection. Perfect for podcasting and video recording.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Tablet - iPad Air 10.9",
                        Category = "Tablets",
                        Quantity = 25,
                        AvailableQuantity = 25,
                        Description = "10.9-inch iPad Air with A14 Bionic chip and 64GB storage. Ideal for digital art and note-taking.",
                        Condition = "Excellent",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "3D Printer - Ender 3 Pro",
                        Category = "Manufacturing",
                        Quantity = 5,
                        AvailableQuantity = 5,
                        Description = "FDM 3D printer with 220x220x250mm build volume. Perfect for engineering and design projects.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Microscope - AmScope M150C",
                        Category = "Science Lab",
                        Quantity = 15,
                        AvailableQuantity = 15,
                        Description = "Compound microscope with 40x-1000x magnification. Essential for biology and chemistry labs.",
                        Condition = "Excellent",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Oscilloscope - Siglent SDS1104X-E",
                        Category = "Electronics",
                        Quantity = 6,
                        AvailableQuantity = 6,
                        Description = "4-channel digital oscilloscope with 100MHz bandwidth. Used in electronics and engineering courses.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "VR Headset - Oculus Quest 2",
                        Category = "VR/AR",
                        Quantity = 10,
                        AvailableQuantity = 10,
                        Description = "Standalone VR headset with 128GB storage. Great for virtual reality experiences and educational applications.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Drawing Tablet - Wacom Intuos Pro",
                        Category = "Design",
                        Quantity = 8,
                        AvailableQuantity = 8,
                        Description = "Professional graphics tablet with 8192 pressure levels. Perfect for digital art and design work.",
                        Condition = "Excellent",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Drone - DJI Mini 3",
                        Category = "Drones",
                        Quantity = 4,
                        AvailableQuantity = 4,
                        Description = "Compact drone with 4K camera and 38-minute flight time. Suitable for aerial photography and videography projects.",
                        Condition = "Excellent",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Arduino Starter Kit",
                        Category = "Electronics",
                        Quantity = 20,
                        AvailableQuantity = 20,
                        Description = "Complete Arduino Uno starter kit with breadboard, LEDs, sensors, and components. Ideal for learning electronics and programming.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Raspberry Pi 4 Model B",
                        Category = "Computers",
                        Quantity = 15,
                        AvailableQuantity = 15,
                        Description = "Single-board computer with 4GB RAM, USB 3.0, and Gigabit Ethernet. Great for IoT projects and programming education.",
                        Condition = "Good",
                        IsDeleted = false
                    },
                    new Equipment
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Audio Interface - Focusrite Scarlett 2i2",
                        Category = "Audio",
                        Quantity = 10,
                        AvailableQuantity = 10,
                        Description = "2-input, 2-output USB audio interface with phantom power. Perfect for music production and recording.",
                        Condition = "Excellent",
                        IsDeleted = false
                    }
                };

                await db.Equipment.AddRangeAsync(seedEquipment);
                await db.SaveChangesAsync();

                logger.LogInformation("Successfully seeded {Count} equipment items.", seedEquipment.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while seeding equipment data");
                // Don't throw - allow the application to continue even if seeding fails
            }
        }

        try
        {
            logger.LogInformation("Applying database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");

            // Seed equipment data if it doesn't exist
            await SeedEquipmentDataAsync(db, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while applying database migrations");
            throw;
        }
    }
    Log.Information("Equipment Lending API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}