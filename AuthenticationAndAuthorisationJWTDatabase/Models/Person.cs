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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase.Models
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class PersonPasswordResetRequest
    {
        [JsonIgnore]
        public int UserId { get; set; }

        /// <summary>
        /// The personId of the user whose password will be reset.
        /// </summary>
        [JsonIgnore]
        public int PersonID { get; set; }

        /// <summary>
        /// The new password for the specified user.
        /// </summary>
        [JsonRequired]
        [MinLength(Constants.PasswordLengthMinimum)]
        [MaxLength(Constants.PasswordLengthMaximum)]
        public string Password { get; set; }
    }

    public class PersonPasswordChangeRequest
    {
        [JsonIgnore]
        public int UserID { get; set; }

        /// <summary>
        /// Current user's old password.
        /// </summary>
        [MinLength(Constants.PasswordLengthMinimum)]
        [MaxLength(Constants.PasswordLengthMaximum)]
        [JsonProperty(PropertyName="OldPassword")]
        public string PasswordOld { get; set; }

        /// <summary>
        /// Current user's new password.
        /// </summary>
        [MinLength(Constants.PasswordLengthMinimum)]
        [MaxLength(Constants.PasswordLengthMaximum)]
        [JsonProperty(PropertyName = "NewPassword")]
        public string PasswordNew { get; set; }
    }
}