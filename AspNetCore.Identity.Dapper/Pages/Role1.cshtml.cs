using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace devMobile.AspNetCore.Identity.Dapper.Pages
{
    [Authorize(Roles = "Role1")]
    public class Role1Model : PageModel
    {
        private readonly ILogger<Role1Model> _logger;

        public Role1Model(ILogger<Role1Model> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}