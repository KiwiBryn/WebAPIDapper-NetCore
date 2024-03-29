//---------------------------------------------------------------------------------
// Copyright (c) June  2021, devMobile Software
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
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
#if ERROR_PAGE_STANDARD
	using Microsoft.Extensions.Hosting;
#endif
using Microsoft.Extensions.DependencyInjection;

using Dapper.Extensions.MSSQL;
#if DAPPER_EXTENSIONS_CACHE_MEMORY
	using Dapper.Extensions.Caching.Memory;
#endif
#if DAPPER_EXTENSIONS_CACHE_REDIS
	using Dapper.Extensions.Caching.Redis;
#endif

namespace devMobile.WebAPIDapper.Basics
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			var errorHandlerSettings = Configuration.GetSection(nameof(ErrorHandlerSettings));
			services.Configure<ErrorHandlerSettings>(errorHandlerSettings);

			var ReadonlyReplicaServersConnectionStringNamesSettings = Configuration.GetSection("ReadonlyReplicaServersConnectionStringNames");
			services.Configure<List<string>>(ReadonlyReplicaServersConnectionStringNamesSettings);

			services.AddResponseCaching();

			services.AddDapperForMSSQL();
#if DAPPER_EXTENSIONS_CACHE_MEMORY
			services.AddDapperCachingInMemory(new MemoryConfiguration
			{
				AllMethodsEnableCache = false
 			});
#endif
#if DAPPER_EXTENSIONS_CACHE_REDIS
			services.AddDapperCachingInRedis(new RedisConfiguration
			{
				AllMethodsEnableCache = false,
				KeyPrefix = Configuration.GetValue<string>("RedisKeyPrefix"),
				ConnectionString = Configuration.GetConnectionString("RedisConnection")
			}); 
#endif
			services.AddApplicationInsightsTelemetry();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
#if ERROR_PAGE_STANDARD
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
#endif
#if ERROR_PAGE_CUSTOM
			app.UseExceptionHandler("/Error");
#endif

			app.UseHttpsRedirection();

			app.UseResponseCaching();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
