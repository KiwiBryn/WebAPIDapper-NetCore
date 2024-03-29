//---------------------------------------------------------------------------------
// Copyright (c) June 2023, devMobile Software
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
#if DISTRIBUTED_CACHE_REDIS || DISTRIBUTED_CACHE_SQL_SERVER
    using Microsoft.Extensions.Configuration;
#endif
using Microsoft.Extensions.DependencyInjection;
using MessagePack;
#if DISTRIBUTED_CACHE_REDIS
    using StackExchange.Redis;
#endif

using devMobile.Dapper;

namespace devMobile.WebAPIDapper.CachingWithDistributedCache
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

#if SERIALISATION_MESSAGE_PACK
            //MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options;
            //MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block);
            MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray);
#endif

#if DISTRIBUTED_CACHE_MEMORY
            //builder.Services.AddDistributedMemoryCache(options =>
            //{
            //    options.SizeLimit = 1000 * 1024 * 1024; // 1000MB
            //});
            builder.Services.AddDistributedMemoryCache();
#endif

#if DISTRIBUTED_CACHE_REDIS
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

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "Dapper WebAPI Instance";
                options.ConfigurationOptions = configurationOptions;
            });
#endif

#if DISTRIBUTED_CACHE_SQL_SERVER
            builder.Services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = builder.Configuration.GetConnectionString("CacheDatabase");
                options.SchemaName = "dbo";
                options.TableName = "StockItemsCache";
            });
#endif

            var app = builder.Build();


            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }      
    }
}