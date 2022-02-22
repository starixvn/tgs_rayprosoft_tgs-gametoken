using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GameToken.Repositories;

internal class MongoToken
{
	[BsonId]
	public string Id { get; set; } = default!;

	public string PlayerCode { get; set; } = default!;

	public DateTime CreatedDate { get; set; }
}
