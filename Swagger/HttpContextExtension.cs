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
namespace devMobile.WebAPIDapper.Swagger
{
    using System.Security.Claims;

    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Helper extension methods for extracting clains HttpContext.
    /// </summary>
    public static class HttpContextExtension
    {
        /// <summary>
        /// Extracts PeronID  from PrimarySid in httpContext.
        /// </summary>
        /// <param name="httpContext">HTTP request context which has PrimarySid .</param>
        /// <returns>Swagger PersonId UserID.</returns>
        public static long PersonId(this HttpContext httpContext)
        {
            return long.Parse(httpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
        }
    }
}