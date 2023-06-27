//---------------------------------------------------------------------------------
// Copyright (c) June 2023, devMobile Software
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
using System.Data.Common;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Dapper;
using devMobile.Dapper;

using StackExchange.Profiling.Data;
using StackExchange.Profiling;

namespace devMobile.WebAPIDapper.PerformanceProfiling.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsOpenCloseController : ControllerBase
    {
        private readonly IDapperContext dapperContext;

        public StockItemsOpenCloseController(IDapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        [HttpGet("Using")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetUsing()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (DbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                response = await profiledDbConnection.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'", commandType: CommandType.Text);
            }

            return this.Ok(response);
        }

        [HttpGet("UsingOpenNoClose")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetUsingOpenNoClose()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (DbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                await profiledDbConnection.OpenAsync();

                response = await profiledDbConnection.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'", commandType: CommandType.Text);
            }

            return this.Ok(response);
        }

        [HttpGet("AdoUsing")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetAdoUsing()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            using (ProfiledDbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                await profiledDbConnection.OpenAsync();

                using (DbCommand command = profiledDbConnection.CreateCommand())
                {
                    command.CommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";
                    command.CommandType = CommandType.Text;

                    //using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    using (var reader = command.ExecuteReader())
                    {
                        var rowParser = reader.GetRowParser<Model.StockItemListDtoV1>();

                        while (await reader.ReadAsync())
                        {
                            response.Add(rowParser(reader));
                        }
                    }
                }
            }

            return this.Ok(response);
        }

        [HttpGet("AdoStep")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetAdoStep()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            ProfiledDbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current);
            {
                using (MiniProfiler.Current.Step("profiledDbConnection.OpenAsync"))
                {
                    await profiledDbConnection.OpenAsync();
                }

                DbCommand command = profiledDbConnection.CreateCommand();
                {
                    //command.CommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";
                    command.CommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]";
                    command.CommandType = CommandType.Text;

                    var reader = await MiniProfiler.Current.Inline(() => command.ExecuteReaderAsync(), "command.ExecuteReaderAsync");
                    {
                        var rowParser = reader.GetRowParser<Model.StockItemListDtoV1>();

                        while (await MiniProfiler.Current.Inline(() => reader.ReadAsync(), "reader.ReadAsync"))
                        {
                            response.Add(rowParser(reader));
                        }

                        using (MiniProfiler.Current.Step("reader.CloseAsync"))
                        {
                            await reader.CloseAsync();
                        }
                    }

                    //command.CloseAsync(); // Not supported
                }

                using (MiniProfiler.Current.Step("profiledDbConnection.CloseAsync"))
                {
                    await profiledDbConnection.CloseAsync();
                }
            }

            return this.Ok(response);
        }

        [HttpGet("DapperStep")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetDapperStep()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (ProfiledDbConnection db = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                using (MiniProfiler.Current.Step("db.QueryAsync"))
                {
                    //response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'", commandType: CommandType.Text);
                    response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY", commandType: CommandType.Text);
                }
            }

            return this.Ok(response);
        }
    }
}


