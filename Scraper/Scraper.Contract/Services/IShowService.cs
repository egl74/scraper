using System.Collections.Generic;
using System.Threading.Tasks;
using Scraper.Contract.Models;

namespace Scraper.Contract.Services
{
    public interface IShowService
    {
        Task<Show> GetAsync(int id);

        Task<List<Show>> GetShowPageAsync(int pageId);
    }
}
