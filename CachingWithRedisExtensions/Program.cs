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
//---------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.MsgPack;

using devMobile.Dapper;


namespace devMobile.WebAPIDapper.CachingWithRedisExtensions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IDapperContext>(s => new DapperContext(builder.Configuration));


            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddSingleton<IRedisClient, RedisClient>();
            builder.Services.AddSingleton<IRedisConnectionPoolManager, RedisConnectionPoolManager>();
            builder.Services.AddSingleton<ISerializer, MsgPackObjectSerializer>();

            var redisConfiguration = builder.Configuration.GetSection("Redis").Get<RedisConfiguration>();

            builder.Services.AddSingleton(redisConfiguration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}