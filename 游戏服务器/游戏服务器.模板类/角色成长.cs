using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace 游戏服务器.模板类;

public sealed class 角色成长
{
	public static Dictionary<int, Dictionary<游戏对象属性, int>> 数据表;

	public static readonly Dictionary<byte, int> 升级所需经验;

	public static readonly ushort[] 宠物升级经验;

	static 角色成长()
	{
		升级所需经验 = new Dictionary<byte, int>
		{
			[1] = 100,
			[2] = 200,
			[3] = 300,
			[4] = 400,
			[5] = 600,
			[6] = 900,
			[7] = 1200,
			[8] = 1700,
			[9] = 2500,
			[10] = 6000,
			[11] = 8000,
			[12] = 10000,
			[13] = 15000,
			[14] = 30000,
			[15] = 40000,
			[16] = 50000,
			[17] = 70000,
			[18] = 100000,
			[19] = 120000,
			[20] = 140000,
			[21] = 250000,
			[22] = 300000,
			[23] = 350000,
			[24] = 400000,
			[25] = 500000,
			[26] = 700000,
			[27] = 1000000,
			[28] = 1400000,
			[29] = 1800000,
			[30] = 2000000,
			[31] = 2400000,
			[32] = 2800000,
			[33] = 3200000,
			[34] = 3600000,
			[35] = 4000000,
			[36] = 4800000,
			[37] = 5600000,
			[38] = 8200000,
			[39] = 9000000,
			[40] = 12000000,
			[41] = 16000000,
			[42] = 30000000,
			[43] = 50000000,
			[44] = 80000000,
			[45] = 120000000,
			[46] = 280000000,
			[47] = 360000000,
			[48] = 400000000,
			[49] = 420000000,
			[50] = 430000000,
			[51] = 440000000,
			[52] = 460000000,
			[53] = 480000000,
			[54] = 500000000,
			[55] = 520000000,
			[56] = 550000000,
			[57] = 600000000,
			[58] = 700000000,
			[59] = 800000000,
			[60] = 800000000,
			[61] = 800000000,
			[62] = 800000000,
			[63] = 800000000,
			[64] = 800000000,
			[65] = 800000000,
			[66] = 800000000,
			[67] = 800000000,
			[68] = 800000000,
			[69] = 800000000,
			[70] = 800000000,
			[71] = 800000000,
			[72] = 800000000,
			[73] = 800000000,
			[74] = 800000000,
			[75] = 800000000,
			[76] = 800000000,
			[77] = 800000000,
			[78] = 800000000,
			[79] = 800000000,
			[80] = 800000000,
			[81] = 800000000,
			[82] = 800000000,
			[83] = 800000000,
			[84] = 800000000,
			[85] = 800000000,
			[86] = 800000000,
			[87] = 800000000,
			[88] = 800000000,
			[89] = 800000000,
			[90] = 800000000,
			[91] = 800000000,
			[92] = 800000000,
			[93] = 800000000,
			[94] = 800000000,
			[95] = 800000000,
			[96] = 800000000,
			[97] = 800000000,
			[98] = 800000000,
			[99] = 800000000,
			[100] = 800000000
		};
		宠物升级经验 = new ushort[9] { 5, 10, 15, 20, 25, 30, 35, 40, 45 };
		数据表 = new Dictionary<int, Dictionary<游戏对象属性, int>>();
		string[] array = Regex.Split(File.ReadAllText(自定义类.游戏数据目录 + "\\System\\成长属性.txt").Trim('\r', '\n', '\r'), "\r\n", RegexOptions.IgnoreCase);
		object 属性名数组 = array[0].Split('\t');
		Dictionary<string, int> dictionary = ((IEnumerable<string>)属性名数组).ToDictionary((string K) => K, (string V) => Array.IndexOf((string[])属性名数组, V));
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('\t');
			if (array2.Length <= 1)
			{
				continue;
			}
			Dictionary<游戏对象属性, int> dictionary2 = new Dictionary<游戏对象属性, int>();
			游戏对象职业 num = (游戏对象职业)Enum.Parse(typeof(游戏对象职业), array2[0]);
			int num2 = Convert.ToInt32(array2[1]);
			int key = (int)num * 256 + num2;
			for (int j = 2; j < ((Array)属性名数组).Length; j++)
			{
				if (Enum.TryParse<游戏对象属性>((string)((object[])属性名数组)[j], out var result) && Enum.IsDefined(typeof(游戏对象属性), result))
				{
					dictionary2[result] = Convert.ToInt32(array2[dictionary[result.ToString()]]);
				}
			}
			数据表.Add(key, dictionary2);
		}
	}

	public static Dictionary<游戏对象属性, int> 获取数据(游戏对象职业 职业, byte 等级)
	{
		return 数据表[(byte)职业 * 256 + 等级];
	}
}
