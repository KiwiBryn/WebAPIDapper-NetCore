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
namespace Hedgebook.DealAPI.Controllers
{
    using System.Security.Claims;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// WebAPI controller for handling Authorisation functionality.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AuthorisationController : ControllerBase
    {
        /// <summary>
        /// Gets a list of the current User's roles.
        /// </summary>
        /// <response code="200">List of claims returned.</response>
        /// <response code="401">Unauthorised, bearer token missing or expired.</response>
        /// <returns>list of claims.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
        public List<string> Get()
        {
            List<string> claimNames = new List<string>();

            foreach (var claim in this.User.Claims.Where(c => c.Type == ClaimTypes.Role))
            {
                claimNames.Add(claim.Value);
            }

            return claimNames;
        }
    }
}