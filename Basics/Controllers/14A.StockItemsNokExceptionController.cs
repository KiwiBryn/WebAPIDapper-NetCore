﻿//---------------------------------------------------------------------------------
// Copyright (c) June 2022, devMobile Software
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
// NOTE - The broken SQL is intentional
//---------------------------------------------------------------------------------
namespace devMobile.WebAPIDapper.Basics.Controllers
{
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using System.Threading.Tasks;

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;

	using devMobile.Azure.DapperTransient;

	[Route("api/[controller]")]
	[ApiController]
	public class StockItemsNokExceptionController : ControllerBase
	{
		private readonly string connectionString;

		public StockItemsNokExceptionController(IConfiguration configuration)
		{
			this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
		}

		public async Task<ActionResult<IEnumerable<Model.StockItemListDtoV1>>> GetException()
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				response = await db.QueryWithRetryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItemsException]", commandType: CommandType.Text);
			}

			return this.Ok(response);
		}
	}
}

