//---------------------------------------------------------------------------------
// Copyright (c) June 2022, devMobile Software
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
namespace devMobile.WebAPIDapper.Lists.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Dapper;
    
    
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsReadonlyReplicasController : ControllerBase
    {
        private readonly ILogger<StockItemsReadonlyReplicasController> logger;
        private readonly List<string> readonlyReplicasConnectionStrings;

        public StockItemsReadonlyReplicasController(ILogger<StockItemsReadonlyReplicasController> logger, IOptions<List<string>> readonlyReplicasServerConnectionStrings)
        {
            this.logger = logger;

            this.readonlyReplicasConnectionStrings = readonlyReplicasServerConnectionStrings.Value;
        }

        [HttpGet]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            if (readonlyReplicasConnectionStrings.Count == 0)
            {
                logger.LogError("No readonly replica server Connection strings configured");

                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }

            Random random = new Random(); // maybe this should be instantiated ever call, but "danger here by thy threading"

            string connectionString = readonlyReplicasConnectionStrings[random.Next(0, readonlyReplicasConnectionStrings.Count)];

            logger.LogTrace("Connection string {connectionString}", connectionString);

            using (SqlConnection db = new SqlConnection(connectionString))
            {
                response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
            }

            return this.Ok(response);
        }
    }
}

