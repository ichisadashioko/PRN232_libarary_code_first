using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibararyWebApplication.Pages
{
    public class ResetPassword : PageModel
    {
        public readonly HttpClient client;
        public ResetPassword(HttpClient client)
        {
            this.client = client;
        }
        public string? ErrorMessage { get; set; }
        public bool IsValidToken = false;
        public string ResetPasswordToken { get; set; } = string.Empty;
        public void OnGet()
        {
            // get token from query string
            var token = Request.Query["token"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Invalid or missing token.";
            }
            else
            {
                // get the base address from current host
                // string encodedToken = Uri.EscapeDataString(token);
                string api_endpoint = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/users/check_password_reset_token";
                // string json_body = new JsonObject
                // {
                //     ["token"] = encodedToken
                // }.ToJsonString();
                Console.WriteLine(api_endpoint);
                var response = client.PostAsync(api_endpoint, JsonContent.Create(new
                {
                    token = token
                })).Result;
                if (response.IsSuccessStatusCode)
                {
                    IsValidToken = true;
                    ResetPasswordToken = token;
                    Console.WriteLine("Token is valid.");
                }
                else
                {
                    ErrorMessage = "Invalid or expired token.";
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    if (response.Content != null)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine($"Content: {content}");
                    }
                }
            }
        }
    }
}
