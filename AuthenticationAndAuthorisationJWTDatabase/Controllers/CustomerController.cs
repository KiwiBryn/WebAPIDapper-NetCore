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
    using System.Security.Claims;
    using System.Threading.Tasks;
    
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using devMobile.Azure.DapperTransient;

    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<CustomerController> logger;

        public CustomerController(IConfiguration configuration, ILogger<CustomerController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
            this.logger = logger;
        }

        [HttpGet("Search"), Authorize(Roles = "SalesPerson,SalesAdministrator")]
        public async Task<ActionResult<IAsyncEnumerable<Models.CustomerListDtoV1>>> Get([FromQuery] Models.CustomerNameSearchDtoV1 request)
        {
            IEnumerable<Models.CustomerListDtoV1> response;

            request.userId = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
               response = await db.QueryWithRetryAsync<Models.CustomerListDtoV1>(sql: "[Sales].[CustomersNameSearchV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("Customer search UserId:{0} with {1} nothing found", request.userId, request.SearchText);
            }

            return this.Ok(response);
        }

        [HttpGet("SearchAll"), Authorize(Roles = "Aministrator,SalesAdministrator")]
        public async Task<ActionResult<IAsyncEnumerable<Models.CustomerNameSearchDtoV1>>> Getll([FromQuery] Models.CustomerNameSearchDtoV1 request)
        {
            IEnumerable<Models.CustomerListDtoV1> response;

            request.userId = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(this.connectionString))
            {
                response = await db.QueryWithRetryAsync<Models.CustomerListDtoV1>(sql: "[Sales].[CustomersNameSearchAllV1]", param: request, commandType: CommandType.StoredProcedure);
            }

            if (!response.Any())
            {
                logger.LogInformation("Customer search UserId:{0} with {1} nothing found", request.userId, request.SearchText);
            }

            return this.Ok(response);
        }
    }
}
