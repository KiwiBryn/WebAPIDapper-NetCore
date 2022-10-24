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
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.Filters;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            // Extract application info for Swagger docs from assmebly info
            var version = Assembly.GetEntryAssembly().GetName().Version;

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = ".NET Core web API + Dapper + Swagger",
                        Version = $"{version.Major}.{version.Minor}",

                        Description = "This sample application shows how .NET Core and Dapper can be used to build a lightweight OPENAPI described Web API",
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
                c.OperationFilter<AddResponseHeadersFilter>();
                c.IncludeXmlComments(string.Format(@"{0}\WebAPIDapper.xml", System.AppDomain.CurrentDomain.BaseDirectory));
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
                    c.InjectStylesheet("/css/.css");
                    c.InjectJavascript("/JavaScript/Swagger.js");
                    c.DocumentTitle = "Web API Dapper Sample";
                });
            }

            builder.Services.AddApplicationInsightsTelemetry();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}