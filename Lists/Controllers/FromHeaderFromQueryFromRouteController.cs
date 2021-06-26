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
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace devMobile.WebAPIDapper.Lists.Controllers
{
	public class FromQueryClassDto
	{
		public string SearchText { get; set; }

		public int MaximumRowsToReturn { get; set; }
	}

	public class FromQueryClassValidationDto
	{
		[Required]
		[MinLength(3, ErrorMessage = "The name search text must be at least 3 characters long")]
		[MaxLength(5, ErrorMessage = "The name search text must be no more that 5 characters long")]
		public string SearchText { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "MaximumRowsToReturn must be present and greater than 0")]
		public int MaximumRowsToReturn { get; set; }
	}

	public class FromQueryClassResponseDto
	{
		public string SearchText { get; set; }

		public int MaximumRowsToReturn { get; set; }
	}

	[Route("api/[controller]")]
	[ApiController]
	public class FromHeaderFromQueryFromRouteController : ControllerBase
	{
		[HttpGet]
		public ActionResult Get()
		{
			return this.Ok();
		}

		/*
			http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromHeaderValidation
			User-Agent: Fiddler
			Host: localhost:36739
			eTag: USB456
		*/
		[HttpGet("FromHeader")]
		public ActionResult GetHeader([FromHeader(Name = "eTag")]string request)
		{
			if (string.IsNullOrEmpty(request))
			{
				return this.Ok("Request null or empty");
			}
			else
			{
				return this.Ok(request);
			}
		}

		/*
			http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromHeaderValidation
			User-Agent: Fiddler
			Host: localhost:36739
			eTag: USB456
		*/
		[HttpGet("FromHeaderValidation")]
		public ActionResult GetHeaderValidation([FromHeader(Name = "eTag")][MinLength(3, ErrorMessage = "The eTag header text must be at least 3 characters long")][MaxLength(5, ErrorMessage = "eTagMust be not more than 5 characters long")][Required(ErrorMessage ="The eTag field is required")] string request)
		{
			return this.Ok(request);
		}

		//http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromHeaderValidation
		[HttpHead("FromHeaderValidation")]
		public ActionResult HeadHeaderValidation([FromHeader(Name = "eTag")][MinLength(3, ErrorMessage = "The eTag header text must be at least 3 characters long")][MaxLength(5, ErrorMessage = "eTagMust be not more than 5 characters long")][Required(ErrorMessage = "The eTag field is required")] string request)
		{
			return this.Ok(request);
		}


		//http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromQueryValidation?eTag=usb456
		[HttpGet("FromQueryValidation")]
		public ActionResult GetQueryValidation([FromQuery(Name = "eTag")][MinLength(3, ErrorMessage = "The eTag query text must be at least 3 characters long")][MaxLength(5, ErrorMessage = "eTagMust be not more than 5 characters long")][Required(ErrorMessage = "The eTag field is required")] string request)
		{
			return this.Ok(request);
		}

		[HttpHead("FromQueryValidation")]
		public ActionResult HeaderQueryValidation([FromQuery(Name = "eTag")][MinLength(3, ErrorMessage = "The eTag query text must be at least 3 characters long")][MaxLength(5, ErrorMessage = "eTagMust be not more than 5 characters long")][Required(ErrorMessage = "The eTag field is required")] string request)
		{
			return this.Ok(request);
		}


		//http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromQueryClass?SearchText=usb456&MaximumRowsToReturn=10
		[HttpGet("FromQueryClass")]
		public ActionResult<FromQueryClassResponseDto> GetQueryClass([FromQuery] FromQueryClassDto request)
		{
			return this.Ok(new FromQueryClassResponseDto() { SearchText = request.SearchText, MaximumRowsToReturn = request.MaximumRowsToReturn });
		}

		[HttpHead("FromQueryClass")]
		public ActionResult<FromQueryClassResponseDto> HeadQueryClass([FromQuery] FromQueryClassDto request)
		{
			return this.Ok(new FromQueryClassResponseDto() { SearchText = request.SearchText, MaximumRowsToReturn = request.MaximumRowsToReturn });
		}


		//http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromQueryClassValidation?SearchText=usb456&MaximumRowsToReturn=10
		[HttpGet("FromQueryClassValidation")]
		public ActionResult<FromQueryClassResponseDto> GetQueryClassValidation([FromQuery] FromQueryClassValidationDto request)
		{
			return this.Ok(new FromQueryClassResponseDto() { SearchText = request.SearchText, MaximumRowsToReturn = request.MaximumRowsToReturn });
		}

		[HttpHead("FromQueryClassValidation")]
		public ActionResult<FromQueryClassResponseDto> HeadQueryClassValidation([FromQuery] FromQueryClassValidationDto request)
		{
			return this.Ok(new FromQueryClassResponseDto() { SearchText = request.SearchText, MaximumRowsToReturn = request.MaximumRowsToReturn });
		}


		// http://localhost:36739/api/FromHeaderFromQueryFromRoute/FromRoute/1
		[HttpGet("FromRoute/{id}")]
		public ActionResult<int> GetRoute([FromRoute(Name= "id")]int id)
		{
			if (id == 1)
			{
				return this.Ok(id);
			}
			else
			{
				return this.StatusCode(StatusCodes.Status304NotModified);
			}
		}

		[HttpHead("FromRoute/{id}")]
		public ActionResult<int> HeadRoute([FromRoute(Name = "id")] int id)
		{
			if (id == 1)
			{
				return this.Ok(id);
			}
			else
			{
				return this.StatusCode(StatusCodes.Status304NotModified);
			}
		}
	}
}
