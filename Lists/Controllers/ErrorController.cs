//---------------------------------------------------------------------------------
// Copyright (c) June 2021, devMobile Software
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
namespace ListsErrorPages.Controllers
{
	using Microsoft.AspNetCore.Diagnostics;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Hosting;

	[ApiController]
	public class ErrorController : Controller
	{
		[Route("/error")]
		public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
		{
			var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;

			return Problem("Something has gone wrong call the help desk on 0800-RebootIT", statusCode:exceptionHandlerFeature.Error.HResult, title:"Webpage has died");
		}
	}
}