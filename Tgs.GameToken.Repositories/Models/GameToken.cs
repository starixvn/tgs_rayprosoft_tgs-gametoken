using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GameToken.Repositories.Models;

[Table("game_tokens")]
public record GameToken
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
	public DateTime CreatedOn { get; set; } = default!;
	/// <summary>
	/// The expire on
	/// </summary>
	[BsonElement("expire_on")]
	public DateTime ExpireOn { get; init; } = default!;
}
