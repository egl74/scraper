using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Scraper.Contract.Models;

namespace Scraper.Contract.Repositories
{
    public interface IShowRepository
    {
        Task<Show> GetAsync(int id);

        Task<IMongoCollection<Show>> CreateCollectionAsync();

        Task InsertShowsAsync(IMongoCollection<Show> collection, IList<Show> shows);

        Task<List<Show>> GetShowPageAsync(int pageId);
    }
}
