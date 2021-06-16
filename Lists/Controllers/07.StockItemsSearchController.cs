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
using System.Collections.Generic;
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
	[Route("[controller]")]
	public class StockItemsSearchController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsSearchController> logger;

		public StockItemsSearchController(IConfiguration configuration, ILogger<StockItemsSearchController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery][MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")][MaxLength(100, ErrorMessage = "The name search text must be no more that 100 characters long")] string name)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					if (name != null)
					{
						var parameters = new DynamicParameters();

						parameters.Add("@Name", value: name);

						response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems] WHERE [StockItemName] LIKE N'%' + @Name + N'%'", param: parameters, commandType: CommandType.Text);
					}
					else
					{
						response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
					}
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsSearch exception searching for list of StockItems with name like:{0}", name);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}

