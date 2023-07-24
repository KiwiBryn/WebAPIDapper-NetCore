﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace devMobile.WebAPIDapper.WebAppCoreIdentity.Pages
{
    [Authorize(Roles = "Role2")]
    public class Role2Model : PageModel
    {
        private readonly ILogger<Role2Model> _logger;

        public Role2Model(ILogger<Role2Model> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}