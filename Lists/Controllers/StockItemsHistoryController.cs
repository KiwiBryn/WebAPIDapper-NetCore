//---------------------------------------------------------------------------------
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
using System.Collections.Generic;
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
	[ApiController]
	[Route("api/[controller]")]
	public class StockItemsHistoryController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsHistoryController> logger;

		public StockItemsHistoryController(IConfiguration configuration, ILogger<StockItemsHistoryController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery]DateTime? asAt)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			if (!asAt.HasValue)
			{
				asAt = DateTime.UtcNow;
			}

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: "[warehouse].[StockItemsHistorySockItemsListAsAtV1]", param: new { asAt= asAt }, commandType: CommandType.StoredProcedure);
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsAsync exception retrieving list of StockItems");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}


		[HttpGet("{id}")]
		public async Task<ActionResult<Model.StockItemGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id, [FromQuery] DateTime? asAt)
		{
			Model.StockItemGetDtoV1 response = null;

			if ( !asAt.HasValue)
			{
				asAt = DateTime.UtcNow; 
			}

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QuerySingleOrDefaultAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsHistoryStockItemLookupAsAtV1]", param: new { asAt=asAt, stockItemID=id }, commandType: CommandType.StoredProcedure);
					if (response == default)
					{
						return this.NotFound($"StockItemsHistory StockItemId:{id} not found or no");
					}
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsLookup exception looking up a StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}


		[HttpGet("{id}/history")]
		public async Task<ActionResult<IEnumerable<Model.StockItemHistoryListDtoV1>>> GetHistory([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			IEnumerable<Model.StockItemHistoryListDtoV1> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QueryAsync<Model.StockItemHistoryListDtoV1>(sql: "[Warehouse].[StockItemsHistoryStockItemHistoryListV1]", param: new { StockItemID = id }, commandType: CommandType.StoredProcedure);
					if (response == default)
					{
						return this.NotFound($"StockItemId:{id} not found");
					}
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsLookup exception looking up a StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}

