using BuildingManager.ActionFilter;
using BuildingManager.Extensions;
using BuildingManager.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.ConfigureCors();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureStorageService();
// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();

builder.Services.AddCors();
//builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.ConfigureModelValidation();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddControllers(options => options.Filters
  .Add(typeof(ValidationFilter)));
//builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:5173"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseErrorHandler();
app.UseAuthenticationHandler();
app.MapControllers();
app.MapHub<BuildingManager.Services.ChatService>("/hubs/chat");

app.Run();
