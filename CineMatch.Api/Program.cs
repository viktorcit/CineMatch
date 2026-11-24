using CineMatch.Api.Configuration;
using CineMatch.Api.Data;
using CineMatch.Api.Entity;
using CineMatch.Api.Midlleware;
using CineMatch.Api.Services.Interfaces.IJwtServices;
using CineMatch.Api.Services.Interfaces.IMovieServices;
using CineMatch.Api.Services.Interfaces.ISessionServices;
using CineMatch.Api.Services.Interfaces.IUserServices;
using CineMatch.Api.Services.JwtServices;
using CineMatch.Api.Services.MovieServices;
using CineMatch.Api.Services.SessionServices;
using CineMatch.Api.Services.UserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddScoped<IMovieSearchService, MovieSearchService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<ISessionMovieService, SessionMovieService>();

builder.Services.AddSingleton<JwtSecurityTokenHandler>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
              .WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddHttpClient<ITmdbService, TmdbService>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");

    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.UseMiddleware<ExeptionHandlingMidlleware>();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

using var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
try
{
    await seeder.SeedAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application startup failed during database seeding");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() => logger.LogInformation("CineMatch API started in {Environment}", app.Environment.EnvironmentName));
app.Lifetime.ApplicationStopping.Register(() => logger.LogInformation("CineMatch API is stopping"));
app.Run();
