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
using System.Data;
using System.Data.Common;

using Microsoft.AspNetCore.Mvc;

using StackExchange.Profiling.Data;

using Dapper;
using devMobile.Dapper;


namespace devMobile.WebAPIDapper.SQLProfiling.Controllers
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

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetUsing()
        {
            using (IDbConnection db = dapperContext.ConnectionCreate())
            {
                return this.Ok(await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems] ; WAITFOR DELAY '00:00:02'", commandType: CommandType.Text));
            }
        }

        [HttpGet("OpenNoClose")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetOpenNoClose()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (IDbConnection sqlConnection = dapperContext.ConnectionCreate())
            {
                using (DbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), StackExchange.Profiling.MiniProfiler.Current))
                {
                    //await profiledDbConnection.OpenAsync();

                    response = await profiledDbConnection.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'", commandType: CommandType.Text);
                }
            }

            return this.Ok(response);
        }

        [HttpGet("OpenClose")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetOpenClose()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (IDbConnection sqlConnection = dapperContext.ConnectionCreate())
            {
                using (DbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), StackExchange.Profiling.MiniProfiler.Current))
                {
                    await profiledDbConnection.OpenAsync();

                    response = await profiledDbConnection.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'", commandType: CommandType.Text);
                    //response = await profiledDbConnection.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);

                    await profiledDbConnection.CloseAsync();
                }
            }

            return this.Ok(response);
        }

        [HttpGet("Ado")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetAdoProfiler()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            using (IDbConnection connection = dapperContext.ConnectionCreate())
            {
                using (ProfiledDbConnection profiledDbConnection = new ProfiledDbConnection((DbConnection)connection, StackExchange.Profiling.MiniProfiler.Current))
                {
                    profiledDbConnection.Open();

                    DbCommand command = profiledDbConnection.CreateCommand();

                    command.CommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";
                    command.CommandType = CommandType.Text;

                    var reader = command.ExecuteReader();

                    while (await reader.ReadAsync())
                    {
                        response.Add(new Model.StockItemListDtoV1()
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            RecommendedRetailPrice = reader.GetDecimal(2),
                            TaxRate = reader.GetDecimal(3)
                        });
                    }
                }
            }

            return this.Ok(response);
        }
    }
}


