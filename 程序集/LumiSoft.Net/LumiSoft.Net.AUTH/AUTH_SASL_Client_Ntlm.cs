using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LumiSoft.Net.AUTH;

public class AUTH_SASL_Client_Ntlm : AUTH_SASL_Client
{
	private class MessageType1
	{
		private string m_Domain;

		private string m_Host;

		public MessageType1(string domain, string host)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			m_Domain = domain;
			m_Host = host;
		}

		public byte[] ToByte()
		{
			short num = (short)m_Domain.Length;
			short num2 = (short)m_Host.Length;
			byte[] array = new byte[32 + num + num2];
			array[0] = 78;
			array[1] = 84;
			array[2] = 76;
			array[3] = 77;
			array[4] = 83;
			array[5] = 83;
			array[6] = 80;
			array[7] = 0;
			array[8] = 1;
			array[9] = 0;
			array[10] = 0;
			array[11] = 0;
			array[12] = 3;
			array[13] = 178;
			array[14] = 0;
			array[15] = 0;
			short num3 = (short)(32 + num2);
			array[16] = (byte)num;
			array[17] = (byte)(num >> 8);
			array[18] = array[16];
			array[19] = array[17];
			array[20] = (byte)num3;
			array[21] = (byte)(num3 >> 8);
			array[24] = (byte)num2;
			array[25] = (byte)(num2 >> 8);
			array[26] = array[24];
			array[27] = array[25];
			array[28] = 32;
			array[29] = 0;
			byte[] bytes = Encoding.ASCII.GetBytes(m_Host.ToUpper(CultureInfo.InvariantCulture));
			Buffer.BlockCopy(bytes, 0, array, 32, bytes.Length);
			byte[] bytes2 = Encoding.ASCII.GetBytes(m_Domain.ToUpper(CultureInfo.InvariantCulture));
			Buffer.BlockCopy(bytes2, 0, array, num3, bytes2.Length);
			return array;
		}
	}

	private class MessageType2
	{
		private byte[] m_Nonce;

		public byte[] Nonce => m_Nonce;

		public MessageType2(byte[] nonce)
		{
			if (nonce == null)
			{
				throw new ArgumentNullException("nonce");
			}
			if (nonce.Length != 8)
			{
				throw new ArgumentException("Argument 'nonce' value must be 8 bytes value.", "nonce");
			}
			m_Nonce = nonce;
		}

		public static MessageType2 Parse(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			byte[] array = new byte[8];
			Buffer.BlockCopy(data, 24, array, 0, 8);
			return new MessageType2(array);
		}
	}

	private class MessageType3
	{
		private string m_Domain;

		private string m_User;

		private string m_Host;

		private byte[] m_LM;

		private byte[] m_NT;

		public MessageType3(string domain, string user, string host, byte[] lm, byte[] nt)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (lm == null)
			{
				throw new ArgumentNullException("lm");
			}
			if (nt == null)
			{
				throw new ArgumentNullException("nt");
			}
			m_Domain = domain;
			m_User = user;
			m_Host = host;
			m_LM = lm;
			m_NT = nt;
		}

		public byte[] ToByte()
		{
			byte[] bytes = Encoding.Unicode.GetBytes(m_Domain.ToUpper(CultureInfo.InvariantCulture));
			byte[] bytes2 = Encoding.Unicode.GetBytes(m_User);
			byte[] bytes3 = Encoding.Unicode.GetBytes(m_Host.ToUpper(CultureInfo.InvariantCulture));
			byte[] array = new byte[64 + bytes.Length + bytes2.Length + bytes3.Length + 24 + 24];
			array[0] = 78;
			array[1] = 84;
			array[2] = 76;
			array[3] = 77;
			array[4] = 83;
			array[5] = 83;
			array[6] = 80;
			array[7] = 0;
			array[8] = 3;
			array[9] = 0;
			array[10] = 0;
			array[11] = 0;
			short num = (short)(64 + bytes.Length + bytes2.Length + bytes3.Length);
			array[12] = 24;
			array[13] = 0;
			array[14] = 24;
			array[15] = 0;
			array[16] = (byte)num;
			array[17] = (byte)(num >> 8);
			short num2 = (short)(num + 24);
			array[20] = 24;
			array[21] = 0;
			array[22] = 24;
			array[23] = 0;
			array[24] = (byte)num2;
			array[25] = (byte)(num2 >> 8);
			short num3 = (short)bytes.Length;
			short num4 = 64;
			array[28] = (byte)num3;
			array[29] = (byte)(num3 >> 8);
			array[30] = array[28];
			array[31] = array[29];
			array[32] = (byte)num4;
			array[33] = (byte)(num4 >> 8);
			short num5 = (short)bytes2.Length;
			short num6 = (short)(num4 + num3);
			array[36] = (byte)num5;
			array[37] = (byte)(num5 >> 8);
			array[38] = array[36];
			array[39] = array[37];
			array[40] = (byte)num6;
			array[41] = (byte)(num6 >> 8);
			short num7 = (short)bytes3.Length;
			short num8 = (short)(num6 + num5);
			array[44] = (byte)num7;
			array[45] = (byte)(num7 >> 8);
			array[46] = array[44];
			array[47] = array[45];
			array[48] = (byte)num8;
			array[49] = (byte)(num8 >> 8);
			short num9 = (short)array.Length;
			array[56] = (byte)num9;
			array[57] = (byte)(num9 >> 8);
			array[60] = 1;
			array[61] = 130;
			array[62] = 0;
			array[63] = 0;
			Buffer.BlockCopy(bytes, 0, array, num4, bytes.Length);
			Buffer.BlockCopy(bytes2, 0, array, num6, bytes2.Length);
			Buffer.BlockCopy(bytes3, 0, array, num8, bytes3.Length);
			Buffer.BlockCopy(m_LM, 0, array, num, 24);
			Buffer.BlockCopy(m_NT, 0, array, num2, 24);
			return array;
		}
	}

	private class NTLM_Utils
	{
		public static byte[] CalculateLM(byte[] nonce, string password)
		{
			if (nonce == null)
			{
				throw new ArgumentNullException("nonce");
			}
			if (password == null)
			{
				throw new ArgumentNullException("password");
			}
			byte[] array = new byte[21];
			byte[] inputBuffer = new byte[8] { 75, 71, 83, 33, 64, 35, 36, 37 };
			byte[] src = new byte[8] { 170, 211, 180, 53, 181, 20, 4, 238 };
			DES dES = DES.Create();
			dES.Mode = CipherMode.ECB;
			if (password.Length < 1)
			{
				Buffer.BlockCopy(src, 0, array, 0, 8);
			}
			else
			{
				dES.Key = PasswordToKey(password, 0);
				dES.CreateEncryptor().TransformBlock(inputBuffer, 0, 8, array, 0);
			}
			if (password.Length < 8)
			{
				Buffer.BlockCopy(src, 0, array, 8, 8);
			}
			else
			{
				dES.Key = PasswordToKey(password, 7);
				dES.CreateEncryptor().TransformBlock(inputBuffer, 0, 8, array, 8);
			}
			return calc_resp(nonce, array);
		}

		public static byte[] CalculateNT(byte[] nonce, string password)
		{
			if (nonce == null)
			{
				throw new ArgumentNullException("nonce");
			}
			if (password == null)
			{
				throw new ArgumentNullException("password");
			}
			byte[] array = new byte[21];
			Buffer.BlockCopy(_MD4.Create().ComputeHash(Encoding.Unicode.GetBytes(password)), 0, array, 0, 16);
			return calc_resp(nonce, array);
		}

		private static byte[] calc_resp(byte[] nonce, byte[] data)
		{
			byte[] array = new byte[24];
			DES dES = DES.Create();
			dES.Mode = CipherMode.ECB;
			dES.Key = setup_des_key(data, 0);
			dES.CreateEncryptor().TransformBlock(nonce, 0, 8, array, 0);
			dES.Key = setup_des_key(data, 7);
			dES.CreateEncryptor().TransformBlock(nonce, 0, 8, array, 8);
			dES.Key = setup_des_key(data, 14);
			dES.CreateEncryptor().TransformBlock(nonce, 0, 8, array, 16);
			return array;
		}

		private static byte[] setup_des_key(byte[] key56bits, int position)
		{
			return new byte[8]
			{
				key56bits[position],
				(byte)((key56bits[position] << 7) | (key56bits[position + 1] >> 1)),
				(byte)((key56bits[position + 1] << 6) | (key56bits[position + 2] >> 2)),
				(byte)((key56bits[position + 2] << 5) | (key56bits[position + 3] >> 3)),
				(byte)((key56bits[position + 3] << 4) | (key56bits[position + 4] >> 4)),
				(byte)((key56bits[position + 4] << 3) | (key56bits[position + 5] >> 5)),
				(byte)((key56bits[position + 5] << 2) | (key56bits[position + 6] >> 6)),
				(byte)(key56bits[position + 6] << 1)
			};
		}

		private static byte[] PasswordToKey(string password, int position)
		{
			byte[] array = new byte[7];
			int charCount = Math.Min(password.Length - position, 7);
			Encoding.ASCII.GetBytes(password.ToUpper(CultureInfo.CurrentCulture), position, charCount, array, 0);
			return setup_des_key(array, 0);
		}
	}

	private bool m_IsCompleted;

	private int m_State;

	private string m_Domain;

	private string m_UserName;

	private string m_Password;

	public override bool IsCompleted => m_IsCompleted;

	public override string Name => "NTLM";

	public override string UserName => m_UserName;

	public override bool SupportsInitialResponse => true;

	public AUTH_SASL_Client_Ntlm(string domain, string userName, string password)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (userName == null)
		{
			throw new ArgumentNullException("userName");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		m_Domain = domain;
		m_UserName = userName;
		m_Password = password;
	}

	public override byte[] Continue(byte[] serverResponse)
	{
		if (m_IsCompleted)
		{
			throw new InvalidOperationException("Authentication is completed.");
		}
		if (m_State == 0)
		{
			m_State++;
			return new MessageType1(m_Domain, Environment.MachineName).ToByte();
		}
		if (m_State == 1)
		{
			m_State++;
			m_IsCompleted = true;
			byte[] nonce = MessageType2.Parse(serverResponse).Nonce;
			return new MessageType3(m_Domain, m_UserName, Environment.MachineName, NTLM_Utils.CalculateLM(nonce, m_Password), NTLM_Utils.CalculateNT(nonce, m_Password)).ToByte();
		}
		throw new InvalidOperationException("Authentication is completed.");
	}
}
