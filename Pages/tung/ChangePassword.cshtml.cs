using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using tung;

namespace LibararyWebApplication.Pages
{
    public class ChangePasswordModel : PageModel
    {
        public readonly HttpClient client;
        public ChangePasswordModel(HttpClient client)
        {
            this.client = client;
        }
        public async Task<IActionResult> OnGet()
        {
            var existing_token = Utils.get_access_token(Request);
            if (string.IsNullOrWhiteSpace(existing_token))
            {
                return Redirect($"/login?return_url={System.Web.HttpUtility.UrlEncode(HttpContext.Request.Path)}");
            }

            //var user = Utils.get_user_from_token(existing_token, client);
            string request_url = $"{Request.Scheme}://{Request.Host}/api/users/fromtoken";
            return Page();
        }
    }
}
