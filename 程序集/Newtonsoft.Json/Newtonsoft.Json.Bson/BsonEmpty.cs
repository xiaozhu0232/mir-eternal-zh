namespace Newtonsoft.Json.Bson;

internal class BsonEmpty : BsonToken
{
	public static readonly BsonToken Null = new BsonEmpty(BsonType.Null);

	public static readonly BsonToken Undefined = new BsonEmpty(BsonType.Undefined);

	public override BsonType Type { get; }

	private BsonEmpty(BsonType type)
	{
		Type = type;
	}
}
