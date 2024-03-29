﻿//---------------------------------------------------------------------------------
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
using Microsoft.Extensions.Logging;

using devMobile.Azure.DapperTransient;
using devMobile.Dapper;

using LazyCache;


namespace devMobile.WebAPIDapper.CachingWithLazyCache.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private const int StockItemSearchMaximumRowsToReturn = 15;
        private readonly TimeSpan StockItemListAbsoluteExpirationTime = new TimeSpan(23, 59, 59);
        private readonly TimeSpan StockItemsSearchSlidingExpiration = new TimeSpan(0, 1, 0);

        private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]";
        //private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";

        private readonly ILogger<StockItemsController> logger;
        private readonly IDbConnection dbConnection;
        private readonly IAppCache cache;

        public StockItemsController(ILogger<StockItemsController> logger, IDapperContext dapperContext, IAppCache cache)
        {
            this.logger = logger;

            this.dbConnection = dapperContext.ConnectionCreate();

            this.cache = cache;
        }

        private async Task<IEnumerable<Model.StockItemListDtoV1>> GetStockItemsAsync()
        {
            return await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            MemoryCacheEntryOptions memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.NeverRemove,
                AbsoluteExpiration = DateTime.UtcNow.Date.Add(StockItemListAbsoluteExpirationTime)
            };

            return this.Ok(await cache.GetOrAddAsync("StockItems", () => GetStockItemsAsync(), memoryCacheEntryOptions));
        }

        [HttpDelete()]
        public ActionResult Delete()
        {
            cache.Remove("StockItems");

            return this.Ok();
        }

        private async Task<Model.StockItemGetDtoV1> StockItemByIdAsync(int id)
        {
            return await dbConnection.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
        {
            var utcNow = DateTime.UtcNow;

            Model.StockItemGetDtoV1 response = await cache.GetOrAddAsync($"StockItem{id}", () => StockItemByIdAsync(id), slidingExpiration: StockItemsSearchSlidingExpiration);

            if (response == default)
            {
                logger.LogInformation("StockItem:{id} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            return this.Ok(response);
        }

        [HttpDelete("{id}")]
        public ActionResult ItemCacheDelete(int id)
        {
            cache.Remove($"StockItem{id}");

            return this.Ok();
        }

        private async Task<IEnumerable<Model.StockItemListDtoV1>> StockItemsSearch(string searchText)
        {
            return await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: new { searchText, MaximumRowsToReturn = StockItemSearchMaximumRowsToReturn }, commandType: CommandType.StoredProcedure);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get([Required][MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")] string searchText)
        {
            return this.Ok(await cache.GetOrAddAsync($"StockItemSearch{searchText}", () => StockItemsSearch(searchText.ToLower()), slidingExpiration: StockItemsSearchSlidingExpiration ));
        }
    }
}

