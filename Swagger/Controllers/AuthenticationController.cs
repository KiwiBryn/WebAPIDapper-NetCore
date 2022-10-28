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
    using System.Data;
    using System.Data.SqlClient;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using devMobile.Azure.DapperTransient;


    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthenticationController> logger;
        private readonly Model.JwtIssuerOptions jwtIssuerOptions;

        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger, IOptions<Model.JwtIssuerOptions> jwtIssuerOptions)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.jwtIssuerOptions = jwtIssuerOptions.Value;
        }

        [HttpPost("logon")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Logon([FromBody] Model.LogonRequest request )
        {
            var claims = new List<Claim>();

            using (SqlConnection db = new SqlConnection(configuration.GetConnectionString("WorldWideImportersDatabase")))
            {
                PersonAuthenticateLogonDetailsDto userLogonUserDetails = await db.QuerySingleOrDefaultWithRetryAsync<PersonAuthenticateLogonDetailsDto>("[Website].[PersonAuthenticateLookupByLogonNameV2]", param: request, commandType: CommandType.StoredProcedure);
                if (userLogonUserDetails == null)
                {
                    logger.LogWarning("Login attempt by user {0} failed", request.LogonName);

                    return this.Unauthorized();
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
