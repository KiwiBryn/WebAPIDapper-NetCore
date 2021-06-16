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
	public class StockItemsPaginationController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockItemsPaginationController> logger;

		public StockItemsPaginationController(IConfiguration configuration, ILogger<StockItemsPaginationController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockItemListDtoV1>>> Get([FromQuery] Model.StockItemPagingDtoV1 request)
		{
			IEnumerable<Model.StockItemListDtoV1> response = null;

			if (!ModelState.IsValid)
			{
				this.BadRequest(ModelState);
			}

			try
			{
				var parameters = new DynamicParameters();

				parameters.Add("@PageNumber", request.PageNumber);
				parameters.Add("@PageSize", request.PageSize);

				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QueryAsync<Model.StockItemListDtoV1>(sql: @"SELECT [StockItemID] as ""ID"", [StockItemName] as ""Name"", [RecommendedRetailPrice], [TaxRate] FROM[Warehouse].[StockItems] ORDER BY ID OFFSET @PageSize * (@PageNumber-1) ROWS FETCH NEXT @PageSize ROWS ONLY", param: parameters, commandType: CommandType.Text);
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "StockItemsPagination exception retrieving list of StockItems with PageSize:{0} PageNumber:{1}", request.PageSize, request.PageNumber);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}
	}
}

