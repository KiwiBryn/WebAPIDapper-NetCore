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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Dapper;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StockGroupsETagController : ControllerBase
	{
		private readonly string connectionString;
		private readonly ILogger<StockGroupsETagController> logger;

		// Would move these to a shared constants file
		private const byte ETagBytesLength = 8;
		private const byte ETagBase64Length = 12;

		public StockGroupsETagController(IConfiguration configuration, ILogger<StockGroupsETagController> logger)
		{
			this.connectionString = configuration.GetSection("ConnectionStrings").GetSection("WideWorldImportersDatabase").Value;

			this.logger = logger;
		}

		[HttpHead]
		public async Task<ActionResult> Head([FromHeader(Name = "ETag"), Required(ErrorMessage = "eTag header missing"), MinLength(ETagBase64Length, ErrorMessage = "eTag header value too short"), MaxLength(ETagBase64Length, ErrorMessage = "eTag header value too long")] string eTag)
		{
			byte[] headerVersion = new byte[ETagBytesLength];

			if (!Convert.TryFromBase64String(eTag, headerVersion, out _))
			{
				logger.LogInformation("eTag invalid format");

				return this.BadRequest("eTag invalid format");
			}

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					byte[] databaseVersion = await db.ExecuteScalarAsync<byte[]>(sql: "SELECT MAX(Version) FROM [Warehouse].[StockGroups]", commandType: CommandType.Text);

					if (headerVersion.SequenceEqual(databaseVersion))
					{
						return this.StatusCode(StatusCodes.Status304NotModified);
					}
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving StockItem list");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok();
		}

		[HttpGet]
		public async Task<ActionResult<IAsyncEnumerable<Model.StockGroupListDtoV1>>> Get()
		{
			IEnumerable<Model.StockGroupListDtoV1> response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					var parameters = new DynamicParameters();

					parameters.Add("@RowVersion", dbType: DbType.Binary, direction: ParameterDirection.Output, size: ETagBytesLength);

					response = await db.QueryAsync<Model.StockGroupListDtoV1>(sql: @"SELECT [StockGroupID] as ""ID"", [StockGroupName] as ""Name""FROM [Warehouse].[StockGroups] ORDER BY Name; SELECT @RowVersion=MAX(Version) FROM [Warehouse].[StockGroups]", param: parameters, commandType: CommandType.Text);

					if (response.Any())
					{
						byte[] rowVersion = parameters.Get<byte[]>("RowVersion");

						this.HttpContext.Response.Headers.Add("ETag", Convert.ToBase64String(rowVersion));
					}
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving list of StockGroups");

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok(response);
		}

		[HttpHead("{id}")]
		public async Task<ActionResult> Head([Range(1, int.MaxValue, ErrorMessage = "Stock Group id must greater than 0")] int id, [Required][FromHeader(Name = "ETag")][MinLength(ETagBase64Length, ErrorMessage = "eTag length {0} invalid too short")][MaxLength(ETagBase64Length, ErrorMessage = "eTag length {0} invalid too long")] string eTag)
		{
			byte[] headerVersion = new byte[ETagBytesLength];

			if (!Convert.TryFromBase64String(eTag, headerVersion, out _))
			{
				logger.LogInformation("eTag invalid format");

				return this.BadRequest("eTag invalid format");
			}

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					byte[] databaseVersion = await db.ExecuteScalarAsync<byte[]>(sql: "SELECT Version FROM [Warehouse].[StockGroups] WHERE [StockGroupID]=@id", param: new { id }, commandType: CommandType.Text);

					if (databaseVersion == null)
					{
						logger.LogInformation("StockGroup:{0} not found", id);

						return this.NotFound($"StockGroup:{id} not found");
					}

					if (headerVersion.SequenceEqual(databaseVersion))
					{
						return this.StatusCode(StatusCodes.Status304NotModified);
					}
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Retrieving rowversion for StockGroup:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}

			return this.Ok();
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Model.StockGroupGetDtoV1>> Get([Range(1, int.MaxValue, ErrorMessage = "Stock Group id must greater than 0")] int id)
		{
			Model.StockGroupGetDtoV1 response = null;

			try
			{
				using (SqlConnection db = new SqlConnection(this.connectionString))
				{
					response = await db.QuerySingleOrDefaultAsync<Model.StockGroupGetDtoV1>(sql: @"SELECT [StockGroupID] as ""ID"", [Version], [StockGroupName] as ""Name"" FROM [Warehouse].[StockGroups] WHERE [StockGroupID]=@id", param: new { id }, commandType: CommandType.Text);

					if (response == null)
					{
						logger.LogInformation("StockItem:{0} not found", id);

						return this.NotFound($"StockItem:{id} not found");
					}

					this.HttpContext.Response.Headers.Add("ETag", Convert.ToBase64String(response.Version));
				}
			}
			catch (SqlException ex)
			{
				logger.LogError(ex, "Error retrieving StockGroup:{0}", id);

				return this.StatusCode(StatusCodes.Status500InternalServerError);
			}
			return this.Ok(response);
		}
	}
}
