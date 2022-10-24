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
   using System.ComponentModel.DataAnnotations;

   using Microsoft.AspNetCore.Mvc;

   public class StockItemListDtoV1
   {
      public int Id { get; set; }

      public string Name { get; set; }

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

   public class StockItemNameSearchDtoV1
   {
      [Required]
      [MinLength(3, ErrorMessage = "The name search text must be at least {1} characters long"), MaxLength(20, ErrorMessage = "The name search text must be no more that {1} characters long")]
      public string SearchText { get; set; }

      [Required]
      [Range(1, 100, ErrorMessage = "The maximum number of rows to return must be greater than or equal to {1} and less then or equal {2}")]
      public int MaximumRowsToReturn { get; set; }
   }
}
