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
using System.Data;
using System.Data.SqlClient;

using Microsoft.AspNetCore.Mvc;

using devMobile.Azure.DapperTransient;


namespace devMobile.WebAPIDapper.ListsDIBasic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private readonly string connectionString;
		private readonly ILogger<StockItemsController> logger;

		public StockItemsController(IConfiguration configuration, ILogger<StockItemsController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

			this.logger = logger;
		}

        [HttpGet]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (SqlConnection db = new(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
            }

            return this.Ok(response);
        }


		[HttpGet("{id}")]
		public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
		{
			Model.StockItemGetDtoV1 response ;

			using (SqlConnection db = new(this.connectionString))
			{
			   response = await db.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
			}

			if (response == default)
			{
				logger.LogInformation("StockItem:{id} not found", id);

				return this.NotFound($"StockItem:{id} not found");
			}

			return this.Ok(response);
		}

		[HttpGet("search")]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery] Model.StockItemNameSearchDtoV1 request)
		{
			IEnumerable<Model.StockItemListDtoV1> response;

			using (SqlConnection db = new(this.connectionString))
			{
				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: request, commandType: CommandType.StoredProcedure);
			}

			return this.Ok(response);
		}
	}
}

