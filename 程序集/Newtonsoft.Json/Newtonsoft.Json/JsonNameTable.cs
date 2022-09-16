namespace Newtonsoft.Json;

public abstract class JsonNameTable
{
	public abstract string? Get(char[] key, int start, int length);
}
