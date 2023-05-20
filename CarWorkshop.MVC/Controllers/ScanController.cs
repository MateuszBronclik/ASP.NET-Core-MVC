using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

public class ScanController : Controller
{
    public async Task<ActionResult> Index(string url)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("API-Key", "");
            var content = new StringContent("{\"url\": \"" + url + "\", \"public\": \"on\"}", Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://urlscan.io/api/v1/scan/", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var scanId = result.uuid;

                return RedirectToAction("Results", new { id = scanId });
            }
            else
            {
                // obsługa błędów
            }
        }

        return View();
    }

    public async Task<ActionResult> Results(string id)
    {
        dynamic result;
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("API-Key", "");

            while (true)
            {
                var response = await httpClient.GetAsync($"https://urlscan.io/api/v1/result/{id}/");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    if (result != null)
                    {
                        // Sprawdź, czy wyniki są już dostępne
                        // Zakładając, że "status" to pole w odpowiedzi JSON, które informuje, czy skanowanie jest gotowe
                        if (result.status == "completed")
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // obsługa błędów
                }

                // Odczekaj chwilę przed następnym odpytaniem
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        // Teraz masz wyniki skanowania i możesz je wyświetlić lub zrobić z nimi co chcesz.
        return View(result);
    }
}
