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
// https://localhost:7077/api/stockitems
//---------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using devMobile.Dapper;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Redis.OM;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;

namespace devMobile.WebAPIDapper.CachingWithRedisOM
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

            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"));

            builder.Services.AddSingleton(new RedisConnectionProvider(connectionMultiplexer));

            //builder.Services.AddSingleton(new RedisConnectionProvider(builder.Configuration.GetConnectionString("Redis")));

            //builder.Services.AddHostedService<IndexCreationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }

    public class IndexCreationService : IHostedService
    {
        private readonly RedisConnectionProvider _provider;

        public IndexCreationService(RedisConnectionProvider provider)
        {
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var info = (await _provider.Connection.ExecuteAsync("FT._LIST")).ToArray().Select(x => x.ToString());
            if (info.All(x => x != "stockitem-idx"))
            {
                await _provider.Connection.CreateIndexAsync(typeof(Model.StockItem));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
