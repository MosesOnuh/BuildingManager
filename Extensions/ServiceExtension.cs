using Amazon.S3;
using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Repository;
using BuildingManager.Services;
using BuildingManager.Utils.Logger;
using BuildingManager.Utils.StorageManager;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using BuildingManager.Validators;
using BuildingManager.Middlewares;

namespace BuildingManager.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureLoggerService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureStorageService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IStorageManager, StorageManager>();
        }

        public static void ConfigureModelValidation(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvc().ConfigureApiBehaviorOptions(o =>
            {
                o.InvalidModelStateResponseFactory = context => new ValidationFailedResult(context.ModelState);
            });
            serviceCollection.AddFluentValidationAutoValidation();
            serviceCollection.AddFluentValidationClientsideAdapters();
            serviceCollection.AddValidatorsFromAssemblyContaining<UserValidator>();
            //serviceCollection.AddValidatorsFromAssemblyContaining<LogInUserValidator>();
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration) 
        {
            
            var jwtSettings = configuration.GetSection("JWT");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.GetSection("Issuer").Value,
                    ValidAudience = jwtSettings.GetSection("Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetSection("Secret").Value))
                };
            });
        }

        public static void ConfigureServiceManager(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddScoped<IServiceManager, ServiceManager>();
            serviceCollection.AddSingleton<IServiceManager, ServiceManager>();
        }

        public static void ConfigureRepositoryManager(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddScoped<IRepositoryManager, RepositoryManager>();
            serviceCollection.AddSingleton<IRepositoryManager, RepositoryManager>();
        }


    }
}
