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
//		*****NOTE This approach doesn't work******
//
//		https://devblogs.microsoft.com/azure-sql/configurable-retry-logic-for-microsoft-data-sqlclient/
//
// I started with the EF list of transient faults (essentially a copy n paste)
//
//		https://raw.githubusercontent.com/aspnet/EntityFrameworkCore/master/src/EFCore.SqlServer/Storage/Internal/SqlServerTransientExceptionDetector.cs
//
//		https://devblogs.microsoft.com/azure-sql/configurable-retry-logic-for-microsoft-data-sqlclient/
//
//      https://github.com/dotnet/SqlClient/issues/1640
// 
//---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;	// Instead of System.Data.SqlClient
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Dapper;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockItemsRetryADONetController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<StockItemsRetryADONetController> logger;

        // This is a bit nasty but sufficient for PoC
        private readonly int NumberOfRetries = 3;
        private readonly TimeSpan TimeBeforeNextExecution = TimeSpan.Parse("00:00:01");
        private readonly TimeSpan MaximumInterval = TimeSpan.Parse("00:00:30");
        private readonly List<int> TransientErrors = new List<int>()
        {
            49920, // Cannot process rquest. Too many operations in progress for subscription
			49919, // Cannot process create or update request.Too many create or update operations in progress for subscription
			49918, // Cannot process request. Not enough resources to process request.
			41839, // Transaction exceeded the maximum number of commit dependencies.
			41325, // The current transaction failed to commit due to a serializable validation failure.
			41305, // The current transaction failed to commit due to a repeatable read validation failure.
			41302, // The current transaction attempted to update a record that has been updated since the transaction started.
			41301, // Dependency failure: a dependency was taken on another transaction that later failed to commit.
			40613, // Database XXXX on server YYYY is not currently available. Please retry the connection later.
			40501, // The service is currently busy. Retry the request after 10 seconds
			40197, // The service has encountered an error processing your request. Please try again
			20041, // Transaction rolled back. Could not execute trigger. Retry your transaction.
			17197, // Login failed due to timeout; the connection has been closed. This error may indicate heavy server load.
			14355, // The MSSQLServerADHelper service is busy. Retry this operation later.
			11001, // Connection attempt failed
			10936, // The request limit for the elastic pool has been reached. 
			10929, // The server is currently too busy to support requests.
			10928, // The limit for the database is has been reached
			10922, // Operation failed. Rerun the statement.
			10060, // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
			10054, // A transport-level error has occurred when sending the request to the server.
			10053, // A transport-level error has occurred when receiving results from the server.
			9515, // An XML schema has been altered or dropped, and the query plan is no longer valid. Please rerun the query batch.
			8651, // Could not perform the operation because the requested memory grant was not available in resource pool
			8645, // A timeout occurred while waiting for memory resources to execute the query in resource pool, Rerun the query
			8628, // A timeout occurred while waiting to optimize the query. Rerun the query. 
			4221, // Login to read-secondary failed due to long wait on 'HADR_DATABASE_WAIT_FOR_TRANSITION_TO_VERSIONING'. The replica is not available for login because row versions are missing for transactions that were in-flight when the replica was recycled
			4060, // Cannot open database requested by the login. The login failed.
			3966, // Transaction is rolled back when accessing version store. It was earlier marked as victim when the version store was shrunk due to insufficient space in tempdb. Retry the transaction.
			3960, // Snapshot isolation transaction aborted due to update conflict. You cannot use snapshot isolation to access table directly or indirectly in database
			3935, // A FILESTREAM transaction context could not be initialized. This might be caused by a resource shortage. Retry the operation.
			1807, // Could not obtain exclusive lock on database 'model'. Retry the operation later.
			1221, // The Database Engine is attempting to release a group of locks that are not currently held by the transaction. Retry the transaction.
			1205, // Deadlock
			1204, // The instance of the SQL Server Database Engine cannot obtain a LOCK resource at this time. Rerun your statement.
			1203, // A process attempted to unlock a resource it does not own. Retry the transaction.
			997, // A connection was successfully established with the server, but then an error occurred during the login process.
			921, // Database has not been recovered yet. Wait and try again.
			669, // The row object is inconsistent. Please rerun the query.
			617, // Descriptor for object in database not found in the hash table during attempt to un-hash it. Rerun the query. If a cursor is involved, close and reopen the cursor.
			601, // Could not continue scan with NOLOCK due to data movement.
			233, // The client was unable to establish a connection because of an error during connection initialization process before login.
			121, // The semaphore timeout period has expired.
			64, // A connection was successfully established with the server, but then an error occurred during the login process.
			20, // The instance of SQL Server you attempted to connect to does not support encryption.
		};

        public StockItemsRetryADONetController(IConfiguration configuration, ILogger<StockItemsRetryADONetController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

            this.logger = logger;
        }

		[HttpGet("Dapper")]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetDapper()
        {
            IEnumerable<Model.StockItemListDtoV1> response = null;

            SqlRetryLogicOption sqlRetryLogicOption = new SqlRetryLogicOption()
            {
                NumberOfTries = NumberOfRetries,
                DeltaTime = TimeBeforeNextExecution,
                MaxTimeInterval = MaximumInterval,
                TransientErrors = TransientErrors,
                //AuthorizedSqlCondition = x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^SELECT", RegexOptions.IgnoreCase),
            };

            SqlRetryLogicBaseProvider sqlRetryLogicProvider = SqlConfigurableRetryFactory.CreateFixedRetryProvider(sqlRetryLogicOption);

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                db.RetryLogicProvider = sqlRetryLogicProvider;

                db.RetryLogicProvider.Retrying += new EventHandler<SqlRetryingEventArgs>(OnDapperRetrying);

                await db.OpenAsync(); // Did explicitly so I could yank out the LAN cable.

                response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
            }

            return this.Ok(response);
        }

        protected void OnDapperRetrying(object sender, SqlRetryingEventArgs args)
        {
            logger.LogInformation("Dapper retrying for {RetryCount} times for {args.Delay.TotalMilliseconds:0.} mSec - Error code: {Number}", args.RetryCount, args.Delay.TotalMilliseconds, (args.Exceptions[0] as SqlException).Number);
        }

        [HttpGet("AdoNet")]
        public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> GetAdoNet()
        {
            List<Model.StockItemListDtoV1> response = new List<Model.StockItemListDtoV1>();

            // Both connection and command share same logic not really an issue for nasty demo
            SqlRetryLogicOption sqlRetryLogicOption = new SqlRetryLogicOption()
            {
                NumberOfTries = NumberOfRetries,
                DeltaTime = TimeBeforeNextExecution,
                MaxTimeInterval = MaximumInterval,
                TransientErrors = TransientErrors,
                //AuthorizedSqlCondition = x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^SELECT", RegexOptions.IgnoreCase),
            };

            SqlRetryLogicBaseProvider sqlRetryLogicProvider = SqlConfigurableRetryFactory.CreateFixedRetryProvider(sqlRetryLogicOption);


            // This ADO.Net is a bit overkill but just wanted to highlight ADO.Net vs. Dapper
            using (SqlConnection sqlConnection = new SqlConnection(this.connectionString))
            {
                sqlConnection.RetryLogicProvider = sqlRetryLogicProvider;
                sqlConnection.RetryLogicProvider.Retrying += new EventHandler<SqlRetryingEventArgs>(OnConnectionRetrying);

                await sqlConnection.OpenAsync(); // Did explicitly so I could yank out the LAN cable.

                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]";
                    sqlCommand.CommandType = CommandType.Text;

                    sqlCommand.RetryLogicProvider = sqlRetryLogicProvider;
                    sqlCommand.RetryLogicProvider.Retrying += new EventHandler<SqlRetryingEventArgs>(OnCommandRetrying);

                    // Over kill but makes really obvious
                    using (SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        while (await sqlDataReader.ReadAsync())
                        {
                            response.Add(new Model.StockItemListDtoV1()
                            {
                                Id = sqlDataReader.GetInt32("Id"),
                                Name = sqlDataReader.GetString("Name"),
                                RecommendedRetailPrice = sqlDataReader.GetDecimal("RecommendedRetailPrice"),
                                TaxRate = sqlDataReader.GetDecimal("TaxRate"),
                            });
                        }
                    }
                };
            }

            return this.Ok(response);
        }

        protected void OnConnectionRetrying(object sender, SqlRetryingEventArgs args)
        {
            logger.LogInformation("Connection retrying for {RetryCount} times for {args.Delay.TotalMilliseconds:0.} mSec - Error code: {Number}", args.RetryCount, args.Delay.TotalMilliseconds, (args.Exceptions[0] as SqlException).Number);
        }

        protected void OnCommandRetrying(object sender, SqlRetryingEventArgs args)
        {
            logger.LogInformation("Command retrying for {RetryCount} times for {args.Delay.TotalMilliseconds:0.} mSec - Error code: {Number}", args.RetryCount, args.Delay.TotalMilliseconds, (args.Exceptions[0] as SqlException).Number);
        }
    }
}

