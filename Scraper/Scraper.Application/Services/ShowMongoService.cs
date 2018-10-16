using System.Collections.Generic;
using System.Threading.Tasks;
using Scraper.Application.Configuration;
using Scraper.Contract.Models;
using Scraper.Contract.Repositories;
using Scraper.Contract.Services;

namespace Scraper.Application.Services
{
    public class ShowMongoService : IShowService
    {
        public ShowMongoService(ShowRepositoryConfiguration configuration, IShowRepository repository)
        {
            this.configuration = configuration;
            this.repository = repository;
        }

        private ShowRepositoryConfiguration configuration { get; }

        public IShowRepository repository { get; }

        public Task<Show> GetAsync(int id)
        {
            return repository.GetAsync(id);
        }

        public Task<List<Show>> GetShowPageAsync(int pageId)
        {
            return repository.GetShowPageAsync(pageId);
        }
    }
}
