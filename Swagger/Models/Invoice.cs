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
namespace devMobile.WebAPIDapper.Swagger.Model
{
	using System;

	public class InvoiceLineSummaryListDtoV1
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

		public Model.InvoiceLineSummaryListDtoV1[] InvoiceLines { get; set; }

		public Model.StockItemTransactionSummaryListDtoV1[] StockItemTransactions { get; set; }
	}

}
