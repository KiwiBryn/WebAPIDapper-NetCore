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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using devMobile.Azure.DapperTransient;

    /// <summary>
    /// WebAPI controller for handling StockItem functionality.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<StockItemController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StockItemController"/> class.
        /// </summary>
        /// <param name="configuration">DI configuration provider.</param>
        /// <param name="logger">DI logging provider.</param>/// 
        public StockItemController(IConfiguration configuration, ILogger<StockItemController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
            this.logger = logger;
        }

        /// <summary>
        /// Gets a summary of the specified invoice plus associated invoice lines and stock item transactions.
        /// </summary>
        /// <param name="id">Numeric ID used for referencing a stockitem within the database.</param>
        /// <response code="200">StockItem information returned.</response>
        /// <response code="404">StockItem ID not found.</response>
        /// <returns>Invoice information with associated invoice lines and item transaction.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Swagger.Models.StockItemGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
        {
            Swagger.Models.StockItemGetDtoV1 response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QuerySingleOrDefaultWithRetryAsync<Swagger.Models.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
            }

            if (response == default)
            {
                logger.LogInformation("StockItem:{0} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            return this.Ok(response);
        }

        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Swagger.Models.StockItemListDtoV1>>> Get([FromQuery] Swagger.Models.StockItemNameSearchDtoV1 request)
        {
            IEnumerable<Swagger.Models.StockItemListDtoV1> response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Swagger.Models.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: request, commandType: CommandType.StoredProcedure);

                if (!response.Any())
                {
                    logger.LogInformation("StockItem search with {0} nothing found", request.SearchText);
                }
            }

            return this.Ok(response);
        }
    }
}
