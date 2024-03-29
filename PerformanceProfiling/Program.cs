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
// https://localhost:7054/profiler/results-index
//---------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using devMobile.Dapper;


namespace devMobile.WebAPIDapper.PerformanceProfiling
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IDapperContext>(s => new DapperContext(builder.Configuration));

            builder.Services.AddControllers();

            builder.Services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                //options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                options.TrackConnectionOpenClose = true;
                //options.PopupShowTrivial = false
            });

            var app = builder.Build();

            app.UseMiniProfiler();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}