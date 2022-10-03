using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
	public sealed class Guards  //地图守卫
	{
		public static Dictionary<ushort, Guards> DataSheet;

		public string 守卫名字;  //Name
		public ushort 守卫编号;  //GuardNumber
		public byte 守卫等级;    //Level
		public bool 虚无状态;   //Nothingness
		public bool 能否受伤;   //CanBeInjured
		public int 尸体保留;   //CorpsePreservation
		public int 复活间隔;   //RevivalInterval
		public bool 主动攻击;   //ActiveAttack
		public byte 仇恨范围;   //RangeHate
		public string 普攻技能;   //BasicAttackSkills
		public int 商店编号;   //StoreId
		public string 界面代码;  //InterfaceCode

		public static void LoadData()
		{
			DataSheet = new Dictionary<ushort, Guards>();
			string text = Config.GameDataPath + "\\System\\Npc数据\\守卫数据\\";

			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<Guards>(text))
					DataSheet.Add(obj.守卫编号, obj);
			}
		}
	}
}
