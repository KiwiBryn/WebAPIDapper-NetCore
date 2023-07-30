//---------------------------------------------------------------------------------
// Copyright (c) July  2023, devMobile Software
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
using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

namespace devMobile.AspNetCore.Identity.Dapper.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
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
