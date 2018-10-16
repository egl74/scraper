using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scraper.Contract.Models
{
    [BsonDiscriminator("show")]
    public class Show
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        public IEnumerable<Actor> Cast { get; set; }
    }
}
