using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace LibararyWebApplication.Pages
{
    public class RulesModel : PageModel
    {
        private readonly PrnContext _context;

        private HttpClient httpClient = new HttpClient();
        public RulesModel(PrnContext context, HttpClient httpClient)
        {
            _context = context;
            this.httpClient = httpClient;
        }
        public List<Rule> Rules { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;

        public async Task<IActionResult> OnGetAsync(int? page)
        {
            CurrentPage = page ?? 1;

            string api_endpoint = $"http://{HttpContext.Request.Host.ToString()}";
            var response = await httpClient.GetAsync($"{api_endpoint}/api/Rules");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var allRules = JsonConvert.DeserializeObject<List<Rule>>(json);

                TotalPages = (int)Math.Ceiling(allRules.Count / (double)PageSize);
                Rules = allRules.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            }
            else
            {
                Console.WriteLine($"Lỗi khi gọi API: {response.StatusCode}");
                Rules = new List<Rule>();
                TotalPages = 0;
            }

            return Page();
        }

    }
}
