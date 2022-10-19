//---------------------------------------------------------------------------------
// Copyright (c) February 2022, devMobile Software
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
namespace devMobile.WebAPIDapper.Basics.Model
{
	using System;

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
}
