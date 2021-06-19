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
	public class StockItemsLookupController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsLookupController> logger;

		public StockItemsLookupController(IConfiguration configuration, ILogger<StockItemsLookupController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Model.StockItemGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			Model.StockItemGetDtoV1 response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QuerySingleOrDefaultAsync<Model.StockItemGetDtoV1>(sql: "StockItemsStockItemLookupV1", param: new { stockItemId=id }, commandType: CommandType.StoredProcedure);
				}

				if (response == default)
				{
					return this.NotFound($"StockItemId:{id} not found");
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsLookup exception looking up a StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}

		[HttpGet("{id}/stockgroups")]

		public async Task<ActionResult<IAsyncEnumerable<Model.StockGroupListDtoV1>>> GetStockGroups([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			IEnumerable<Model.StockGroupListDtoV1> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QueryAsync<Model.StockGroupListDtoV1>(sql: "StockItemsStockItemStockGroupListV1", param: new { stockItemId = id }, commandType: CommandType.Text);
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItems exception retrieving list of StockGroups for StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}

		[HttpGet]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery] Model.StockItemSearchDtoV1 request)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsSearchV1]", param: request, commandType: CommandType.Text);
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsSearch exception searching for list of StockItems with name like:{0}", request);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}

