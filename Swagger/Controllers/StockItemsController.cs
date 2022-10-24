//---------------------------------------------------------------------------------
// Copyright (c) October 2022, devMobile Software
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
namespace devMobile.WebAPIDapper.Swagger.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Data.SqlClient;

    using Microsoft.AspNetCore.Mvc;

    using devMobile.Azure.DapperTransient;

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

        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
        {
            Model.StockItemGetDtoV1 response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
            }

            if (response == default)
            {
                logger.LogInformation("StockItem:{0} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            return this.Ok(response);
        }

        [HttpGet("Search")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery] Model.StockItemNameSearchDtoV1 request)
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: request, commandType: CommandType.StoredProcedure);

                if (!response.Any())
                {
                    logger.LogInformation("StockItem search with {0} nothing found", request.SearchText);
                }
            }

            return this.Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Delivered([FromQuery] Model.StockItemNameSearchDtoV1 request)
        {
            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsV1]", param: request, commandType: CommandType.StoredProcedure);

                if (!response.Any())
                {
                    logger.LogInformation("StockItem search with {0} nothing found", request.SearchText);
                }
                )
            }

            return this.Ok();
        }
    }
}
