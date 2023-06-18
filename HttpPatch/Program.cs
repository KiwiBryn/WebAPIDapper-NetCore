//---------------------------------------------------------------------------------
// Copyright (c) July 2022, devMobile Software
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
using devMobile.Azure.Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using devMobile.WebAPIDapper.HttpPatch.Model;

namespace devMobile.WebAPIDapper.HttpPatch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IDapperContext>(s => new DapperContext(builder.Configuration));

            builder.Services.AddAutoMapper(typeof(Program));

            // Auto Mapper Configurations
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);

            builder.Services.AddControllers().AddNewtonsoftJson();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                // Add as many of these lines as you need to map your objects
                CreateMap<StockItemGetDtoV1, StockItemPatchDtoV1>();
                CreateMap<StockItemPatchDtoV1, StockItemGetDtoV1>();
            }
        }
    }
}