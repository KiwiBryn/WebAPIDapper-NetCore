﻿//---------------------------------------------------------------------------------
// Copyright (c) Feb 2022, devMobile Software
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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using devMobile.Azure.DapperTransient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace devMobile.WebAPIDapper.Lists.Controllers
{
	/*
	public class InvoiceSummaryGetDtoV1
	{
		public int OrderId { get; set; }

		public int DeliveryMethodId { get; set; }
		public string DeliveryMethodName { get; set; }

		public int SalesPersonId { get; set; }
		public string SalesPersonName { get; set; }

		public DateTime InvoicedOn { get; set; }
		public string CustomerPurchaseOrderNumber { get; set; }
		public bool IsCreditNote { get; set; }
		public string CreditNoteReason { get; set; }

		public string Comments { get; set; }


		public string DeliveryInstructions { get; set; }

		public string DeliveryRun { get; set; }

		public DateTime? DeliveredAt { get; set; }

		public string DeliveredTo { get; set; }

		public InvoiceInvoiceLineSummaryListDtoV1[] InvoiceLines { get; set; }

		public StockItemTransactionSummaryListDtoV1[] StockItemTransactions { get; set; }
	}

	public class InvoiceInvoiceLineSummaryListDtoV1
	{
		public int InvoiceLineID { get; set; }

		public int StockItemID { get; set; }
		public string StockItemDescription { get; set; }

		public int PackageTypeID { get; set; }
		public string PackageTypeName { get; set; }

		public int Quantity { get; set; }
		public decimal? UnitPrice { get; set; }
		public decimal TaxRate { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal ExtendedPrice { get; set; }
	}

	public class StockItemTransactionSummaryListDtoV1
	{
		public int StockItemTransactionID { get; set; }

		public int StockItemID { get; set; }
		public string StockItemName { get; set; }

		public int TransactionTypeID { get; set; }
		public string TransactionTypeName { get; set; }

		public DateTime TransactionAt { get; set; }

		public decimal Quantity { get; set; }
	}
	*/

	[Route("api/[controller]")]
	[ApiController]
	public class InvoiceSummaryLookupController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<InvoiceSummaryLookupController> logger;

		public InvoiceSummaryLookupController(IConfiguration configuration, ILogger<InvoiceSummaryLookupController> logger)
		{
			this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

			this.logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<IAsyncEnumerable<Model.InvoiceSummaryGetDtoV1>>> Get([Range(1, int.MaxValue, ErrorMessage = "Invoice id must greater than 0")] int id)
		{
			Model.InvoiceSummaryGetDtoV1 response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					var order = await db.QueryMultipleWithRetryAsync("[Sales].[InvoiceSummaryGetV1]", param: new { InvoiceId = id }, commandType: CommandType.StoredProcedure);

					response = await order.ReadSingleOrDefaultWithRetryAsync<Model.InvoiceSummaryGetDtoV1>();
					if (response == default)
					{
						logger.LogInformation("Order:{0} not found", id);

						return this.NotFound($"Order:{id} not found");
					}

					response.InvoiceLines = (await order.ReadWithRetryAsync<Model.InvoiceLineSummaryListDtoV1>()).ToArray();

					response.StockItemTransactions = (await order.ReadWithRetryAsync<Model.StockItemTransactionSummaryListDtoV1>()).ToArray();
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving Order and list of OrderLines");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}
