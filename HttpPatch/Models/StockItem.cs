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
//---------------------------------------------------------------------------------
namespace devMobile.WebAPIDapper.HttpPatch.Model
{
	using System.ComponentModel.DataAnnotations;

	public class StockItemListDtoV1
	{
		public int Id { get; set; }

		public string Name { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal RecommendedRetailPrice { get; set; }

		public decimal TaxRate { get; set; }
	}

	public class StockItemGetDtoV1
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public decimal UnitPrice { get; set; }

		public decimal RecommendedRetailPrice { get; set; }

		public decimal TaxRate { get; set; }

		public int QuantityPerOuter { get; set; }

		public decimal TypicalWeightPerUnit { get; set; }

		public string UnitPackageName { get; set; }

		public string OuterPackageName { get; set; }

		public int SupplierID { get; set; }

		public string SupplierName { get; set; }
	}

    public class StockItemPatchDtoV1
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "The name text must be at least {2} and no more than {1} characters long")]  // These would be constants in a real application
        public string Name { get; set; }

        [Required]
        [Range(0.0, 100.0)] // These would be constants in a real application
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0.0, 1000000.0)] // These would be constants in a real application
        public decimal RecommendedRetailPrice { get; set; }
    }
}
