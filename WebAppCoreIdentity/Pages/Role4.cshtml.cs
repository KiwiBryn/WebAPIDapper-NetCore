using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace devMobile.WebAPIDapper.WebAppCoreIdentity.Pages
{
    [Authorize(Roles = "Role4")]
    public class Role4Model : PageModel
    {
        private readonly ILogger<Role4Model> _logger;

        public Role4Model(ILogger<Role4Model> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}