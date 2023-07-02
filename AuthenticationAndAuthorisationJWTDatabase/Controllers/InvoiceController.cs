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
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using devMobile.Azure.DapperTransient;

    /// <summary>
    /// WebAPI controller for handling Invoice functionality.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<InvoiceController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceController"/> class.
        /// </summary>
        /// <param name="configuration">DI configuration provider.</param>
        /// <param name="logger">DI logging provider.</param>/// 
        public InvoiceController(IConfiguration configuration, ILogger<InvoiceController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

            this.logger = logger;
        }

        /// <summary>
        /// Gets a summary of the specified invoice plus associated invoice lines and stock item transactions.
        /// </summary>
        /// <param name="invoiceId">Numeric ID used for referencing an invoice within the database.</param>
        /// <response code="200">Summary of Invoice plus associated InvoiceLines and StockItemTransactions returned.</response>
        /// <response code="404">Invoice ID not found.</response>
        /// <returns>Invoice information with associated invoice lines and item transaction.</returns>
        [Authorize(Roles = "SalesPerson,SalesAdministrator,Administrator")]
        [HttpGet("{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(Models.InvoiceSummaryGetDtoV1))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Models.InvoiceSummaryGetDtoV1>> Get([Required][Range(1, int.MaxValue, ErrorMessage = "Invoice id must greater than 0")] int invoiceId)
        {
            Models.InvoiceSummaryGetDtoV1 response ;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                var invoiceSummary = await db.QueryMultipleWithRetryAsync("[Sales].[InvoiceSummaryGetV2]", param: new { UserId = HttpContext.PersonId(), InvoiceId = invoiceId }, commandType: CommandType.StoredProcedure);

                response = await invoiceSummary.ReadSingleOrDefaultWithRetryAsync<Models.InvoiceSummaryGetDtoV1>();
                if (response == default)
                {
                    logger.LogInformation("Invoice:{invoiceId} not found", invoiceId);

                    return this.NotFound($"Invoice:{invoiceId} not found");
                }

                response.InvoiceLines = (await invoiceSummary.ReadWithRetryAsync<Models.InvoiceLineSummaryListDtoV1>()).ToArray();

                response.StockItemTransactions = (await invoiceSummary.ReadWithRetryAsync<Models.StockItemTransactionSummaryListDtoV1>()).ToArray();
            }

            return this.Ok(response);
        }
    }
}

