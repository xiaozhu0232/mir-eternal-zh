namespace Newtonsoft.Json.Bson;

internal class BsonBoolean : BsonValue
{
	public static readonly BsonBoolean False = new BsonBoolean(value: false);

	public static readonly BsonBoolean True = new BsonBoolean(value: true);

	private BsonBoolean(bool value)
		: base(value, BsonType.Boolean)
	{
	}
}
