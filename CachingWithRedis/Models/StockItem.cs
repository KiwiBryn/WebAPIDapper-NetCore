//---------------------------------------------------------------------------------
// Copyright (c) September 2023, devMobile Software
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
namespace devMobile.WebAPIDapper.CachingWithRedis.Model
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class StockItemListDtoV1
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal RecommendedRetailPrice { get; set; }

        public decimal TaxRate { get; set; }
    }

#if SERIALISER_SOURCE_GENERATION
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(List<StockItemListDtoV1>))]
    public partial class StockItemListDtoV1GenerationContext : JsonSerializerContext
    {
    }
#endif

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
}
