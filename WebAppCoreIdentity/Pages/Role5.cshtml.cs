using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace devMobile.WebAPIDapper.WebAppCoreIdentity.Pages
{
    [Authorize(Roles = "Role5")]
    public class Role5Model : PageModel
    {
        private readonly ILogger<Role5Model> _logger;

        public Role5Model(ILogger<Role5Model> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}