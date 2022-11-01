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
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using devMobile.Azure.DapperTransient;

    /// <summary>
    /// WebAPI controller for handling Authentication functionality.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthenticationController> logger;
        private readonly Models.JwtIssuerOptions jwtIssuerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger, IOptions<Models.JwtIssuerOptions> jwtIssuerOptions)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.jwtIssuerOptions = jwtIssuerOptions.Value;
        }

        [HttpPost("logon")]
        public async Task<ActionResult> Logon([FromBody] Models.LogonRequest request )
        {
            var claims = new List<Claim>();

            PersonAuthenticateLogonDetailsDto userLogonUserDetails;
            IEnumerable<string> permissions;

            using (SqlConnection db = new SqlConnection(configuration.GetConnectionString("WorldWideImportersDatabase")))
            {
                userLogonUserDetails = await db.QuerySingleOrDefaultWithRetryAsync<PersonAuthenticateLogonDetailsDto>("[Website].[PersonAuthenticateLookupByLogonNameV2]", param: request, commandType: CommandType.StoredProcedure);
                if (userLogonUserDetails == null)
                {
                    logger.LogWarning("Login attempt by user {0} failed", request.LogonName);

                    return this.Unauthorized();
                }

                // Lookup the Person's permissions
                permissions = await db.QueryWithRetryAsync<string>("[Website].[PersonPermissionsByPersonIdV1]", new { userLogonUserDetails.PersonID }, commandType: CommandType.StoredProcedure);
            }

            // Setup the primary SID + name info
            claims.Add(new Claim(ClaimTypes.PrimarySid, userLogonUserDetails.PersonID.ToString()));
            if (userLogonUserDetails.IsSystemUser)
            {
                claims.Add(new Claim(ClaimTypes.Role, "SystemUser"));
            }
            if (userLogonUserDetails.IsEmployee)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Employee"));
            }
            if (userLogonUserDetails.IsSalesPerson)
            {
                claims.Add(new Claim(ClaimTypes.Role, "SalesPerson"));
            }

            foreach(string permission in permissions)
            {
               claims.Add(new Claim(ClaimTypes.Role, permission));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtIssuerOptions.SecretKey));

            var token = new JwtSecurityToken(
                  issuer: jwtIssuerOptions.Issuer,
                  audience: jwtIssuerOptions.Audience, 
                  expires: DateTime.UtcNow.Add(jwtIssuerOptions.TokenExpiresAfter),
                  claims: claims,
                  signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
                
            return this.Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
            });
        }

        private class PersonAuthenticateLogonDetailsDto
        {
            public int PersonID { get; set; }

            public string FullName { get; set; }

            public string EmailAddress { get; set; }

            public bool IsSystemUser { get; set; }

            public bool IsEmployee { get; set; }

            public bool IsSalesPerson { get; set; }
        }
    }
}
