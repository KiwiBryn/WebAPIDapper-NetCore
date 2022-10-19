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
    using Microsoft.OpenApi.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        // Consider picking some of these up from Package info
                        Title = "devMobile WebAPIDapper",
                        Version = "v1.0",
                        Description = ".NET Core WebAPI sample with StackOverflow Dapper for data access",

                        License = new OpenApiLicense
                        {
                            Name = "APACHE LICENSE, VERSION 2.0",
                            Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0")
                        },
                        TermsOfService = new Uri("https://blog.devmobile.co.nz/about/"),

                        Contact = new OpenApiContact
                        {
                            Name = "devMobile Software",
                            Url = new Uri("https://blog.devmobile.co.nz")
                        }
                    });
                //c.OperationFilter<AddResponseHeadersFilter>();
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