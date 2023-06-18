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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase.Controllers
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using devMobile.Azure.DapperTransient;

    /// <summary>
    /// WebAPI controller for handling Customer functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class CustomerController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<CustomerController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        public CustomerController(IConfiguration configuration, ILogger<CustomerController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
            this.logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <response code="200">logon successful.</response>
        ///	<response code="401">Invalid LogonName or password.</response>
        /// <returns></returns>
        [HttpGet(), Authorize(Roles = "SalesPerson,SalesAdministrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Models.CustomerListDtoV1>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IAsyncEnumerable<Models.CustomerListDtoV1>>> Get()
        {
            IEnumerable<Models.CustomerListDtoV1> response;

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.CustomerListDtoV1>(sql: "[Sales].[CustomersListV1]", param: new { userId = HttpContext.PersonId() }, commandType: CommandType.StoredProcedure);
            }

            return this.Ok(response);
        }

        [HttpGet("SearchUnion"), Authorize(Roles = "SalesPerson,SalesAdministrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Models.CustomerListDtoV1>))]
        public async Task<ActionResult<IAsyncEnumerable<Models.CustomerListDtoV1>>> GetUnion([FromQuery] Models.CustomerNameSearchDtoV1 request)
        {
            IEnumerable<Models.CustomerListDtoV1> response;

            request.userId = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.CustomerListDtoV1>(sql: "[Sales].[CustomersNameSearchUnionV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("Customer search Union UserId:{userId} with {SearchText} nothing found", request.userId, request.SearchText);
            }

            return this.Ok(response);
        }

        [HttpGet("SearchView"), Authorize(Roles = "SalesPerson,SalesAdministrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Models.CustomerListDtoV1>))]
        public async Task<ActionResult<IAsyncEnumerable<Models.CustomerListDtoV1>>>GetView([FromQuery] Models.CustomerNameSearchDtoV1 request)
        {
            IEnumerable<Models.CustomerListDtoV1> response;

            request.userId = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.CustomerListDtoV1>(sql: "[Sales].[CustomersNameSearchViewV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("Customer search View UserId:{userId} with {SearchText} nothing found", request.userId, request.SearchText);
            }

            return this.Ok(response);
        }

        [HttpGet("SearchAll"), Authorize(Roles = "Administrator,SalesAdministrator")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Models.CustomerListDtoV1>))]
        public async Task<ActionResult<IAsyncEnumerable<Models.CustomerNameSearchDtoV1>>> GetAll([FromQuery] Models.CustomerNameSearchDtoV1 request)
        {
            IEnumerable<Models.CustomerListDtoV1> response;

            request.userId = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.CustomerListDtoV1>(sql: "[Sales].[CustomersNameSearchAllV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("Customer search UserId:{userId} with {SearchText} nothing found", request.userId, request.SearchText);
            }

            return this.Ok(response);
        }

        [HttpPut("{customerId}/CreditStatus", Name ="CreditHold")]
        [Authorize(Roles = "Administrator,SalesAdministrator,SalesPerson")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CustomerCreditHold(int customerId, [FromBody] Models.CustomerCreditHoldUpdateV1 request )
        {
            request.UserId = HttpContext.PersonId();
            request.CustomerId = customerId;

            using (SqlConnection db = new SqlConnection(connectionString))
            {
                if (await db.ExecuteWithRetryAsync("[Sales].[CustomerCreditHoldStatusUpdateV1]", param: request, commandType: CommandType.StoredProcedure) != 1)
                {
                    logger.LogWarning("Person {UserId} Customer {CustomerId} IsOnCreditHold {IsOnCreditHold} update failed", request.UserId, request.CustomerId, request.IsOnCreditHold);

                    return this.Conflict();
                }
            }

            return this.Ok();
        }
    }
}
