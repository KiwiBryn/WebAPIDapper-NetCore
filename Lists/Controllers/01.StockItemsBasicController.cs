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
using System.Data;
using System.Data.SqlClient;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Dapper;

namespace devMobile.WebAPIDapper.Lists.Controllers
{
	public class StockItemListDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal RecommendedRetailPrice { get; set; }
		public decimal TaxRate { get; set; }
	}

	[ApiController]
	[Route("api/[controller]")]
	public class StockItemsBasicController : ControllerBase
	{
		private readonly string connectionString;

		public StockItemsBasicController(IConfiguration configuration)
		{
			this.connectionString = configuration.GetConnectionString("WideWorldImportersDatabase");
		}

		public IEnumerable<StockItemListDto> Get()
		{
			IEnumerable<StockItemListDto> response = null;

			using (SqlConnection db = new SqlConnection(this.connectionString))
			{
				response = db.Query<StockItemListDto>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM [Warehouse].[StockItems]", commandType: CommandType.Text);
			}

			return response;
		}
	}
}
