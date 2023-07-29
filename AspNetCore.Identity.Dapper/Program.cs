//---------------------------------------------------------------------------------
// Copyright (c) July  2023, devMobile Software
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
using System;

using devMobile.AspNetCore.Identity.Dapper.Data;
using devMobile.AspNetCore.Identity.Dapper.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace devMobile.AspNetCore.Identity.Dapper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationInsightsTelemetry();

            var connectionString = builder.Configuration.GetConnectionString("AspNetCoreIdentityDapperContextConnection") ?? throw new InvalidOperationException("Connection string 'AspNetCoreIdentityDapperContextConnection' not found.");

            builder.Services.AddDbContext<AspNetCoreIdentityDapperContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
               .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<AspNetCoreIdentityDapperContext>();

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.Configure<Models.SendGridSettings>(builder.Configuration.GetSection("SendGrid"));

            builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); ;
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}