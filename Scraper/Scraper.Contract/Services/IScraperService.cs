using System.Threading.Tasks;

namespace Scraper.Contract.Services
{
    public interface IScraperService
    {
        Task ScrapeShowsToDatabaseAsync();
    }
}
