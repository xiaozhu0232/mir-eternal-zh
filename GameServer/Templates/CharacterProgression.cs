using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameServer.Templates
{

    public sealed class CharacterProgression  //成长属性
    {

        static CharacterProgression()
        {

            Dictionary<byte, long> dictionary = new Dictionary<byte, long>();
			dictionary[1] = 100L;
			dictionary[2] = 200L;
			dictionary[3] = 300L;
			dictionary[4] = 400L;
			dictionary[5] = 600L;
			dictionary[6] = 900L;
			dictionary[7] = 1200L;
			dictionary[8] = 1700L;
			dictionary[9] = 2500L;
			dictionary[10] = 6000L;
			dictionary[11] = 8000L;
			dictionary[12] = 10000L;
			dictionary[13] = 15000L;
			dictionary[14] = 30000L;
			dictionary[15] = 40000L;
			dictionary[16] = 50000L;
			dictionary[17] = 70000L;
			dictionary[18] = 100000L;
			dictionary[19] = 120000L;
			dictionary[20] = 140000L;
			dictionary[21] = 250000L;
			dictionary[22] = 300000L;
			dictionary[23] = 350000L;
			dictionary[24] = 400000L;
			dictionary[25] = 500000L;
			dictionary[26] = 700000L;
			dictionary[27] = 1000000L;
			dictionary[28] = 1400000L;
			dictionary[29] = 1800000L;
			dictionary[30] = 2000000L;
			dictionary[31] = 2400000L;
			dictionary[32] = 2800000L;
			dictionary[33] = 3200000L;
			dictionary[34] = 3600000L;
			dictionary[35] = 4000000L;
			dictionary[36] = 4800000L;
			dictionary[37] = 5600000L;
			dictionary[38] = 8200000L;
			dictionary[39] = 9000000L;
			dictionary[40] = 12000000L;
			dictionary[41] = 16000000L;
			dictionary[42] = 30000000L;
			dictionary[43] = 50000000L;
			dictionary[44] = 80000000L;
			dictionary[45] = 120000000L;
			dictionary[46] = 280000000L;
			dictionary[47] = 360000000L;
			dictionary[48] = 400000000L;
			dictionary[49] = 420000000L;
			dictionary[50] = 430000000L;
			dictionary[51] = 440000000L;
			dictionary[52] = 460000000L;
			dictionary[53] = 480000000L;
			dictionary[54] = 500000000L;
			dictionary[55] = 520000000L;
			dictionary[56] = 550000000L;
			dictionary[57] = 600000000L;
			dictionary[58] = 700000000L;
			dictionary[59] = 800000000L;
			dictionary[60] = 900000000L;
			dictionary[61] = 1000000000L;
			dictionary[62] = 1100000000L;
			dictionary[63] = 1200000000L;
			dictionary[64] = 1300000000L;
			dictionary[65] = 1400000000L;
			dictionary[66] = 1500000000L;
			dictionary[67] = 1600000000L;
			dictionary[68] = 1700000000L;
			dictionary[69] = 1800000000L;
			dictionary[70] = 1900000000L;
			dictionary[71] = 2000000000L;
			dictionary[72] = 2100000000L;
			dictionary[73] = 2100000000L;
			dictionary[74] = 2100000000L;
			dictionary[75] = 2100000000L;
			dictionary[76] = 2100000000L;
			dictionary[77] = 2100000000L;
			dictionary[78] = 2100000000L;
			dictionary[79] = 2100000000L;
			dictionary[80] = 2100000000L;
			dictionary[81] = 2100000000L;
			dictionary[82] = 2100000000L;
			dictionary[83] = 2100000000L;
			dictionary[84] = 2100000000L;
			dictionary[85] = 2100000000L;
			dictionary[86] = 2100000000L;
			dictionary[87] = 2100000000L;
			dictionary[88] = 2100000000L;
			dictionary[89] = 2100000000L;
			dictionary[90] = 2100000000L;
			dictionary[91] = 2100000000L;
			dictionary[92] = 2100000000L;
			dictionary[93] = 2100000000L;
			dictionary[94] = 2100000000L;
			dictionary[95] = 2100000000L;
			dictionary[96] = 2100000000L;
			dictionary[97] = 2100000000L;
			dictionary[98] = 2100000000L;
			dictionary[99] = 2100000000L;
			dictionary[100] = 2100000000L;

            for (byte i = 100; i <= 105; i++)
                dictionary[i] = 2100000000;

            CharacterProgression.升级所需经验 = dictionary;   //MaxExpTable
            CharacterProgression.宠物升级经验 = new ushort[]
            {
                5,
                10,
                15,
                20,
                25,
                30,
                35,
                40,
                45
            };
            CharacterProgression.DataSheet = new Dictionary<int, Dictionary<GameObjectStats, int>>();
            string path = Config.GameDataPath + "\\System\\成长属性.txt";
            string[] array = Regex.Split(File.ReadAllText(path).Trim(new char[]
            {
                '\r',
                '\n',
                '\r'
            }), "\r\n", RegexOptions.IgnoreCase);
            Dictionary<string, int> dictionary2 = array[0].Split(new char[]
            {
                '\t'
            }).ToDictionary((string K) => K, (string V) => Array.IndexOf<string>((string[])array[0].Split(new char[]
            {
                '\t'
            }), V));
            for (int i = 1; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(new char[]
                {
                    '\t'
                });
                if (array2.Length > 1)
                {
                    Dictionary<GameObjectStats, int> dictionary3 = new Dictionary<GameObjectStats, int>();
                    int num = (int)((GameObjectRace)Enum.Parse(typeof(GameObjectRace), array2[0]));
                    int num2 = Convert.ToInt32(array2[1]);
                    int key = num * 256 + num2;
                    for (int j = 2; j < array[0].Split(new char[]
                    {
                        '\t'
                    }).Length; j++)
                    {
                        GameObjectStats GameObjectProperties;
                        if (Enum.TryParse<GameObjectStats>(array[0].Split(new char[]
                        {
                            '\t'
                        })[j], out GameObjectProperties) && Enum.IsDefined(typeof(GameObjectStats), GameObjectProperties))
                        {
                            dictionary3[GameObjectProperties] = Convert.ToInt32(array2[dictionary2[GameObjectProperties.ToString()]]);
                        }
                    }
                    CharacterProgression.DataSheet.Add(key, dictionary3);
                }
            }
        }


        public static Dictionary<GameObjectStats, int> GetData(GameObjectRace 职业, byte 等级)
        {
            return CharacterProgression.DataSheet[(int)((byte)职业) * 256 + (int)等级];
        }


        public CharacterProgression()
        {


        }


        public static Dictionary<int, Dictionary<GameObjectStats, int>> DataSheet;  //游戏对象属性  数据表


        public static readonly Dictionary<byte, long> 升级所需经验;  //MaxExpTable

        public static readonly ushort[] 宠物升级经验;
    }
}
