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
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Dapper;
using devMobile.Dapper;

using StackExchange.Profiling.Data;
using StackExchange.Profiling;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace devMobile.WebAPIDapper.PerformanceProfiling.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsController : ControllerBase
    {
        private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]";
        //private const string sqlCommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]; WAITFOR DELAY '00:00:02'";

        private readonly IDapperContext dapperContext;

        public StockItemsController(IDapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        [HttpGet("DapperProfiled")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetDapperProfiled()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (IDbConnection db = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);
            }

            return this.Ok(response);
        }

        [HttpGet("DapperProfiledQueryMultiple")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetDapperProfiledQueryMultiple([Required][Range(1, int.MaxValue, ErrorMessage = "Invoice id must greater than 0")] int id)
        {
            Model.InvoiceSummaryGetDtoV1 response = null;

            using (IDbConnection db = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                var invoiceSummary = await db.QueryMultipleAsync("[Sales].[InvoiceSummaryGetV1]", param: new { InvoiceId = id }, commandType: CommandType.StoredProcedure);

                response = await invoiceSummary.ReadSingleOrDefaultAsync<Model.InvoiceSummaryGetDtoV1>();
                if (response == default)
                {
                    return this.NotFound($"Invoice:{id} not found");
                }

                using (MiniProfiler.Current.Step("invoiceSummaryLine.ReadAsync"))
                {
                    response.InvoiceLines = (await invoiceSummary.ReadAsync<Model.InvoiceLineSummaryListDtoV1>()).ToArray();
                }

                using (MiniProfiler.Current.Step("TransactionSummary.ReadAsync"))
                {

                    response.StockItemTransactions = (await invoiceSummary.ReadAsync<Model.StockItemTransactionSummaryListDtoV1>()).ToArray();
                }
            }

            return this.Ok(response);
        }

        [HttpGet("DapperStep")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetDapperStep()
        {
            IEnumerable<Model.StockItemListDtoV1> response;

            using (ProfiledDbConnection connection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                using (MiniProfiler.Current.Step("connection.OpenAsync"))
                {
                    await connection.OpenAsync();
                }

                using (MiniProfiler.Current.Step("connection.QueryAsync"))
                {
                    response = await connection.QueryAsync<Model.StockItemListDtoV1>(sql: sqlCommandText, commandType: CommandType.Text);
                }

                using (MiniProfiler.Current.Step("connection.CloseAsync"))
                {
                    await connection.CloseAsync();
                }
            }

            return this.Ok(response);
        }

        [HttpGet("Profiled")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetProfiled()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            using (ProfiledDbConnection connection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                await connection.OpenAsync();

                using (ProfiledDbCommand command = (ProfiledDbCommand)connection.CreateCommand())
                {
                    command.CommandText = sqlCommandText;
                    command.CommandType = CommandType.Text;

                    using (ProfiledDbDataReader reader = (ProfiledDbDataReader)await command.ExecuteReaderAsync())
                    {
                        var rowParser = reader.GetRowParser<Model.StockItemListDtoV1>();

                        while (await reader.ReadAsync())
                        {
                            response.Add(rowParser(reader));
                        }

                        await reader.CloseAsync();
                    }
                }

                await connection.CloseAsync();
            }

            return this.Ok(response);
        }

        [HttpGet("AdoConnectionProfiled")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetAdoConnectionProfiled()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            using (ProfiledDbConnection connection = new ProfiledDbConnection((DbConnection)dapperContext.ConnectionCreate(), MiniProfiler.Current))
            {
                await connection.OpenAsync();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqlCommandText;
                    command.CommandType = CommandType.Text;

                    using (DbDataReader sqlDataReader = await command.ExecuteReaderAsync())
                    {
                        var rowParser = sqlDataReader.GetRowParser<Model.StockItemListDtoV1>();

                        while (await sqlDataReader.ReadAsync())
                        {
                            response.Add(rowParser(sqlDataReader));
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

            using (DbConnection connection = (DbConnection)dapperContext.ConnectionCreate())
            {
                using (MiniProfiler.Current.Step("dbConnection.OpenAsync"))
                {
                    await connection.OpenAsync();
                }

                using (DbCommand command = MiniProfiler.Current.Inline(() => connection.CreateCommand(), "dbConnection.CreateCommand"))
                {
                    command.CommandText = sqlCommandText;
                    command.CommandType = CommandType.Text;

                    using (DbDataReader reader = await MiniProfiler.Current.Inline(() => command.ExecuteReaderAsync(), "command.ExecuteReaderAsync"))
                    {
                        var rowParser = reader.GetRowParser<Model.StockItemListDtoV1>();

                        while (await MiniProfiler.Current.Inline(() => reader.ReadAsync(), "reader.ReadAsync"))
                        {
                            response.Add(rowParser(reader));
                        }
                    }
                }

                using (MiniProfiler.Current.Step("connection.CloseAsync"))
                {
                    await connection.CloseAsync();
                }
            }

            return this.Ok(response);
        }

        [HttpGet("AdoProfiled")]
        public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetProfiledAdo()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            using (SqlConnection connection = (SqlConnection)dapperContext.ConnectionCreate())
            {
                using (ProfiledDbConnection profiledDbConnection = new ProfiledDbConnection(connection, MiniProfiler.Current))
                {
                    await profiledDbConnection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sqlCommandText, connection))
                    {
                        using (ProfiledDbCommand profiledDbCommand = new ProfiledDbCommand(command, connection, MiniProfiler.Current))
                        {
                            DbDataReader reader = await profiledDbCommand.ExecuteReaderAsync();

                            using (ProfiledDbDataReader profiledDbDataReader = new ProfiledDbDataReader(reader, MiniProfiler.Current))
                            {
                                var rowParser = profiledDbDataReader.GetRowParser<Model.StockItemListDtoV1>();

                                while (await profiledDbDataReader.ReadAsync())
                                {
                                    response.Add(rowParser(profiledDbDataReader));
                                }
                            }
                        }
                    }
                }
            }

            return this.Ok(response);
        }
    }
}


