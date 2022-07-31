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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using devMobile.Azure.DapperTransient;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StockItemsIAsyncEnumerableController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsIAsyncEnumerableController> logger;

		public StockItemsIAsyncEnumerableController(IConfiguration configuration, ILogger<StockItemsIAsyncEnumerableController> logger)
		{
			this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

			this.logger = logger;
		}

		[HttpGet("IEnumerableSmall")]
		public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetIEnumerableSmall([FromQuery]bool buffered = false)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				logger.LogInformation("IEnumerableSmall start Buffered:{buffered} i", buffered);

				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(
					sql: @"SELECT [SI1].[StockItemID] as ""ID"", [SI1].[StockItemName] as ""Name"", [SI1].[RecommendedRetailPrice], [SI1].[TaxRate]" +
						   "FROM [Warehouse].[StockItems] as SI1",
					buffered,
					commandType: CommandType.Text);

				logger.LogInformation("IEnumerableSmall done");
			}

			return this.Ok(response);
		}

		[HttpGet("IEnumerableMedium")]
		public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetIEnumerableMedium([FromQuery] bool buffered = false)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				logger.LogInformation("IEnumerableMedium start Buffered:{buffered}", buffered);

				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(
					sql: @" SELECT [SI2].[StockItemID] as ""ID"", [SI2].[StockItemName] as ""Name"", [SI2].[RecommendedRetailPrice], [SI2].[TaxRate]" +
							"FROM [Warehouse].[StockItems] as SI1" +
							"	CROSS JOIN[Warehouse].[StockItems] as SI2",
					buffered,
					commandType: CommandType.Text);

				logger.LogInformation("IEnumerableMedium done");
			}

			return this.Ok(response);
		}

		[HttpGet("IEnumerableLarge")]
		public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetIEnumerableLarge([FromQuery] bool buffered = false)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				logger.LogInformation("IEnumerableLarge start Buffered:{buffered}", buffered);

				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(
					sql: @" SELECT [SI3].[StockItemID] as ""ID"", [SI3].[StockItemName] as ""Name"", [SI3].[RecommendedRetailPrice], [SI3].[TaxRate]" +
							"FROM [Warehouse].[StockItems] as SI1" +
							"	CROSS JOIN[Warehouse].[StockItems] as SI2" +  
							"	CROSS JOIN[Warehouse].[StockItems] as SI3",
					buffered,
					commandType: CommandType.Text);

				logger.LogInformation("IEnumerableLarge done");
			}

			return this.Ok(response);
		}

		[HttpGet("IAsyncEnumerableSmall")]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetIAsyncEnumerableSmall([FromQuery] bool buffered = false)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				logger.LogInformation("IAsyncEnumerableSmall start Buffered:{buffered}", buffered);

				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(
					sql: @"SELECT [SI1].[StockItemID] as ""ID"", [SI1].[StockItemName] as ""Name"", [SI1].[RecommendedRetailPrice], [SI1].[TaxRate]" +
							"FROM [Warehouse].[StockItems] as SI1",
					buffered,
					commandType: CommandType.Text);

				logger.LogInformation("IAsyncEnumerableSmall done");
			}

			return this.Ok(response);
		}

		[HttpGet("IAsyncEnumerableMedium")]
		public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetIAsyncEnumerableMedium([FromQuery] bool buffered = false)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				logger.LogInformation("IAsyncEnumerableMedium start Buffered:{buffered}", buffered);

				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(
					sql: @"SELECT [SI2].[StockItemID] as ""ID"", [SI2].[StockItemName] as ""Name"", [SI2].[RecommendedRetailPrice], [SI2].[TaxRate]" +
							"FROM [Warehouse].[StockItems] as SI1" +
							"	CROSS JOIN[Warehouse].[StockItems] as SI2",
					buffered,
					commandType: CommandType.Text);

				logger.LogInformation("IAsyncEnumerableMedium done");
			}

			return this.Ok(response);
		}

		[HttpGet("IAsyncEnumerableLarge")]
		public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetAsyncEnumerableLarge([FromQuery] bool buffered = false)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				logger.LogInformation("IAsyncEnumerableLarge start Buffered:{buffered}", buffered);

				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(
					sql: @"SELECT [SI3].[StockItemID] as ""ID"", [SI3].[StockItemName] as ""Name"", [SI3].[RecommendedRetailPrice], [SI3].[TaxRate]" +
							"FROM [Warehouse].[StockItems] as SI1" +
							"   CROSS JOIN[Warehouse].[StockItems] as SI2" +
							"	CROSS JOIN[Warehouse].[StockItems] as SI3", 
				buffered,
				commandType: CommandType.Text);

				logger.LogInformation("IAsyncEnumerableLarge done");
			}

			return this.Ok(response);
		}
	}
}



