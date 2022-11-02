//---------------------------------------------------------------------------------
// Copyright (c) November 2022, devMobile Software
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

    using Newtonsoft.Json;

    public class OrderToPickListDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to a order within the database
        /// </summary>
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int SalesPersonPersonId { get; set; }

        public string SalesPersonName { get; set; }

        public string CustomerPurchaseOrderNumber { get; set; } 

        public DateTime ExpectedDeliveryDate { get; set; }
    }

    /// <summary>
    /// DTO for returning summarised list of stock item information.
    /// </summary>
    public class OrderToPickGetDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to a order within the database
        /// </summary>
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }

        public string DeliveryAddressLine1 { get; set; }

        public string DeliveryAddressLine2 { get; set; }

        public string DeliveryPostalCode { get; set; }

        public string DeliveryCity { get; set; }

        public string DeliveryLocation { get; set; }

        public string DeliveryRun { get; set; }

        public string DeliveryRunPosition { get; set; }

        public bool IsOneCreditHold { get; set; }

        public OrderLineToPickListDtoV1[] OrderLinesToPick { get; set; }
    }

    public class OrderLineToPickListDtoV1
    {
        public int Id { get; set; }

        public int StockItemId { get; set; }

        public string StockItemName { get; set; }

        public int StockItemColourId { get; set; }

        public string StockItemColourName { get; set; }

        public int StockItemUnitPackageId { get; set; }

        public string StockItemUnitPackageName { get; set; }

        public int StockItemOuterPackageId { get; set; }

        public string StockItemOuterPackageName { get; set; }

        public string StockItemBrand { get; set; }

        public string StockItemSize{ get; set; }

        public string StockQuantityPerOuter { get; set; }

        public bool IsChillerStock{ get; set; }

        public string StockItemBarcode { get; set; }

        public double TypicalWeightPerUnit { get; set; }

        public int Quantity { get; set; }

        public int QuantityPicked { get; set; }

        public DateTime PickingCompletedAt { get; set; }
    }
}
