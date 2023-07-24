using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace devMobile.WebAPIDapper.WebAppCoreIdentity.Pages
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