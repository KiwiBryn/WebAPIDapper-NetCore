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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;

using devMobile.Azure.DapperTransient;
using devMobile.Dapper;


namespace devMobile.WebAPIDapper.CachingWithRedisExtensions.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private const int StockItemSearchMaximumRowsToReturn = 15;
        private readonly TimeSpan StockItemListAbsoluteExpiration = new TimeSpan(23, 59, 59);
        private readonly TimeSpan StockItemsSearchSlidingExpiration = new TimeSpan(0, 1, 0);

        private const string StackItemsListCompositeKey = "StockItems-Ex";
        private const string StackItemsListIdCompositeKey = "StockItem-Ex:{0}";
        private const string StackItemsListSearchCompositeKey = "StockItems-ExSearch:{0}-{1}";
        private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]";
        //private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";

        private readonly ILogger<StockItemsController> logger;
        private readonly IDbConnection dbConnection;
        private readonly IRedisDatabase redisClient;

        public StockItemsController(ILogger<StockItemsController> logger, IDapperContext dapperContext, IRedisClient redisClient)
        {
            this.logger = logger;

            this.dbConnection = dapperContext.ConnectionCreate();

            this.redisClient = redisClient.GetDefaultDatabase();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            DateTime requestAtUtc = DateTime.UtcNow;

            var stockItems = await redisClient.GetAsync<IEnumerable<Model.StockItemListDtoV1>>(StackItemsListCompositeKey);
            if (stockItems is not null)
            {
                return this.Ok(stockItems);
            }

            stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);

            await redisClient.AddAsync(StackItemsListCompositeKey, stockItems, requestAtUtc.Add(StockItemListAbsoluteExpiration));

            return this.Ok(stockItems);
        }

        [HttpGet("NoLoad")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetNoLoad()
        {
            var stockItems = await redisClient.GetAsync<IEnumerable<Model.StockItemListDtoV1>>(StackItemsListCompositeKey);
            if (stockItems is null)
            {
                return this.NoContent();
            }

            return this.Ok(stockItems);
        }

        [HttpPost("Load")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> PostLoad()
        {
            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);

            await redisClient.AddAsync(StackItemsListCompositeKey, stockItems);

            return this.Ok();
        }

        [HttpDelete()]
        public async Task<ActionResult> ListCacheDelete()
        {
            await redisClient.RemoveAsync(StackItemsListCompositeKey);

            logger.LogInformation("StockItems list removed");

            return this.Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
        {
            string compositeKey = string.Format(StackItemsListIdCompositeKey, id);

            var cached = await redisClient.GetAsync<Model.StockItemGetDtoV1>(compositeKey, StockItemsSearchSlidingExpiration);
            if (cached is not null)
            {
                return this.Ok(cached);
            }

            var stockItem = await dbConnection.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
            if (stockItem == default)
            {
                logger.LogInformation("StockItem:{id} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            await redisClient.AddAsync<Model.StockItemGetDtoV1>(compositeKey, stockItem, StockItemsSearchSlidingExpiration);

            return this.Ok(stockItem);
        }

        [HttpDelete("{id}")]
        public ActionResult ItemCacheDelete(int id)
        {
            string compositeKey = string.Format(StackItemsListIdCompositeKey, id);

            redisClient.RemoveAsync(compositeKey);

            logger.LogInformation("StockItem:{id} removed", id);

            return this.NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get([Required][MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")] string searchText, [Range(1, StockItemSearchMaximumRowsToReturn, ErrorMessage = "The maximum number of rows to return must be between {1} and {2}")] int maximumRowsToReturn = StockItemSearchMaximumRowsToReturn)
        {
            string compositeKey = string.Format(StackItemsListSearchCompositeKey, maximumRowsToReturn, searchText.ToLower());

            var cached = await redisClient.GetAsync<IList<Model.StockItemListDtoV1>>(compositeKey);
            if (cached is not null)
            {
                return this.Ok(cached);
            }

            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: new { searchText, maximumRowsToReturn }, commandType: CommandType.StoredProcedure);

            await redisClient.AddAsync(compositeKey, stockItems);

            return this.Ok(stockItems);
        }
    }
}

