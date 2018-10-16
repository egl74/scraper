using MongoDB.Bson.Serialization.Attributes;

namespace Scraper.Contract.Models
{
    [BsonDiscriminator("actor")]
    public class Actor
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("birthday")]
        public string Birthday { get; set; }
    }
}
