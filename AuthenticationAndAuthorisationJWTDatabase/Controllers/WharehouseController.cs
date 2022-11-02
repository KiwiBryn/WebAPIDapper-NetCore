//---------------------------------------------------------------------------------
// Copyright (c) October 2022, devMobile Software
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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase.Controllers
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using devMobile.Azure.DapperTransient;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// WebAPI controller for handling Invoice functionality.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<WarehouseController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceController"/> class.
        /// </summary>
        /// <param name="configuration">DI configuration provider.</param>
        /// <param name="logger">DI logging provider.</param>/// 
        public WarehouseController(IConfiguration configuration, ILogger<WarehouseController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

            this.logger = logger;
        }

        [Authorize(Roles = "WarehousePerson,WarehouseAdministrator,Administrator")]
        [HttpGet("NextToPick")]
        public async Task<ActionResult<IAsyncEnumerable<Models.OrderToPickListDtoV1>>> Get()
        {
            IEnumerable<Models.OrderToPickListDtoV1> response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.OrderToPickListDtoV1>(sql: "[Warehouse].[OrderNextReadyToPickV1]", param:new { UserId = HttpContext.PersonId()}, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("No orders ready to pick");
            }

            return this.Ok(response);
        }

        [Authorize(Roles = "WarehousePerson,WarehouseAdministrator,Administrator")]
        [HttpGet("OrderToPick")]
        public async Task<ActionResult<Models.OrderToPickGetDtoV1>> Get([Required][Range(1, int.MaxValue, ErrorMessage = "Order id must greater than 0")] int orderId)
        {
            Models.OrderToPickGetDtoV1 response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                var invoiceSummary = await db.QueryMultipleWithRetryAsync("[Sales].[InvoiceSummaryGetV1]", commandType: CommandType.StoredProcedure);

                response = await invoiceSummary.ReadSingleOrDefaultWithRetryAsync<Models.InvoiceSummaryGetDtoV1>();
                if (response == default)
                {
                    logger.LogInformation("Order:{invoiceId} not found", orderId);

                    return this.NotFound($"Invoice:{orderId} not found");
                }

                response.OrderLinesToPick = (await invoiceSummary.ReadWithRetryAsync<Models.OrderLineToPickListDtoV1>()).ToArray();
            }

            return this.Ok(response);
        }
    }
}