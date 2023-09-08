//---------------------------------------------------------------------------------
// Copyright (c) September 2023, devMobile Software
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
// https://localhost:7054/profiler/results-index
//---------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using devMobile.Dapper;


namespace devMobile.WebAPIDapper.CachingWithRedis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();

            // Add services to the container.
            builder.Services.AddSingleton<IDapperContext>(s => new DapperContext(builder.Configuration));

            builder.Services.AddControllers();

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { builder.Configuration.GetSection("RedisConnection").GetValue<string>("EndPoints") },
                AllowAdmin = true,
                Password = builder.Configuration.GetSection("RedisConnection").GetValue<string>("Password"),
                Ssl = true,
                ConnectRetry = 5,
                ConnectTimeout = 10000,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                AbortOnConnectFail = false,
            };

            builder.Services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configurationOptions));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
