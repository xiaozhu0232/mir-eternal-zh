using System.Reflection;

internal class AssemblyInfo
{
	private static string version = null;

	public static string Version
	{
		get
		{
			if (version == null)
			{
				version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				if (version == null)
				{
					version = string.Empty;
				}
			}
			return version;
		}
	}
}
