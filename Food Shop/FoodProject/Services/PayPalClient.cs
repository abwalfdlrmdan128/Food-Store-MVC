using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodProject.Services
{
    public class PayPalClient
    {
        public string Mode { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }

        public string BaseUrl => Mode == "Live" ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";

        public PayPalClient(string clientId, string clientSecret, string mode)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Mode = mode;
        }

        private async Task<AuthResponse> Authenticate()
        {
            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{BaseUrl}/v1/oauth2/token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Authorization", $"Basic {auth}" }
                },
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                })
            };

            var client = new HttpClient();
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AuthResponse>(json)!;
        }

        public async Task<CreateOrderResponse> CreateOrder(string value, string currency, string reference)
        {
            var auth = await Authenticate();

            var request = new CreateOrderRequest
            {
                intent = "CAPTURE",
                purchase_units = new List<PurchaseUnit>
                {
                    new PurchaseUnit
                    {
                        reference_id = reference,
                        amount = new Amount
                        {
                            currency_code = currency,
                            value = value
                        }
                    }
                }
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse($"Bearer {auth.access_token}");

            var response = await client.PostAsJsonAsync(
                $"{BaseUrl}/v2/checkout/orders", request);

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CreateOrderResponse>(json)!;
        }

        public async Task<CaptureOrderResponse> CaptureOrder(string orderId)
        {
            var auth = await Authenticate();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse($"Bearer {auth.access_token}");

            var response = await client.PostAsync(
                $"{BaseUrl}/v2/checkout/orders/{orderId}/capture",
                new StringContent("", Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CaptureOrderResponse>(json)!;
        }
    }

    // ===== DTOs =====
    public class AuthResponse { public string access_token { get; set; } }

    public class CreateOrderRequest
    {
        public string intent { get; set; }
        public List<PurchaseUnit> purchase_units { get; set; }
    }

    public class CreateOrderResponse
    {
        public string id { get; set; }
        public string status { get; set; }
    }

    public class CaptureOrderResponse
    {
        public string id { get; set; }
        public string status { get; set; }
    }

    public class PurchaseUnit
    {
        public string reference_id { get; set; }
        public Amount amount { get; set; }
    }

    public class Amount
    {
        public string currency_code { get; set; }
        public string value { get; set; }
    }
}