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
//
//---------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Dapper;
using devMobile.Azure.DapperTransient;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsParameterController : ControllerBase
    {
        private readonly string connectionString;

        public StockItemsParameterController(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
        }

        //
        // https://localhost:5001/api/StockItemsParameter/dynamic?SearchText=USB&maximumRowsToReturn=5
        //
        [HttpGet("Dynamic")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetDynamic(
                    [Required][MinLength(3, ErrorMessage = "The name search text must be at least {1} characters long"), MaxLength(20, ErrorMessage = "The name search text must be no more that {1} characters long")] string searchText,
                    [Required][Range(1, int.MaxValue, ErrorMessage = "MaximumRowsToReturn must be greater than or equal to {1}")] int maximumRowsToReturn)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("MaximumRowsToReturn", maximumRowsToReturn);
                parameters.Add("SearchText", searchText);

                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: parameters, commandType: CommandType.StoredProcedure);
            }

            return this.Ok(response);
        }

        //
        // https://localhost:5001/api/StockItemsParameter/Anonymous?SearchText=USB&maximumRowsToReturn=5
        //
        [HttpGet("Anonymous")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetAnonymous(
                    [Required][MinLength(3, ErrorMessage = "The name search text must be at least {1} characters long"), MaxLength(20, ErrorMessage = "The name search text must be no more that {1} characters long")] string searchText,
                    [Required][Range(1, int.MaxValue, ErrorMessage = "MaximumRowsToReturn must be at least {1}")] int maximumRowsToReturn)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", new { maximumRowsToReturn, searchText }, commandType: CommandType.StoredProcedure);
            }

            return this.Ok(response);
        }

        //
        // https://localhost:5001/api/StockItemsParameter/Automagic?SearchText=USB&maximumRowsToReturn=5
        //
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

        //
        // https://localhost:5001/api/StockItemsParameter/Array?StockItemId=1&StockItemId=5&StockItemId=10
        //
        [HttpGet("Array")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetArray(
            [FromQuery(Name = "stockItemID")][Required(), MinLength(1, ErrorMessage = "Minimum of {1} StockItem ids"), MaxLength(100, ErrorMessage = "Maximum {1} StockItem ids")] int[] stockItemIDs)
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems] WHERE  StockItemID IN @StockItemIds ", new { StockItemIDs = stockItemIDs }, commandType: CommandType.Text);
            }

            return this.Ok(response);
        }
    }
}

