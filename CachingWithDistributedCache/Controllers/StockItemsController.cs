//---------------------------------------------------------------------------------
// Copyright (c) July 2023, devMobile Software
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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using MessagePack;

using devMobile.Azure.DapperTransient;
using devMobile.Dapper;


namespace devMobile.WebAPIDapper.CachingWithDistributedCache.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private const int StockItemSearchMaximumRowsToReturn = 15;
        private readonly TimeSpan StockItemGetByIdRelativeToNowExpiration = new TimeSpan(0, 5, 0);
        private readonly TimeSpan StockItemsSearchSlidingExpiration = new TimeSpan(0, 1, 0);

        private readonly ILogger<StockItemsController> logger;
        private readonly IDbConnection dbConnection;
        private readonly IDistributedCache distributedCache;

        public StockItemsController(ILogger<StockItemsController> logger, IDapperContext dapperContext, IDistributedCache distributedCache)
        {
            this.logger = logger;

            this.dbConnection = dapperContext.ConnectionCreate();

            this.distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            var utcNow = DateTime.UtcNow;

            var cached = await distributedCache.GetAsync("StockItems");
            if (cached != null)
            {
                return this.Ok(MessagePackSerializer.Deserialize<List<Model.StockItemListDtoV1>>(cached));
            }

            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);

            await distributedCache.SetAsync("StockItems", MessagePackSerializer.Serialize(stockItems), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = new DateTime(utcNow.Year, utcNow.Month, DateTime.DaysInMonth(utcNow.Year, utcNow.Month), 23, 59, 59)
            });

            return this.Ok(stockItems);
        }

        [HttpDelete()]
        public async Task<ActionResult> ListCacheDelete()
        {
            await distributedCache.RemoveAsync("StockItems");

            logger.LogInformation("StockItems list removed");

            return this.Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
        {
            var cached = await distributedCache.GetAsync($"StockItem:{id}");
            if (cached != null)
            {
                return this.Ok(MessagePackSerializer.Deserialize<Model.StockItemGetDtoV1>(cached));
            }

            var stockItem = await dbConnection.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
            if (stockItem == default)
            {
                logger.LogInformation("StockItem:{id} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            await distributedCache.SetAsync($"StockItem:{id}", MessagePackSerializer.Serialize<Model.StockItemGetDtoV1>(stockItem), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = StockItemGetByIdRelativeToNowExpiration
            });

            return this.Ok(stockItem);
        }

        [HttpDelete("{id}")]
        public ActionResult ItemCacheDelete(int id)
        {
            distributedCache.Remove($"StockItem:{id}");

            logger.LogInformation("StockItem:{id} removed", id);

            return this.NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get([Required][MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")] string searchText)
        {
            var cached = await distributedCache.GetAsync($"StockItemsSearch:{searchText.ToLower()}");
            if (cached != null)
            {
                return this.Ok(MessagePackSerializer.Deserialize<List<Model.StockItemListDtoV1>>(cached));
            }

            var stockItems = await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: new { searchText, MaximumRowsToReturn = StockItemSearchMaximumRowsToReturn }, commandType: CommandType.StoredProcedure);

            await distributedCache.SetAsync($"StockItemsSearch:{searchText.ToLower()}", MessagePackSerializer.Serialize(stockItems), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = StockItemsSearchSlidingExpiration
            });

            return this.Ok(stockItems);
        }
    }
}

