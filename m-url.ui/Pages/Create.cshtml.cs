using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Net.Http;
using M_url.Models;

namespace M_url.Ui.Pages
{
    [ValidateAntiForgeryToken]
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(ILogger<CreateModel> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public SlugDto SlugModel { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            //try
            //{
                HttpClient apiClient = _httpClientFactory.CreateClient();
                
                var response = await apiClient.PostAsync(_configuration["ApiUrl"], new StringContent(
                    string.Format("{0} \"url\": \"{1}\" {2}", "{", SlugModel.Url, "}"),
                    System.Text.Encoding.UTF8,
                    "application/json"));

                //if (!response.IsSuccessStatusCode)
                //    return View("Error", new ErrorViewModel { RequestId = response.StatusCode.ToString() });

                var content = await response.Content.ReadAsStringAsync();
                var x = JsonSerializer.Deserialize<SlugDto>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                SlugModel.Slug = x.Slug;

                return Page();
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(string.Format($"The following error occurred {e.Message}"));
            //    //return View("Error", new ErrorViewModel { RequestId = e.Message });
            //}
            //return Page();
        }
    }
}