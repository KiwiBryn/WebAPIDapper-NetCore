//---------------------------------------------------------------------------------
// Copyright (c) July  2023, devMobile Software
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
// https://www.twilio.com/blog/how-to-send-asp-net-core-identity-emails-with-twilio-sendgrid
//
//---------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

using devMobile.AspNetCore.Identity.Dapper.Models;

namespace devMobile.AspNetCore.Identity.Dapper.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridSettings sendGridSettings;
        private readonly ILogger logger;

        public SendGridEmailSender(IOptions<SendGridSettings> sendGridSettings, ILogger<SendGridEmailSender> logger)
        {
            this.sendGridSettings = sendGridSettings.Value;
            this.logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            string apiKey = sendGridSettings.ApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("The SendGrid ApiKey is not configured");
            }

            string addressFrom = sendGridSettings.AddressFrom;
            if (string.IsNullOrEmpty(addressFrom))
            {
                throw new Exception("The SendGrid AddressFrom is not configured");
            }

            string webSiteFrom = sendGridSettings.WebSiteFrom;
            if (string.IsNullOrEmpty(webSiteFrom))
            {
                throw new Exception("The SendGrid WebSiteFrom is not configured");
            }

            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(addressFrom, webSiteFrom),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(toEmail));

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("SendGrid send failed {StatusCode}", response.StatusCode);
            }

            logger.LogInformation("SendGrid Email queued successfully");
        }
    }
}

