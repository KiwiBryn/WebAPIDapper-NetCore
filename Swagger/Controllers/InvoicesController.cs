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
namespace devMobile.WebAPIDapper.Swagger.Controllers
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Data.SqlClient;

    using Microsoft.AspNetCore.Mvc;
    
    using devMobile.Azure.DapperTransient;

    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<InvoicesController> logger;

        public InvoicesController(IConfiguration configuration, ILogger<InvoicesController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Model.InvoiceSummaryGetDtoV1>> Get([Required][Range(1, int.MaxValue, ErrorMessage = "Invoice id must greater than 0")] int id)
        {
            Model.InvoiceSummaryGetDtoV1 response ;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                var invoiceSummary = await db.QueryMultipleWithRetryAsync("[Sales].[InvoiceSummaryGetV1]", param: new { InvoiceId = id }, commandType: CommandType.StoredProcedure);

                response = await invoiceSummary.ReadSingleOrDefaultWithRetryAsync<Model.InvoiceSummaryGetDtoV1>();
                if (response == default)
                {
                    logger.LogInformation("Invoice:{id} not found", id);

                    return this.NotFound($"Invoice:{id} not found");
                }

                response.InvoiceLines = (await invoiceSummary.ReadWithRetryAsync<Model.InvoiceLineSummaryListDtoV1>()).ToArray();

                response.StockItemTransactions = (await invoiceSummary.ReadWithRetryAsync<Model.StockItemTransactionSummaryListDtoV1>()).ToArray();
            }

            return this.Ok(response);
        }
    }
}

