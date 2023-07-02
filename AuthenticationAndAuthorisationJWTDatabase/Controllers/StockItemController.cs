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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase.Controllers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using devMobile.Azure.DapperTransient;

    /// <summary>
    /// WebAPI controller with functionality for managing StockItems.
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
        public StockItemController(IConfiguration configuration, ILogger<StockItemController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
            this.logger = logger;
        }

        [Authorize(Roles = "SalesPerson,SalesAdministrator,Administrator")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.StockItemGetDtoV1))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<Models.StockItemGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
        {
            Models.StockItemGetDtoV1 response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QuerySingleOrDefaultWithRetryAsync<Models.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV2]", param: new { UserId = HttpContext.PersonId(), StockItemId = id }, commandType: CommandType.StoredProcedure);
            }

            if (response == default)
            {
                logger.LogInformation("StockItem:{0} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            return this.Ok(response);
        }

        [Authorize(Roles = "SalesPerson,SalesAdministrator,Administrator")]
        [HttpGet("Search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Models.StockItemListDtoV1>))]
        public async Task<ActionResult<IEnumerable<Models.StockItemListDtoV1>>> Get([FromQuery] Models.StockItemNameSearchDtoV1 request)
        {
            IEnumerable<Models.StockItemListDtoV1> response;

            request.UserId = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV2]", param: request, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("StockItem search with {0} nothing found", request.SearchText);
            }

            return this.Ok(response);
        }
    }
}
