using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using StellarBlueAssignment.Models;
using StellarBlueAssignment.Data;
using Microsoft.Extensions.Options;

namespace StellarBlueAssignment.Services
{
    public class EnergyDataService{
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private AuthTokenResponse? _tokenData; 

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { 
            PropertyNameCaseInsensitive = true 
        };

        private readonly EnergyOptions _apiOptions;

        public EnergyDataService(IHttpClientFactory httpClientFactory, AppDbContext dbContext, IOptions<EnergyOptions> apiOptions){
            _httpClient = httpClientFactory.CreateClient("StellarBlueApi");
            _dbContext = dbContext;
            _apiOptions = apiOptions.Value; 
        }

        private async Task<string> GetBearerTokenAsync(){
            if (_tokenData != null && _tokenData.ExpirationTime > DateTime.UtcNow.AddMinutes(5)) {
                return _tokenData.Token!;
            }

            var authPayload = new{
                grant_type = "password",
                username = _apiOptions.Username,
                password = _apiOptions.Password
            };


            var jsonContent = JsonSerializer.Serialize(authPayload);


            using var jsonStringContent = new StringContent(
                jsonContent, 
                System.Text.Encoding.UTF8, 
                "application/json"
            );

            var response = await _httpClient.PostAsync("token", jsonStringContent);
            
            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode){
                throw new HttpRequestException($"Authentication failed with status code {response.StatusCode}. API response body: {jsonResponse}");
            }
            
            var deserializedToken = JsonSerializer.Deserialize<AuthTokenResponse>(
                jsonResponse,
                JsonOptions 
            );

            if (deserializedToken == null || string.IsNullOrEmpty(deserializedToken.Token)){
                throw new InvalidOperationException("Authentication failed: Received null or empty token in response.");
            }

            _tokenData = deserializedToken;
            _tokenData.ExpirationTime = DateTime.UtcNow.AddSeconds(_tokenData.ExpiresIn);

            return _tokenData.Token;
        }

        public async Task<List<McpData>> FetchMcpDataAsync(DateTime dateFrom, DateTime dateTo){
            var token = await GetBearerTokenAsync();

            var actualDateTo = dateTo.Date.AddDays(1);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"MCP?date_from={dateFrom:yyyy-MM-dd}&date_to={actualDateTo:yyyy-MM-dd}");
            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            var dataPoints = JsonSerializer.Deserialize<List<McpData>>(jsonResponse, JsonOptions);

            return dataPoints ?? new List<McpData>(); 
        }


        public async Task<int> ProcessAndStoreAveragesAsync(DateTime dateFrom, DateTime dateTo) {
            var rawData = await FetchMcpDataAsync(dateFrom, dateTo); 
            if (rawData == null || !rawData.Any()) {
                return 0; 
            }

            var dailyAverages = rawData
                .GroupBy(p => p.TimeStamp.Date)
                .Select(g => new EnergyPriceEntity{
                    Date = DateOnly.FromDateTime(g.Key),
                    AveragePrice = Math.Round(g.Average(p => p.Value), 2)})
                .ToList();

            var newRecords = new List<EnergyPriceEntity>();


            foreach (var average in dailyAverages){
                var existingRecord = await _dbContext.DailyEnergyPrices
                    .FirstOrDefaultAsync(d => d.Date == average.Date);

                if (existingRecord == null){
                    newRecords.Add(average);
                } else {
                    existingRecord.AveragePrice = average.AveragePrice;
                }
            }

            await _dbContext.DailyEnergyPrices.AddRangeAsync(newRecords);
            
            var savedCount = await _dbContext.SaveChangesAsync();

            return savedCount;
        }

        public async Task<List<EnergyPriceEntity>> GetAveragesFromDbAsync(DateTime dateFrom, DateTime dateTo) { 
            var startDateOnly = DateOnly.FromDateTime(dateFrom.Date);
            var endDateOnly = DateOnly.FromDateTime(dateTo.Date);
            return await _dbContext.DailyEnergyPrices
                .Where(d => d.Date >= startDateOnly && d.Date <= endDateOnly)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }
    }
}
