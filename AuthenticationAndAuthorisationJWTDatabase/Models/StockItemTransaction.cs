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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase.Models
{
	using System;

    /// <summary>
    /// DTO for returning summarised list of stock item transaction information.
    /// </summary>
	public class StockItemTransactionSummaryListDtoV1
	{
        /// <summary>
        /// Numeric ID used to refer to a stock item transaction within the database.
        /// </summary>
        public int StockItemTransactionID { get; set; }

        /// <summary>
        /// Numeric ID used for reference to a stock item within the database.
        /// </summary>
        public int StockItemID { get; set; }
        /// <summary>
        /// Full name of a stock item (but not a full description)
        /// </summary>
        public string StockItemName { get; set; }

        /// <summary>
        /// Numeric ID used for reference to a transaction type within the database.
        /// </summary>
		public int TransactionTypeID { get; set; }
        /// <summary>
        /// Full name of the transaction type.
        /// </summary>
		public string TransactionTypeName { get; set; }

        /// <summary>
        /// Date and time when the transaction occurred.
        /// </summary>
		public DateTime TransactionAt { get; set; }

        /// <summary>
        /// Quantity of stock movement (positive is incoming stock, negative is outgoing).
        /// </summary>
		public decimal Quantity { get; set; }
	}
}
