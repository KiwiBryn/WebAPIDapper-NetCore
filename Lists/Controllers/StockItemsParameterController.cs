//---------------------------------------------------------------------------------
// Copyright (c) August 2022, devMobile Software
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

using Dapper;
using devMobile.Azure.DapperTransient;
using System.ComponentModel.DataAnnotations;

namespace devMobile.WebAPIDapper.Lists.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsParameterController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<StockItemsParameterController> logger;

        public StockItemsParameterController(IConfiguration configuration, ILogger<StockItemsParameterController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

            this.logger = logger;
        }

        [HttpGet("Dynamic")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetDynamic([FromQuery] Model.StockItemNameSearchDtoV1 request)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("MaximumRowsToReturn", request.MaximumRowsToReturn);
                parameters.Add("SearchText", request.SearchText);

                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            return this.Ok(response);
        }

        [HttpGet("Automagic")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetMapping([FromQuery] Model.StockItemNameSearchDtoV1 request)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            return this.Ok(response);
        }

        [HttpGet("Anonymous")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetAnonymous([FromQuery] Model.StockItemNameSearchDtoV1 request)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", new { MaximumRowsToReturn = request.MaximumRowsToReturn, SearchText = request.SearchText }, commandType: CommandType.StoredProcedure);
            }

            return this.Ok(response);
        }

        [HttpGet("Array")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetArray([FromQuery][Required(), MinLength(1)]int[] stockItemID)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems] WHERE  StockItemID IN @StockItemIds ", new { StockItemIDs = stockItemID }, commandType: CommandType.Text);
            }

            return this.Ok(response);
        }
    }
}

