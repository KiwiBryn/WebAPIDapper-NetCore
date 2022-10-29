//---------------------------------------------------------------------------------
// Copyright (c) October 2022, devMobile Software
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
namespace devMobile.WebAPIDapper.Swagger.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.SqlClient;
    using System.Data;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    using devMobile.Azure.DapperTransient;

    /// <summary>
    /// WebAPI controller with functionality for managing People.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly string connectionString;
        private readonly ILogger<PersonController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonController"/> class.
        /// </summary>
        public PersonController(IConfiguration configuration, ILogger<PersonController> logger)
        {
            this.connectionString = configuration.GetConnectionString("WorldWideImportersDatabase");
            this.logger = logger;
        }

        /// <summary>
        /// Resets the specified user's password (System administrator)
        /// </summary>
        /// <param name="personId">Person whose password will be reset</param>
        /// <param name="request">New password</param>
        /// <response code="200">Password reset.</response>
        /// <response code="401">Unauthorised, bearer token missing or expired.</response>
        /// <response code="409">Specified Person not found</response> 
        [Authorize()]
        [HttpPut("{personId:int}", Name = "PasswordReset")]
        public async Task<ActionResult> PasswordReset([Range(1, int.MaxValue, ErrorMessage = "Person id must greater than 0")] int personId, [FromBody] Model.PersonPasswordResetRequest request)
        {
            request.UserId = HttpContext.PersonId();
            request.PersonID = personId;

            using (SqlConnection db = new SqlConnection(connectionString))
            {
                if (await db.ExecuteWithRetryAsync("[WebSite].[PersonPasswordChangeV1]", param: request, commandType: CommandType.StoredProcedure) != 1)
                {
                    logger.LogWarning("Person {0} password change failed", request.PersonID);

                    return this.Conflict();
                }
            }

            return this.Ok();
        }

        /// <summary>
        /// Changes current user's password.
        /// </summary>
        /// <param name="request">Current password and new password</param>
        /// <response code="200">Password changed.</response>
        /// <response code="401">Unauthorised, bearer token missing or expired.</response>
        /// <response code="409">Previous password invalid.</response>
        [Authorize()]
        [HttpPut(Name = "PasswordChange")]
        public async Task<ActionResult> PasswordChange([FromBody] Model.PersonPasswordChangeRequest request)
        {
            request.UserID = HttpContext.PersonId();

            using (SqlConnection db = new SqlConnection(connectionString))
            {
                if (1 != await db.ExecuteWithRetryAsync("[WebSite].[PersonPasswordChangeV1]", param: request, commandType: CommandType.StoredProcedure))
                {
                    logger.LogWarning("Person {0} password change failed", request.UserID);

                    return this.Conflict();
                }
            }

            return this.Ok();
        }
    }
}