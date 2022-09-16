using System;

namespace Newtonsoft.Json.Linq;

public class JsonLoadSettings
{
	private CommentHandling _commentHandling;

	private LineInfoHandling _lineInfoHandling;

	private DuplicatePropertyNameHandling _duplicatePropertyNameHandling;

	public CommentHandling CommentHandling
	{
		get
		{
			return _commentHandling;
		}
		set
		{
			if (value < CommentHandling.Ignore || value > CommentHandling.Load)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_commentHandling = value;
		}
	}

	public LineInfoHandling LineInfoHandling
	{
		get
		{
			return _lineInfoHandling;
		}
		set
		{
			if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_lineInfoHandling = value;
		}
	}

	public DuplicatePropertyNameHandling DuplicatePropertyNameHandling
	{
		get
		{
			return _duplicatePropertyNameHandling;
		}
		set
		{
			if (value < DuplicatePropertyNameHandling.Replace || value > DuplicatePropertyNameHandling.Error)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_duplicatePropertyNameHandling = value;
		}
	}

	public JsonLoadSettings()
	{
		_lineInfoHandling = LineInfoHandling.Load;
		_commentHandling = CommentHandling.Ignore;
		_duplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
	}
}
