using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GameToken.Repositories;

internal class MongoToken
{
	[BsonId]
	public string Id { get; set; } = default!;

	[BsonElement("player_code")]
	[BsonRepresentation(BsonType.String)]
	public string PlayerCode { get; set; } = default!;

	[BsonElement("status")]
	[BsonDefaultValue(1)]
	public int Status { get; set; }

	[BsonElement("created_on")]
	[BsonRepresentation(BsonType.String)]
	public DateTime CreatedOn { get; set; }
}
