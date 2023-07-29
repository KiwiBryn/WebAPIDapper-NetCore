﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace devMobile.AspNetCore.Identity.Dapper.Pages
{
    [Authorize(Roles = "Role3")]
    public class Role3Model : PageModel
    {
        private readonly ILogger<Role3Model> _logger;

        public Role3Model(ILogger<Role3Model> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}