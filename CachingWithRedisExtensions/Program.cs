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
#if SERIALISER_MESSAGE_PACK
    using StackExchange.Redis.Extensions.MsgPack;
#endif
#if SERIALISE_NEWTONSOFT
    using StackExchange.Redis.Extensions.Newtonsoft;
#endif

using devMobile.Dapper;

#if SERIALISER_MESSAGE_PACK && SERIALISE_NEWTONSOFT
    #error Only one serialiser can be defined
#endif

#if !SERIALISER_MESSAGE_PACK && !SERIALISE_NEWTONSOFT
    #error At least one serialiser must be defined
#endif


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
#if SERIALISER_MESSAGE_PACK
    builder.Services.AddSingleton<ISerializer, MsgPackObjectSerializer>();
#endif
#if SERIALISE_NEWTONSOFT
    builder.Services.AddSingleton<ISerializer, NewtonsoftSerializer>();
#endif

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