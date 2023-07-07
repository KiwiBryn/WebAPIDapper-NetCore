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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using devMobile.Azure.DapperTransient;
using devMobile.Dapper;
using System.Text.Json;

namespace devMobile.WebAPIDapper.CachingWithDistributedCache.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private const int SearchMaximumRowsToReturn = 15;
        private readonly TimeSpan StockItemListAbsoluteExpiration = new TimeSpan(0, 5, 0);
        private readonly TimeSpan StockItemSearchSlidingExpiration = new TimeSpan(0, 1, 0);

        private readonly ILogger<StockItemsController> logger;
        private readonly IDapperContext dapperContext;
        private readonly IDistributedCache cache;

        public StockItemsController(ILogger<StockItemsController> logger, IDapperContext dapperContext, IDistributedCache cache)
        {
            this.logger = logger;

            this.dapperContext = dapperContext;

            this.cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            var cached = await cache.GetAsync("StockItems");

            if (cached == null)
            {
                cached = JsonSerializer.SerializeToUtf8Bytes<IEnumerable<Model.StockItemListDtoV1>>(await dapperContext.ConnectionCreate().QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text));

                await cache.SetAsync("StockItems", cached, new DistributedCacheEntryOptions()
                { 
                    AbsoluteExpiration = DateTimeOffset.Now.Add(StockItemListAbsoluteExpiration)
                });
            }

            var response = JsonSerializer.Deserialize<IEnumerable<Model.StockItemListDtoV1>>(cached);

            return this.Ok(response);
        }

        [HttpDelete()]
        public async Task<ActionResult> ListCacheDelete()
        {
            await cache.RemoveAsync("StockItems");

            return this.NoContent();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
        {
            var cached = await cache.GetAsync($"StockItem{id}");

            if (cached == null)
            {
                var response = await dapperContext.ConnectionCreate().QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
                if (response == default)
                {
                    logger.LogInformation("StockItem:{id} not found", id);

                    return this.NotFound($"StockItem:{id} not found");
                }

                cached = JsonSerializer.SerializeToUtf8Bytes<Model.StockItemGetDtoV1>(await dapperContext.ConnectionCreate().QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure));

                await cache.SetAsync("StockItems{id}", cached, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(StockItemListAbsoluteExpiration)
                });
            }

            return this.Ok(JsonSerializer.Deserialize<Model.StockItemListDtoV1>(cached));
        }

        [HttpDelete("{id}")]
        public ActionResult ItemCacheDelete(int id)
        {
            cache.Remove($"StockItem{id}");

            return this.NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get([Required][MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")] string searchText)
        {
            var cached = await cache.GetAsync($"StockItemsSearch:{searchText.ToLower()}");

            if (cached == null)
            {
                cached = JsonSerializer.SerializeToUtf8Bytes<IEnumerable<Model.StockItemListDtoV1>>(await dapperContext.ConnectionCreate().QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: new { searchText, MaximumRowsToReturn = SearchMaximumRowsToReturn }, commandType: CommandType.StoredProcedure));

                await cache.SetAsync($"StockItemsSearch:{searchText.ToLower()}", cached, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(StockItemListAbsoluteExpiration)
                });
            }

            var response = JsonSerializer.Deserialize<IEnumerable<Model.StockItemListDtoV1>>(cached);

            return this.Ok(response);
        }
    }
}

