using System.IO;
using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip;

public class ZipNameTransform : INameTransform
{
	private string trimPrefix;

	private bool relativePath;

	public string TrimPrefix
	{
		get
		{
			return trimPrefix;
		}
		set
		{
			trimPrefix = value;
		}
	}

	public ZipNameTransform()
	{
		relativePath = true;
	}

	public ZipNameTransform(bool useRelativePaths)
	{
		relativePath = useRelativePaths;
	}

	public ZipNameTransform(bool useRelativePaths, string trimPrefix)
	{
		this.trimPrefix = trimPrefix;
		relativePath = useRelativePaths;
	}

	public string TransformDirectory(string name)
	{
		name = TransformFile(name);
		if (name.Length > 0)
		{
			if (!name.EndsWith("/"))
			{
				name += "/";
			}
		}
		else
		{
			name = "/";
		}
		return name;
	}

	public string TransformFile(string name)
	{
		if (name != null)
		{
			if (trimPrefix != null && name.IndexOf(trimPrefix) == 0)
			{
				name = name.Substring(trimPrefix.Length);
			}
			if (Path.IsPathRooted(name))
			{
				name = name.Substring(Path.GetPathRoot(name).Length);
			}
			if (relativePath)
			{
				if (name.Length > 0 && (name[0] == Path.AltDirectorySeparatorChar || name[0] == Path.DirectorySeparatorChar))
				{
					name = name.Remove(0, 1);
				}
			}
			else if (name.Length > 0 && name[0] != Path.AltDirectorySeparatorChar && name[0] != Path.DirectorySeparatorChar)
			{
				name = name.Insert(0, "/");
			}
			name = name.Replace("\\", "/");
		}
		else
		{
			name = "";
		}
		return name;
	}
}
