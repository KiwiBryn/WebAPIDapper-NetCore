﻿//---------------------------------------------------------------------------------
// Copyright (c) June 2022, devMobile Software
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
namespace devMobile.WebAPIDapper.Basics.Controllers
{
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Options;


	[ApiController]
	public class ErrorController : Controller
	{
		private readonly ErrorHandlerSettings errorHandlerSettings;

		public ErrorController(IOptions<ErrorHandlerSettings> errorHandlerSettings)
		{
			this.errorHandlerSettings = errorHandlerSettings.Value;
		}

		[Route("/error")]
		public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
		{
			if (!this.errorHandlerSettings.UrlSpecificSettings.ContainsKey(this.Request.Host.Host))
			{
				return Problem(detail: errorHandlerSettings.Detail, title: errorHandlerSettings.Title);
			}

			return Problem(errorHandlerSettings.UrlSpecificSettings[this.Request.Host.Host].Title, errorHandlerSettings.UrlSpecificSettings[this.Request.Host.Host].Detail);
		}
	}
}