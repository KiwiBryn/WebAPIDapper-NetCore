//---------------------------------------------------------------------------------
// Copyright (c) Feb 2022, devMobile Software
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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using devMobile.Azure.DapperTransient;


namespace devMobile.WebAPIDapper.Basics.Controllers
{
	public class StockGroupStockItemsListDto
	{
		StockGroupStockItemsListDto()
		{
			StockItems = new List<StockItemListDto>();
		}

		public int StockGroupID { get; set; }

		public string StockGroupName { get; set; }

		public List<StockItemListDto> StockItems { get; set; }
	}

	[Route("api/[controller]")]
	[ApiController]
	public class StockGroupStockItemsMultiMappingController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockGroupStockItemsMultiMappingController> logger;

		public StockGroupStockItemsMultiMappingController(IConfiguration configuration, ILogger<StockGroupStockItemsMultiMappingController> logger)
		{
			this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");

			this.logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<StockGroupStockItemsListDto>>> Get()
		{
			IEnumerable<StockGroupStockItemsListDto> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					var stockGroups = await db.QueryWithRetryAsync<StockGroupStockItemsListDto, StockItemListDto, StockGroupStockItemsListDto>(
						sql: @"SELECT [StockGroups].[StockGroupID] as 'StockGroupID'" +
									",[StockGroups].[StockGroupName]" +
									",[StockItems].StockItemID as 'ID'" +
									",[StockItems].StockItemName as 'Name'" +
									",[StockItems].TaxRate" +
									",[StockItems].RecommendedRetailPrice " +
								"FROM [Warehouse].[StockGroups] " +
									"INNER JOIN[Warehouse].[StockItemStockGroups] ON ([StockGroups].[StockGroupID] = [StockItemStockGroups].[StockGroupID])" +
									"INNER JOIN[Warehouse].[StockItems] ON ([Warehouse].[StockItemStockGroups].[StockItemID] = [StockItems].[StockItemID])",
							(stockGroup, stockItem) =>
							{
								// Not certain I like using a List<> here...
								stockGroup.StockItems.Add(stockItem);
								return stockGroup;
							},
						splitOn: "ID",
						commandType: CommandType.Text);

					response = stockGroups.GroupBy(p => p.StockGroupID).Select(g =>
					{
						var groupedStockGroup = g.First();
						groupedStockGroup.StockItems = g.Select(p => p.StockItems.Single()).ToList();
						return groupedStockGroup;
					});
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving StockGroup, StockItemStockGroup or StockItems");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}
