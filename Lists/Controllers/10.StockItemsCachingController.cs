//---------------------------------------------------------------------------------
// Copyright (c) Feb 2022, devMobile Software
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
using Microsoft.Extensions.Logging;

using Dapper.Extensions;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StockItemsCachingController : ControllerBase
	{
		private const int StockItemsListResponseCacheDuration = 30;

		private readonly ILogger<StockItemsCachingController> logger;
		private readonly IDapper dapper;

		public StockItemsCachingController(ILogger<StockItemsCachingController> logger, IDapper dapper)
		{
			this.logger = logger;

			this.dapper = dapper;
		}

		[HttpGet("Response")]
		[ResponseCache(Duration = StockItemsListResponseCacheDuration)]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetResponse()
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			logger.LogInformation("Response cache load");

			try
			{
				response = await dapper.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving list of StockItems");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}

		[HttpGet("ResponseVarying")]
		[ResponseCache(Duration = StockItemsListResponseCacheDuration, VaryByQueryKeys = new string[] { "id" })]
		public async Task<ActionResult<Model.StockItemGetDtoV1>> Get([FromQuery(Name = "id"), Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			Model.StockItemGetDtoV1 response = null;

			logger.LogInformation("Response cache varying load id:{0}", id);

			try
			{
				response = await dapper.QuerySingleOrDefaultAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
				if (response == default)
				{
					logger.LogInformation("StockItem:{0} not found", id);

					return this.NotFound($"StockItem:{id} not found");
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Looking up StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}

#if DAPPER_EXTENSIONS_CACHE_MEMORY
		[HttpGet("DapperMemory")]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetDapper()
		{
			List<Model.StockItemListDtoV1> response;

			logger.LogInformation("Dapper memory cache load");

			try
			{
				response = await dapper.QueryAsync<Model.StockItemListDtoV1>(
							sql: "[Warehouse].[StockItemsStockItemLookupV1]",
							commandType: CommandType.StoredProcedure,
							enableCache: true,
							cacheExpire: TimeSpan.FromSeconds(StockItemsListResponseCacheDuration));
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving list of StockItems");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}

		[HttpGet("DapperMemoryVarying")]
		public async Task<ActionResult<Model.StockItemGetDtoV1>> GetDapperVarying([FromQuery(Name = "id"), Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			Model.StockItemGetDtoV1 response = null;

			logger.LogInformation("Dapper memory cache load id:{0}", id);

			try
			{
				response = await dapper.QuerySingleOrDefaultAsync<Model.StockItemGetDtoV1>(
							sql: "[Warehouse].[StockItemsStockItemLookupV1]",
							param: new { stockItemId = id },
							commandType: CommandType.StoredProcedure,
							cacheKey: $"StockItem:{id}",
							enableCache: true,
							cacheExpire: TimeSpan.FromSeconds(StockItemsListResponseCacheDuration)
							);
				if (response == default)
				{
					logger.LogInformation("StockItem:{0} not found", id);

					return this.NotFound($"StockItem:{id} not found");
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Looking up StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
#endif
	}
}

