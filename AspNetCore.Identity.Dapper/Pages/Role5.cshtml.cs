using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace devMobile.AspNetCore.Identity.Dapper.Pages
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