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
namespace devMobile.WebAPIDapper.Swagger.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class LogonRequest
    {
        [JsonRequired]
        [MinLength(Constants.LogonNameLengthMinimum)]
        [MaxLength(Constants.LogonNameLengthMaximum)]
        public string LogonName { get; set; }

        [JsonRequired]
        [MinLength(Constants.PasswordLengthMinimum)]
        [MaxLength(Constants.PasswordLengthMaximum)]
        public string Password { get; set; }
    }

    public class JwtIssuerOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string SecretKey { get; set; }

        public TimeSpan TokenExpiresAfter { get; set; }
    }
}