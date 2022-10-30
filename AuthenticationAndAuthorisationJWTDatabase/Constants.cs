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
namespace devMobile.WebAPIDapper.AuthenticationAndAuthorisationJwtDatabase
{
    /// <summary>
    /// Constants used in the validation of Swagger strings etc.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Minimum length of a person login name.
        /// </summary>
        public const int LogonNameLengthMinimum = 3;

        /// <summary>
        /// Maximum length of a person login name.
        /// </summary>
        public const int LogonNameLengthMaximum = 50;

        /// <summary>
        /// Minimum length of a person's password.
        /// </summary>
        public const int PasswordLengthMinimum = 8;

        /// <summary>
        /// Maximum length of a person's password.
        /// </summary>
        public const int PasswordLengthMaximum = 40;
    }
}
