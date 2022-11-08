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

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    using Newtonsoft.Json;

    /// <summary>
    /// DTO for returning summarised list of stock item information.
    /// </summary>
    public class CustomerListDtoV1
    {
        /// <summary>
        /// Numeric ID used for reference to a stock item within the database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of a stock item (but not a full description).
        /// </summary>
        public string Name { get; set; }

        public bool IsOnCreditHold { get; set; }
    }

    public class CustomerNameSearchDtoV1
    {
        [BindNever]
        public int userId { get; set; }

        /// <summary>
        /// Text in stock item name to search for.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "The name search text must be at least {1} characters long"), MaxLength(Constants.CustomerNameLengthMaximum, ErrorMessage = "The name search text must be no more that {1} characters long")]
        public string SearchText { get; set; }

        /// <summary>
        /// Maximum number of search result to return for a query.
        /// </summary>
        [Required]
        [Range(1, Constants.CustomerSearchMaximumRowsToReturn, ErrorMessage = "The maximum number of rows to return must be greater than or equal to {1} and less then or equal {2}")]
        public int MaximumRowsToReturn { get; set; }
    }
}
