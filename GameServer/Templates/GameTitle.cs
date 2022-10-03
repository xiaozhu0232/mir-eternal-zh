using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
	public sealed class GameTitle //游戏称号
	{
		public static Dictionary<byte, GameTitle> DataSheet; //游戏称号> 数据表

		public byte 称号编号; //Id
		public string 称号名字;  //Name
		public int 称号战力;  //Combat
		public int 有效时间;  //EffectiveTime
		public Dictionary<GameObjectStats, int> 称号属性;  //游戏对象属性 Attributes

		public static void LoadData()
		{
			DataSheet = new Dictionary<byte, GameTitle>();
			string text = Config.GameDataPath + "\\System\\物品数据\\游戏称号\\";
			if (Directory.Exists(text))
			{
				var array = Serializer.Deserialize<GameTitle>(text);
				for (int i = 0; i < array.Length; i++)
					DataSheet.Add(array[i].称号编号, array[i]);
			}
		}
	}
}
