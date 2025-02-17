using AirLineSystem.Api.Configurtations;
using AirLineSystem.Domain.Models;
using AirLineSystem.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp",
        builder =>
                 {
                     builder.WithOrigins("http://localhost:3000").WithHeaders(new string[]
                     { 
                        HeaderNames.ContentType,
                        HeaderNames.Authorization,
                     }).AllowAnyMethod();
                 });
    });

builder.Services.AddControllers().AddJsonOptions(options=> { 
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddDependencyInjectionConfiguration();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.Authority = "https://dev-5rbbag7mx6hlxlhq.eu.auth0.com/";
    options.Audience = "https://airline-booking-api.example.com";
});

var app = builder.Build();

app.UseCors("AllowReactApp");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
