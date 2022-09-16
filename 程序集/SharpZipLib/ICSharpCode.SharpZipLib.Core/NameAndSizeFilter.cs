using System.IO;

namespace ICSharpCode.SharpZipLib.Core;

public class NameAndSizeFilter : PathFilter
{
	private long minSize = 0L;

	private long maxSize = long.MaxValue;

	public long MinSize
	{
		get
		{
			return minSize;
		}
		set
		{
			minSize = value;
		}
	}

	public long MaxSize
	{
		get
		{
			return maxSize;
		}
		set
		{
			maxSize = value;
		}
	}

	public NameAndSizeFilter(string filter, long minSize, long maxSize)
		: base(filter)
	{
		this.minSize = minSize;
		this.maxSize = maxSize;
	}

	public override bool IsMatch(string fileName)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		long length = fileInfo.Length;
		return base.IsMatch(fileName) && MinSize <= length && MaxSize >= length;
	}
}
