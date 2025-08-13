using Api.Filters;
using Api.Security;
using Application.Abstractions;
using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddHttpContextAccessor();

// Application
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
// Also scan Application assembly for handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IUserContext).Assembly));

builder.Services.AddControllers(o => o.Filters.Add<GlobalExceptionFilter>())
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateActivityValidator>();

// Infrastructure
builder.Services.AddInfrastructure(config);

// UserContext (trust header from gateway; fallback to JWT)
builder.Services.AddScoped<IUserContext, UserContext>();

// Optional JWT (enable when testing direct)
var useJwt = config.GetValue("Auth:EnableJwt", false);
if (useJwt)
{
    //builder.Services.AddAuthentication("Bearer")
    //  .AddJwtBearer("Bearer", options =>
    //  {
    //      options.Authority = config["Auth:Authority"]; // e.g., https://localhost:5001
    //      options.RequireHttpsMetadata = false;
    //      options.Audience = config["Auth:Audience"]; // api-gateway
    //  });
    //builder.Services.AddAuthorization();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var injectMockHeader = config.GetValue("Auth:InjectMockHeader", builder.Environment.IsDevelopment());
var headerKey = config.GetValue<string>("Auth:TrustedHeader") ?? "X-User-Id";
var mockUserId = config.GetValue<string>("Auth:MockUserId")
                 ?? "8a9defb6-e562-405b-9ac1-672e238bd20f";

var app = builder.Build();

if (injectMockHeader)
{
    app.Use(async (ctx, next) =>
    {
        if (!ctx.Request.Headers.TryGetValue(headerKey, out var _))
        {
            ctx.Request.Headers[headerKey] = mockUserId;
        }
        await next();
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (useJwt)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure collections & indexes
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<Infrastructure.Mongo.MongoContext>();
    await Infrastructure.Mongo.MongoIndexes.EnsureCollectionsAndIndexesAsync(ctx);
}

app.Run();