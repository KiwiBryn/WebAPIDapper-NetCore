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
    using System.ComponentModel.DataAnnotations;

    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Newtonsoft.Json;

    /// <summary>
    /// DTO for returning summarised list of stock item information.
    /// </summary>
    public class StockItemListDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to a stock item within the database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of a stock item (but not a full description).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Recommended retail price for this stock item.
        /// </summary>
        public decimal RecommendedRetailPrice { get; set; }
        /// <summary>
        /// Tax rate to be applied.
        /// </summary>
        public decimal TaxRate { get; set; }
    }

    /// <summary>
    /// DTO for returning summarised stock item information.
    /// </summary>
    public class StockItemGetDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to a stock item within the database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of a stock item (but not a full description).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Selling price (ex-tax) for one unit of this product.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Recommended retail price for this stock item.
        /// </summary>
        public decimal RecommendedRetailPrice { get; set; }

        /// <summary>
        /// Tax rate to be applied.
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Quantity of the stock item in an outer package.
        /// </summary>
        public int QuantityPerOuter { get; set; }

        /// <summary>
        /// Typical weight for one unit of this product (packaged).
        /// </summary>
        public decimal TypicalWeightPerUnit { get; set; }

        /// <summary>
        /// Full name of unit package that stock items can be purchased in or sold in.
        /// </summary>
        public string UnitPackageName { get; set; }

        /// <summary>
        /// Full name of outer package that stock items can be purchased in or sold in.
        /// </summary>
        public string OuterPackageName { get; set; }

        /// <summary>
        /// Numeric ID used for reference to a supplier within the database.
        /// </summary>
        public int SupplierID { get; set; }

        /// <summary>
        /// Supplier's full name (usually a trading name).
        /// </summary>
        public string SupplierName { get; set; }
    }

    /// <summary>
    /// DTO for stock item search parameters.
    /// </summary>
    public class StockItemNameSearchDtoV1
    {
        [BindNever]
        public int UserId { get; set; }

        /// <summary>
        /// Text in stock item name to search for.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "The name search text must be at least {1} characters long"), MaxLength(Constants.StockItemNameMaximumLength, ErrorMessage = "The name search text must be no more that {1} characters long")]
        public string SearchText { get; set; }

        /// <summary>
        /// Maximum number of search result to return for a query.
        /// </summary>
        [Required]
        [Range(1, Constants.StockItemSearchMaximumRowsToReturn, ErrorMessage = "The maximum number of rows to return must be greater than or equal to {1} and less then or equal {2}")]
        public int MaximumRowsToReturn { get; set; }
    }
}
