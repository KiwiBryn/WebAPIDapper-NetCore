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
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

using AutoMapper;

using devMobile.Azure.Dapper;
using devMobile.Azure.DapperTransient;

namespace devMobile.WebAPIDapper.HttpPatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private readonly ILogger<StockItemsController> logger;
        private readonly IDapperContext dapperContext;
        private readonly IMapper mapper;

        public StockItemsController(ILogger<StockItemsController> logger, IDapperContext dapperContext, IMapper mapper)
        {
            this.logger = logger;
            this.dapperContext = dapperContext;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (IDbConnection db = dapperContext.ConnectionCreate())
            {
                response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [UnitPrice] as ""UnitPrice"",[RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
            }

            return this.Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
        {
            Model.StockItemGetDtoV1 response;

            using (IDbConnection db = dapperContext.ConnectionCreate())
            {
                response = await db.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);
            }

            if (response == default)
            {
                logger.LogInformation("StockItem:{id} not found", id);

                return this.NotFound($"StockItem:{id} not found");
            }

            return this.Ok(response);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Model.StockItemGetDtoV1>> Patch([FromBody] JsonPatchDocument<Model.StockItemPatchDtoV1> stockItemPatch, int id)
        {
            Model.StockItemGetDtoV1 stockItem;

            using (IDbConnection db = dapperContext.ConnectionCreate())
            {
                stockItem = await db.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);

                if (stockItem == default)
                {
                    logger.LogInformation("StockItem:{id} not found", id);

                    return this.NotFound($"StockItem:{id} not found");
                }

                Model.StockItemPatchDtoV1 stockItemPatchDto = mapper.Map<Model.StockItemPatchDtoV1>(stockItem);

                stockItemPatch.ApplyTo(stockItemPatchDto, ModelState);

                if (!ModelState.IsValid || !TryValidateModel(stockItemPatchDto))
                {
                    logger.LogInformation("stockItemPatchDto invalid {0}", string.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception))); // would extract this out into shared module

                    return BadRequest(ModelState);
                }

                mapper.Map(stockItemPatchDto, stockItem);

                await db.ExecuteWithRetryAsync(sql: "UPDATE Warehouse.StockItems SET StockItemName=@Name, UnitPrice=@UnitPrice, RecommendedRetailPrice=@RecommendedRetailPrice WHERE StockItemId=@Id", param: stockItem, commandType: CommandType.Text);
            }

            return this.Ok();
        }
    }
}

