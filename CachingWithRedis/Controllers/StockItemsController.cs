//---------------------------------------------------------------------------------
// Copyright (c) September 2023, devMobile Software
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
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using devMobile.Azure.DapperTransient;
using devMobile.Dapper;

using StackExchange.Redis;


namespace devMobile.WebAPIDapper.CachingWithRedis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private const int StockItemSearchMaximumRowsToReturn = 15;
        private readonly TimeSpan StockItemListExpiration = new TimeSpan(0, 5, 0);

        private const string StackItemsListCompositeKey = "StockItems";
        private const string StackItemsListIdCompositeKey = "StockItem:{0}";

        private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]";
        //private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";

        private readonly ILogger<StockItemsController> logger;
        private readonly IDbConnection dbConnection;
        private readonly IDatabase redisCache;

        public StockItemsController(ILogger<StockItemsController> logger, IDapperContext dapperContext, IConnectionMultiplexer connectionMultiplexer)
        {
            this.logger = logger;

            this.dbConnection = dapperContext.ConnectionCreate();

            this.redisCache = connectionMultiplexer.GetDatabase();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            var cached = await redisCache.StringGetAsync(StackItemsListCompositeKey);
            if (cached.HasValue)
            {
                return Content(cached, "application/json");
            }

            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);

#if SERIALISER_SOURCE_GENERATION
            string json = JsonSerializer.Serialize(stockItems, typeof(List<Model.StockItemListDtoV1>), Model.StockItemListDtoV1GenerationContext.Default);
#else
            string json = JsonSerializer.Serialize(stockItems);
#endif

            await redisCache.StringSetAsync(StackItemsListCompositeKey, json, expiry: StockItemListExpiration);

            return Content(json, "application/json");
        }

        [HttpGet("NoLoad")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetNoLoad()
        {
            var cached = await redisCache.StringGetAsync(StackItemsListCompositeKey);
            if (!cached.HasValue)
            {
                return this.NoContent();
            }

            return this.Ok(cached);
        }

        [HttpPost("Load")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> PostLoad()
        {
            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);

#if SERIALISER_SOURCE_GENERATION
            string json = JsonSerializer.Serialize(stockItems, typeof(List<Model.StockItemListDtoV1>), Model.StockItemListDtoV1GenerationContext.Default);
#else
            string json = JsonSerializer.Serialize(stockItems);
#endif

            return this.Ok();
        }

        [HttpDelete()]
        public async Task<ActionResult> ListCacheDelete()
        {
            await redisCache.KeyDeleteAsync(StackItemsListCompositeKey);

            logger.LogInformation("StockItems list removed");

            return this.Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
        {
            string compositeKey = string.Format(StackItemsListIdCompositeKey, id);

            var cached = await redisCache.StringGetAsync(compositeKey);
            if (cached.HasValue)
            {
                return this.Ok(JsonSerializer.Deserialize<Model.StockItemGetDtoV1>(cached));
            }

            var stockItem = await dbConnection.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
            if (stockItem == default)
            {
                logger.LogInformation("StockItem:{id} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            await redisCache.StringSetAsync(compositeKey, JsonSerializer.Serialize<Model.StockItemGetDtoV1>(stockItem));

            return this.Ok(stockItem);
        }

        [HttpDelete("{id}")]
        public ActionResult ItemCacheDelete(int id)
        {
            string compositeKey = string.Format(StackItemsListIdCompositeKey, id);

            redisCache.KeyDelete(compositeKey);

            logger.LogInformation("StockItem:{id} removed", id);

            return this.NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get([Required][MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")] string searchText, [Range(1, StockItemSearchMaximumRowsToReturn, ErrorMessage = "The maximum number of rows to return must be between {1} and {2}")] int maximumRowsToReturn = StockItemSearchMaximumRowsToReturn)
        {
            var cached = await redisCache.StringGetAsync($"StockItemsSearch:{searchText.ToLower()}");
            if (cached.HasValue)
            {
                return this.Ok(JsonSerializer.Deserialize<List<Model.StockItemListDtoV1>>(cached));
            }

            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: new { searchText, maximumRowsToReturn }, commandType: CommandType.StoredProcedure);

            await redisCache.StringSetAsync($"StockItemsSearch:{searchText.ToLower()}", JsonSerializer.Serialize(stockItems));

            return this.Ok(stockItems);
        }
    }
}