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
namespace devMobile.WebAPIDapper.Swagger.Models
{
    using System;

    using Newtonsoft.Json;

    /// <summary>
    /// Invoice line summary DTO with reduced number of fields some of which have been "flattened" for easy of display in lists.
    /// </summary>
    public class InvoiceLineSummaryListDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to a line on an Order within the database.
        /// </summary>
        public int InvoiceLineID { get; set; }

        /// <summary>
        /// Numeric ID used for reference to a stock item within the database.
        /// </summary>
        public int StockItemID { get; set; }

        /// <summary>
        /// Description of the item supplied (Usually the stock item name but can be overridden).
        /// </summary>
        public string StockItemDescription { get; set; }

        /// <summary>
        /// Numeric ID of type of package to be supplied.
        /// </summary>
        public int PackageTypeID { get; set; }

        /// <summary>
        /// Name of type of package to be supplied
        /// </summary>
        public string PackageTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Quantity to be supplied.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price to be charged.
        /// </summary>
		public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Tax rate to be applied.
        /// </summary>
		public decimal TaxRate { get; set; }

        /// <summary>
        /// Tax amount calculated.
        /// </summary>
		public decimal TaxAmount { get; set; }

        /// <summary>
        /// Extended line price charged.
        /// </summary>
		public decimal ExtendedPrice { get; set; }
    }

    /// <summary>
    /// Invoice summary DTO with reduced number of fields some of which have been "flattened" for easy of display in lists.
    ///</summary>
    public class InvoiceSummaryGetDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to an invoice within the database.
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Numeric ID used for reference to an order within the database.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Numeric ID used for reference to a delivery method within the database.
        /// </summary>
		public int DeliveryMethodId { get; set; }

        /// <summary>
        /// Full name of methods that can be used for delivery of customer orders.
        /// </summary>
		public string DeliveryMethodName { get; set; } = string.Empty;

        /// <summary>
        /// Numeric ID used for reference to a sales person within the database.
        /// </summary>
		public int SalesPersonId { get; set; }

        /// <summary>
        /// Full name of the salesperson.
        /// 
        /// </summary>
        public string SalesPersonName { get; set; } = string.Empty;

        /// <summary>
        /// Date that this invoice was raised.
        /// </summary>
        public DateTime InvoicedOn { get; set; }

        /// <summary>
        /// Purchase Order Number received from customer.
        /// </summary>
		public string CustomerPurchaseOrderNumber { get; set; }

        /// <summary>
        /// Is this a credit note (rather than an invoice).
        /// </summary>
		public bool IsCreditNote { get; set; }

        /// <summary>
        /// eason that this credit note needed to be generated (if applicable).
        /// </summary>
        public string CreditNoteReason { get; set; }

        /// <summary>
        /// Any comments related to this invoice (sent to customer).
        /// </summary>
		public string Comments { get; set; }

        /// <summary>
        /// Any comments related to delivery (sent to customer)'.
        /// </summary>
		public string DeliveryInstructions { get; set; }

        /// <summary>
        /// Delivery run for this shipment.
        /// </summary>
		public string DeliveryRun { get; set; }

        /// <summary>
        /// Confirmed delivery date and time promoted from JSON delivery data.
        /// </summary>
		public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Confirmed receiver promoted from JSON delivery data.
        /// </summary>
        [JsonProperty("test", Required = Required.Always)]
        public string DeliveredTo { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public InvoiceLineSummaryListDtoV1[] InvoiceLines { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public Models.StockItemTransactionSummaryListDtoV1[] StockItemTransactions { get; set; }
    }
}
