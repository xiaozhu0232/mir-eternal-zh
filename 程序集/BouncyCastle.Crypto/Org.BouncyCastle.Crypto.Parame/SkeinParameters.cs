using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters;

public class SkeinParameters : ICipherParameters
{
	public class Builder
	{
		private IDictionary parameters = Platform.CreateHashtable();

		public Builder()
		{
		}

		public Builder(IDictionary paramsMap)
		{
			IEnumerator enumerator = paramsMap.Keys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int num = (int)enumerator.Current;
				parameters.Add(num, paramsMap[num]);
			}
		}

		public Builder(SkeinParameters parameters)
		{
			IEnumerator enumerator = parameters.parameters.Keys.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int num = (int)enumerator.Current;
				this.parameters.Add(num, parameters.parameters[num]);
			}
		}

		public Builder Set(int type, byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentException("Parameter value must not be null.");
			}
			switch (type)
			{
			default:
				throw new ArgumentException("Parameter types must be in the range 0,5..47,49..62.");
			case 0:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 21:
			case 22:
			case 23:
			case 24:
			case 25:
			case 26:
			case 27:
			case 28:
			case 29:
			case 30:
			case 31:
			case 32:
			case 33:
			case 34:
			case 35:
			case 36:
			case 37:
			case 38:
			case 39:
			case 40:
			case 41:
			case 42:
			case 43:
			case 44:
			case 45:
			case 46:
			case 47:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
				if (type == 4)
				{
					throw new ArgumentException("Parameter type " + 4 + " is reserved for internal use.");
				}
				parameters.Add(type, value);
				return this;
			}
		}

		public Builder SetKey(byte[] key)
		{
			return Set(0, key);
		}

		public Builder SetPersonalisation(byte[] personalisation)
		{
			return Set(8, personalisation);
		}

		public Builder SetPersonalisation(DateTime date, string emailAddress, string distinguisher)
		{
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
				streamWriter.Write(date.ToString("YYYYMMDD", CultureInfo.InvariantCulture));
				streamWriter.Write(" ");
				streamWriter.Write(emailAddress);
				streamWriter.Write(" ");
				streamWriter.Write(distinguisher);
				Platform.Dispose(streamWriter);
				return Set(8, memoryStream.ToArray());
			}
			catch (IOException innerException)
			{
				throw new InvalidOperationException("Byte I/O failed.", innerException);
			}
		}

		public Builder SetPublicKey(byte[] publicKey)
		{
			return Set(12, publicKey);
		}

		public Builder SetKeyIdentifier(byte[] keyIdentifier)
		{
			return Set(16, keyIdentifier);
		}

		public Builder SetNonce(byte[] nonce)
		{
			return Set(20, nonce);
		}

		public SkeinParameters Build()
		{
			return new SkeinParameters(parameters);
		}
	}

	public const int PARAM_TYPE_KEY = 0;

	public const int PARAM_TYPE_CONFIG = 4;

	public const int PARAM_TYPE_PERSONALISATION = 8;

	public const int PARAM_TYPE_PUBLIC_KEY = 12;

	public const int PARAM_TYPE_KEY_IDENTIFIER = 16;

	public const int PARAM_TYPE_NONCE = 20;

	public const int PARAM_TYPE_MESSAGE = 48;

	public const int PARAM_TYPE_OUTPUT = 63;

	private IDictionary parameters;

	public SkeinParameters()
		: this(Platform.CreateHashtable())
	{
	}

	private SkeinParameters(IDictionary parameters)
	{
		this.parameters = parameters;
	}

	public IDictionary GetParameters()
	{
		return parameters;
	}

	public byte[] GetKey()
	{
		return (byte[])parameters[0];
	}

	public byte[] GetPersonalisation()
	{
		return (byte[])parameters[8];
	}

	public byte[] GetPublicKey()
	{
		return (byte[])parameters[12];
	}

	public byte[] GetKeyIdentifier()
	{
		return (byte[])parameters[16];
	}

	public byte[] GetNonce()
	{
		return (byte[])parameters[20];
	}
}
