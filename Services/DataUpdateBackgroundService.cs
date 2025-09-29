using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StellarBlueAssignment.Services;

namespace StellarBlueAssignment.Services{

    public class DataUpdateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataUpdateBackgroundService> _logger;

        public DataUpdateBackgroundService(IServiceProvider serviceProvider, ILogger<DataUpdateBackgroundService> logger){
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken){
            _logger.LogInformation("Data Update Service starting.");

            while (!stoppingToken.IsCancellationRequested){
                try{
                    await DoWork(stoppingToken);
                } catch (Exception ex){
                    _logger.LogError(ex, "An error occurred while running the background service.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken stoppingToken){
            using (var scope = _serviceProvider.CreateScope()){
                var service = scope.ServiceProvider.GetRequiredService<EnergyDataService>();

                var dateTo = DateTime.Today.AddDays(-1);
                var dateFrom = dateTo.AddDays(-3);

                _logger.LogInformation("Attempting to update data for range: {From:yyyy-MM-dd} to {To:yyyy-MM-dd}", dateFrom, dateTo);

                var savedRecords = await service.ProcessAndStoreAveragesAsync(dateFrom, dateTo);

                _logger.LogInformation("Data update complete. {Count} records saved/updated.", savedRecords);
            }
        }
    }
}