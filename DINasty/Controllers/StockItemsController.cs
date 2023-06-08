//---------------------------------------------------------------------------------
// Copyright (c) July 2022, devMobile Software
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
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using devMobile.Azure.DapperTransient;


namespace devMobile.WebAPIDapper.ListsDIBasic.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
   public class StockItemsController : ControllerBase
   {
      private readonly ILogger<StockItemsController> logger;
      private readonly IDbConnection dbConnection;

      public StockItemsController(ILogger<StockItemsController> logger, IDbConnection dbConnection)
      {
         this.logger = logger;

         this.dbConnection = dbConnection;
      }

      [HttpGet]
      public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get()
      {
         // return this.Ok(await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02';", commandType: CommandType.Text));
         return this.Ok(await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text));
       }


        [HttpGet("{id}")]
      public async Task<ActionResult<Model.StockItemGetDtoV1>> Get(int id)
      {
         Model.StockItemGetDtoV1 response;

         response = await dbConnection.QuerySingleOrDefaultWithRetryAsync<Model.StockItemGetDtoV1>(sql: "[Warehouse].[StockItemsStockItemLookupV1]", param: new { stockItemId = id }, commandType: CommandType.StoredProcedure);

         if (response == default)
         {
            logger.LogInformation("StockItem:{id} not found", id);

            return this.NotFound($"StockItem:{id} not found");
         }

         return this.Ok(response);
      }

      [HttpGet("search")]
      public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery] Model.StockItemNameSearchDtoV1 request)
      {
         return this.Ok(await dbConnection.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: "[Warehouse].[StockItemsNameSearchV1]", param: request, commandType: CommandType.StoredProcedure));
      }
   }
}

