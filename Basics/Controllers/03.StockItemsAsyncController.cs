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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Dapper;


namespace devMobile.WebAPIDapper.Basics.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StockItemsAsyncController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsAsyncController> logger;

		public StockItemsAsyncController(IConfiguration configuration, ILogger<StockItemsAsyncController> logger)
		{
			this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

			this.logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get()
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving list of StockItems");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}

