﻿//---------------------------------------------------------------------------------
// Copyright (c) June 2021, devMobile Software
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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Dapper;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StockItemsImageController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsImageController> logger;

		public StockItemsImageController(IConfiguration configuration, ILogger<StockItemsImageController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		[HttpGet("{id}/image")]
		public async Task<ActionResult> GetImage([Range(1, int.MaxValue, ErrorMessage = "StockItem id must greater than 0")] int id)
		{
			Byte[] response;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.ExecuteScalarAsync<byte[]>(sql: @"SELECT [Photo] as ""photo"" FROM [WareHouse].[StockItems] WHERE StockItemID=@StockItemId", param: new { StockItemId = id }, commandType: CommandType.Text);
				}

				if (response == default)
				{
					logger.LogInformation("StockItem:{0} image not found", id);

					return this.NotFound($"StockItem:{id} image not found");
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Looking up a StockItem:{0} image", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return File(response, "image/jpeg");
		}

		[HttpGet("{id}/base64")]
		public async Task<ActionResult> GetBase64([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			Byte[] response;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.ExecuteScalarAsync<byte[]>(sql: @"SELECT [Photo] as ""photo"" FROM [WareHouse].[StockItems] WHERE StockItemID=@StockItemId", param: new { StockItemId = id }, commandType: CommandType.Text);
				}

				if (response == default)
				{
					logger.LogInformation("StockItem:{0} Base64 not found", id);

					return this.NotFound($"StockItem:{id} image not found");
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Looking up a StockItem withID:{0} base64", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return Ok("data:image/jpeg;base64," + Convert.ToBase64String(response));
		}
	}
}