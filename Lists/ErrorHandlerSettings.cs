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
namespace devMobile.WebAPIDapper.Lists
{
	using System.Collections.Generic;

	public class UrlSpecificSetting
	{
		public string Title { get; set; } = "";
		
		public string Detail { get; set; } = "";

		public UrlSpecificSetting()
		{
		}

		public UrlSpecificSetting(string title, string detail)
		{
			this.Title = title;
			this.Detail = detail;
		}
	}

	public class ErrorHandlerSettings
	{
		public string Title { get; set; } = "System Error";

		public string Detail { get; set; } = "devMobile Lists Classic API failure";

		public Dictionary<string, UrlSpecificSetting> UrlSpecificSettings { get; set; }

		public ErrorHandlerSettings()
		{
		}

		public ErrorHandlerSettings(string title, string detail, Dictionary<string, UrlSpecificSetting> urlSpecificSettings )
		{
			Title = title;

			Detail = detail;

			UrlSpecificSettings = urlSpecificSettings;
		}
	}
}
