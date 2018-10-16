using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Scraper.Application.Configuration;
using Scraper.Contract.Services;

namespace Scraper.Application.Services
{
    public class ScraperTaskService : IHostedService, IDisposable
    {
        public ScraperTaskService(IScraperService scraperService, ScraperTaskConfiguration configuration)
        {
            this.scraperService = scraperService;
            this.configuration = configuration;
        }

        private IScraperService scraperService { get; }

        private ScraperTaskConfiguration configuration { get; }

        private Timer _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(
                async e => await scraperService.ScrapeShowsToDatabaseAsync(), 
                null, 
                TimeSpan.Zero, 
                TimeSpan.FromHours(configuration.TaskIntervalHours));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
