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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Dapper;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StockItemsLookupController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsLookupController> logger;

		public StockItemsLookupController(IConfiguration configuration, ILogger<StockItemsLookupController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Model.StockItemGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock item id must greater than 0")] int id)
		{
			Model.StockItemGetDtoV1 response = null;

			try
			{
				var parameters = new DynamicParameters();

				parameters.Add("@stockItemId", id);

				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					// Not certain if reabability would be improved by an @ on each line vs. @'s on line lines that need them
					response = await db.QuerySingleOrDefaultAsync<Model.StockItemGetDtoV1>(sql: @"SELECT[StockItems].[StockItemID] as ""ID"" " +
																							@",[StockItems].[StockItemName] as ""Name""" +
																							@",[StockItems].[UnitPrice]" +
																							",[StockItems].[RecommendedRetailPrice]" +
																							",[StockItems].[TaxRate]" +
																							",[StockItems].[QuantityPerOuter]" +
																							",[StockItems].[TypicalWeightPerUnit]" +
																							@",[UnitPackage].PackageTypeName as ""UnitPackageName""" +
																							@",[OuterPackage].PackageTypeName as ""OuterPackageName""" +
																							",[Supplier].[SupplierID]" +
																							",[Supplier].[SupplierName]" +
																						"FROM[Warehouse].[StockItems] as StockItems " +
																						"INNER JOIN[Warehouse].[PackageTypes] as UnitPackage ON ([StockItems].[UnitPackageID] = [UnitPackage].[PackageTypeID])" +
																						"INNER JOIN[Warehouse].[PackageTypes] as OuterPackage ON ([StockItems].[OuterPackageID] = [OuterPackage].[PackageTypeID])" +
																						"INNER JOIN[Purchasing].[Suppliers] as Supplier ON ([StockItems].SupplierID = Supplier.SupplierID)" +
																						"WHERE[StockItems].[StockItemID] = @StockItemId",
																					param: parameters,
																					commandType: CommandType.Text);
				}

				if (response == default)
				{
					return this.NotFound($"StockItemId:{id} not found");
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsLookup exception looking up a StockItem with Id:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}

