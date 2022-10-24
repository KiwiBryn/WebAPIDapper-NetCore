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
            var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = fileVersionInfo.ProductName,
                        Version = $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}",
                        Description = fileVersionInfo.Comments,

                        License = new OpenApiLicense
                        {
                            Name = fileVersionInfo.LegalCopyright,
                            //Url = new Uri(""),
                        },
                        //TermsOfService = new Uri(""),

                        Contact = new OpenApiContact
                        {
                            Name = fileVersionInfo.CompanyName,
                            //Url = new Uri(""),
                        }
                    });
                c.OperationFilter<AddResponseHeadersFilter>();
                c.IncludeXmlComments(string.Format(@"{0}\WebAPIDapper.xml", System.AppDomain.CurrentDomain.BaseDirectory));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            builder.Services.AddApplicationInsightsTelemetry();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}