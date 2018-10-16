using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Scraper.Application.Configuration;
using Scraper.Contract.Models;
using Scraper.Contract.Repositories;

namespace Scraper.DAL.Repositories
{
    public class ShowMongoRepository : IShowRepository
    {
        public ShowMongoRepository(ShowRepositoryConfiguration configuration, IMongoClient dbClient)
        {
            this.configuration = configuration;
            this.dbClient = dbClient;
        }

        private ShowRepositoryConfiguration configuration { get; }

        public IMongoClient dbClient { get; }

        public async Task<Show> GetAsync(int id)
        {
            var db = dbClient.GetDatabase(configuration.DatabaseName);
            var collection = db.GetCollection<Show>(configuration.CollectionName);
            var result = await collection.FindAsync(x => x.Id == id);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<List<Show>> GetShowPageAsync(int pageId)
        {
            var db = dbClient.GetDatabase(configuration.DatabaseName);
            var collection = db.GetCollection<Show>(configuration.CollectionName);
            return await collection.Find(x => true).Skip(pageId * configuration.PageSize).Limit(configuration.PageSize).ToListAsync();
        }

        public async Task<IMongoCollection<Show>> CreateCollectionAsync()
        {
            var db = dbClient.GetDatabase(configuration.DatabaseName);
            var cursor = await db.ListCollectionNamesAsync();
            var collectionNames = cursor.ToList();
            if (!collectionNames.Contains(configuration.CollectionName))
            {
                await db.CreateCollectionAsync(configuration.CollectionName);
            }
            return db.GetCollection<Show>(configuration.CollectionName);
        }

        public async Task InsertShowsAsync(IMongoCollection<Show> collection, IList<Show> shows)
        {
            try
            {
                await collection.InsertManyAsync(shows);
            }
            catch (MongoBulkWriteException e)
            {
                var errors = e.WriteErrors;
                foreach (var error in errors)
                {
                    var show = shows[error.Index];
                    await collection.ReplaceOneAsync(item => item.Id == show.Id, show);
                }
            }
        }
    }
}
