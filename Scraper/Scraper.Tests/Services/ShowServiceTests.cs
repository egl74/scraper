using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Scraper.Application.Configuration;
using Scraper.Application.Services;
using Scraper.Contract.Models;
using Scraper.Contract.Services;
using Scraper.DAL.Repositories;
using Xunit;

namespace Scraper.Tests
{
    public class ShowServiceTests
    {
        public ShowServiceTests()
        {

        }

        [Fact]
        public async Task GetAsync_ConfigurationEmpty_ArgumentNullExceptionExpected()
        {
            // Arrange
            var repositoryConfigMock = new Mock<ShowRepositoryConfiguration>();
            var dbClientMock = new Mock<MongoClient>();
            var repository = new ShowMongoRepository(repositoryConfigMock.Object, dbClientMock.Object);
            var target = new ShowMongoService(repositoryConfigMock.Object, repository);
            Func<Task> t = async () => await target.GetAsync(1);

            // Act
            var ex = await Record.ExceptionAsync(t);

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public async Task GetAsync_Successful()
        {
            // Arrange
            var repository = GetTestRepository();
            var target = new ShowMongoService(TestRepositoryConfiguration, repository);
            var collection = await repository.CreateCollectionAsync();
            await repository.InsertShowsAsync(collection, new[] { new Show { Id = 1 } });

            // Act
            var result = await target.GetAsync(1);

            // Assert
            Assert.Equal(1, result.Id);

        }

        private ShowMongoRepository GetTestRepository()
        {

            var dbClientMock = new Mock<MongoClient>();
            var repository = new ShowMongoRepository(TestRepositoryConfiguration, dbClientMock.Object);
            return repository;
        }

        private ShowRepositoryConfiguration TestRepositoryConfiguration =>
            new ShowRepositoryConfiguration
            {

                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "TVShowTest",
                CollectionName = "shows"
            };
    }
}
