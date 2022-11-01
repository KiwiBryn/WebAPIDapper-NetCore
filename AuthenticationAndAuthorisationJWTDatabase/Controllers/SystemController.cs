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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase.Controllers
{
    using System.Reflection;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// WebAPI controller for handling System Dapper functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        /// <summary>
        /// Returns the Application version in [Major].[Minor].[Build].Revision] format.
        /// </summary>
        /// <response code="200">List of claims returned.</response>
        /// <response code="401">Unauthorised, bearer token missing or expired.</response>
        /// <returns>Returns the Application version in [Major].[Minor].[Build].Revision] format.</returns>
        [HttpGet("DeploymentVersion"), Authorize]
        public string DeploymentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
