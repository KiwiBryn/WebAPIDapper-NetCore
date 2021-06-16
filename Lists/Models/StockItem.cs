//---------------------------------------------------------------------------------
// Copyright (c) June 2021, devMobile Software
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
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace devMobile.WebAPIDapper.Lists.Model
{
	public class StockItemListDtoV1
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public decimal RecommendedRetailPrice { get; set; }

		public decimal TaxRate { get; set; }
	}

	public class StockItemGetDtoV1
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("unitPrice")]
		public decimal UnitPrice { get; set; }

		[JsonPropertyName("recommendedRetailPrice")]
		public decimal RecommendedRetailPrice { get; set; }

		[JsonPropertyName("taxRate")]
		public decimal TaxRate { get; set; }

		[JsonPropertyName("quantityPerOuter")]
		public int QuantityPerOuter { get; set; }

		[JsonPropertyName("typicalWeightPerUnit")]
		public decimal TypicalWeightPerUnit { get; set; }

		[JsonPropertyName("unitPackageName")]
		public string UnitPackageName { get; set; }

		[JsonPropertyName("outerPackageName")]
		public string OuterPackageName { get; set; }

		[JsonPropertyName("supplierID")]
		public int SupplierID { get; set; }

		[JsonPropertyName("supplierName")]
		public string SupplierName { get; set; }

	}

	public class StockItemPagingDtoV1
	{
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "PageSize must be present and greater than 0")]
		public int PageSize { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "PageNumber must be present and greater than 0")]
		public int PageNumber { get; set; }
	}
}
