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
namespace devMobile.WebAPIDapper.Swagger
{
    using System;
    using System.IO;
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.Filters;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddSwaggerGenNewtonsoftSupport();

            //builder.Services.Configure<Model.JwtIssuerOptions>(builder.Configuration.GetSection(nameof(Model.JwtIssuerOptions)));

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
            });

            var app = builder.Build();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "css")),
                RequestPath = "/css",
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images")),
                RequestPath = "/images",
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "JavaScript")),
                RequestPath = "/JavaScript",
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    //c.EnableFilter();
                    c.InjectStylesheet("/css/Swagger.css");
                    c.InjectJavascript("/JavaScript/Swagger.js");
                    c.DocumentTitle = "Web API Dapper Sample";
                });
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}