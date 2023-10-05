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
using Redis.OM.Modeling;


namespace devMobile.WebAPIDapper.CachingWithRedisOM.Model
{
    /*
    CREATE TABLE [Warehouse].[StockItems](
	    [StockItemID] [int] IDENTITY(1,1) NOT NULL,
	    [StockItemName] [nvarchar](100) NOT NULL,
	    [SupplierID] [int] NOT NULL,
	    [ColorID] [int] NULL,
	    [UnitPackageID] [int] NOT NULL,
	    [OuterPackageID] [int] NOT NULL,
	    [Brand] [nvarchar](50) NULL,
	    [Size] [nvarchar](20) NULL,
	    [LeadTimeDays] [int] NOT NULL,
	    [QuantityPerOuter] [int] NOT NULL,
	    [IsChillerStock] [bit] NOT NULL,
	    [Barcode] [nvarchar](50) NULL,
	    [TaxRate] [decimal](18, 3) NOT NULL,
	    [UnitPrice] [decimal](18, 2) NOT NULL,
	    [RecommendedRetailPrice] [decimal](18, 2) NULL,
	    [TypicalWeightPerUnit] [decimal](18, 3) NOT NULL,
	    [MarketingComments] [nvarchar](max) NULL,
	    [InternalComments] [nvarchar](max) NULL,
	    [Photo] [varbinary](max) NULL,
	    [CustomFields] [nvarchar](max) NULL,
	    [Tags]  AS (json_query([CustomFields],N'$.Tags')),
	    [SearchDetails]  AS (concat([StockItemName],N' ',[MarketingComments])),
	    [LastEditedBy] [int] NOT NULL,
	    [ValidFrom] [datetime2](7) GENERATED ALWAYS AS ROW START NOT NULL,
	    [ValidTo] [datetime2](7) GENERATED ALWAYS AS ROW END NOT NULL,
     CONSTRAINT [PK_Warehouse_StockItems] PRIMARY KEY CLUSTERED 
    (
	    [StockItemID] ASC
    )
    */

    [Document(StorageType = StorageType.Json, Prefixes = new[] { "StockItem" })]
    public class StockItem
    {
        [RedisIdField][Indexed] public int Id { get; set; }

        [Searchable] public string? Name { get; set; }

        public decimal RecommendedRetailPrice { get; set; }

        public decimal TaxRate { get; set; }
    }
}

