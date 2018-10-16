using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using Scraper.Application.Configuration;
using Scraper.Contract.Models;
using Scraper.Contract.Repositories;
using Scraper.Contract.Services;

namespace Scraper.Application.Services
{
    public class ScraperMongoService : IScraperService
    {
        public ScraperMongoService(
            ShowRepositoryConfiguration showRepositoryConfiguration,
            ScraperServiceConfiguration scraperServiceConfiguration,
            IShowRepository repository,
            IRestClient httpClient,
            IDeserializer deserializer)
        {
            this.showRepositoryConfiguration = showRepositoryConfiguration;
            this.scraperServiceConfiguration = scraperServiceConfiguration;
            this.repository = repository;
            this.httpClient = httpClient;
            this.deserializer = deserializer;
        }

        private ShowRepositoryConfiguration showRepositoryConfiguration { get; }

        private ScraperServiceConfiguration scraperServiceConfiguration { get; }

        private IShowRepository repository { get; }

        private IRestClient httpClient { get; }

        private IDeserializer deserializer { get; }

        public async Task ScrapeShowsToDatabaseAsync()
        {
            var collection = await repository.CreateCollectionAsync();
            var pageNumber = 0;
            while (true)
            {
                var shows = await GetShowsByPageId(pageNumber);
                if (shows == null)
                {
                    break;
                }
                await repository.InsertShowsAsync(collection, shows);
                pageNumber++;
            }
        }

        private async Task<IList<Show>> GetShowsByPageId(int pageId)
        {
            var request = CreateShowsRequest(pageId);
            var response = await SendRequest(request);
            var shows = DeserializePageOfShows(response);
            await EnrichShowsWithCast(shows);
            return shows;
        }

        private IList<Show> DeserializePageOfShows(IRestResponse queryResult)
        {
            var shows = deserializer.Deserialize<List<Show>>(queryResult);
            return shows;
        }

        private Actor DeserializeCast(string queryResult)
        {
            var actors = JsonConvert.DeserializeObject<Actor>(queryResult);
            return actors;
        }

        private async Task EnrichShowsWithCast(IList<Show> shows)
        {
            foreach (var show in shows)
            {
                await EnrichShowWithCast(show);
            }
        }

        private async Task EnrichShowWithCast(Show show)
        {
            show.Cast = (await GetActorsByShowId(show.Id)).ToList();
        }

        private async Task<IEnumerable<Actor>> GetActorsByShowId(int showId)
        {
            var request = CreateCastRequest(showId);
            var response = await SendRequest(request);
            return DeserializeActors(response);
        }

        private async Task<IRestResponse> SendRequest(RestRequest request)
        {
            var response = await httpClient.ExecuteTaskAsync(request);
            switch (response.StatusCode)
            {
                case HttpStatusCode.TooManyRequests:
                    await Task.Delay(scraperServiceConfiguration.WaitIfToManyRequestMilliseconds);
                    return await SendRequest(request);
                case HttpStatusCode.NotFound:
                    return null;
            }
            return response;
        }

        private List<Actor> DeserializeActors(IRestResponse response)
        {
            var actors = new List<Actor>();
            var cast = deserializer.Deserialize<List<Dictionary<string, string>>>(response);
            foreach (var actor in cast)
            {
                if (actor.ContainsKey("person"))
                {
                    actors.Add(DeserializeCast(actor["person"]));
                }
            }
            return actors;
        }

        private RestRequest CreateShowsRequest(int pageId)
        {
            var request = new RestRequest("shows?page={pageId}", Method.GET);
            request.AddParameter("pageId", pageId, ParameterType.UrlSegment);
            SetJsonContentType(request);
            return request;
        }

        private RestRequest CreateCastRequest(int showId)
        {
            var request = new RestRequest("shows/{showId}/cast", Method.GET);
            request.AddParameter("showId", showId, ParameterType.UrlSegment);
            SetJsonContentType(request);
            return request;
        }

        private void SetJsonContentType(RestRequest request)
        {
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
        }
    }
}