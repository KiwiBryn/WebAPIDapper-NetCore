//---------------------------------------------------------------------------------
// Copyright (c) October 2022, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.Filters;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddMvc().AddNewtonsoftJson();
            builder.Services.AddSwaggerGenNewtonsoftSupport();

            builder.Services.Configure<Models.JwtIssuerOptions>(builder.Configuration.GetSection(nameof(Models.JwtIssuerOptions)));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(swagger =>
            {
                swagger.OperationFilter<AddResponseHeadersFilter>();
                swagger.IncludeXmlComments(string.Format(@"{0}\WebAPIDapper.xml", System.AppDomain.CurrentDomain.BaseDirectory));

                swagger.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = ".NET Core web API + Dapper + Swagger",
                        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                        Contact = new()
                        {
                            //Email = "", // Don't think this would be a good idea...
                            Name = "Bryn Lewis",
                            Url = new Uri("https://blog.devMobile.co.nz")
                        },
                        License = new()
                        {
                            Name = "Apache License V2.0",
                            Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0"),
                        }
                    });

                swagger.AddSecurityDefinition("Bearer", //Name the security scheme
                                     new OpenApiSecurityScheme
                                     {
                                         Description = "JWT Authorization header using the Bearer scheme.",
                                         Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                                         Scheme = "bearer", //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer"
                                         BearerFormat = "JWT",
                                     });

                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });

            Models.JwtIssuerOptions jwtIssuerOptions = builder.Configuration.GetSection(nameof(Models.JwtIssuerOptions)).Get<Models.JwtIssuerOptions>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuerOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtIssuerOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtIssuerOptions.SecretKey)),

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtIssuerOptions.Issuer;
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;

                configureOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    //c.EnableFilter();
                    c.DocumentTitle = "Web API Dapper Sample";
                });
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}