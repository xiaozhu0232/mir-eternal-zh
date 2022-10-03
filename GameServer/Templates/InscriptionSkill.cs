using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
	public sealed class InscriptionSkill  //铭文技能
	{
		public static Dictionary<ushort, InscriptionSkill> DataSheet;  //铭文技能> 数据表;
		private static Dictionary<byte, List<InscriptionSkill>> _probabilityTable;  //概率表

		public string 技能名字;   //SkillName 
		public GameObjectRace 技能职业;  // Race
		public ushort 技能编号;  //SkillId
		public byte 铭文编号;   //Id 
		public byte 技能计数;  //SkillCount
		public ushort 计数周期;  //PeriodCount
		public bool 被动技能;  //PassiveSkill
		public byte 铭文品质;  //Quality
		public int 洗练概率;  //Probability
		public bool 广播通知;  //BroadcastNotification
		public bool 角色死亡消失;   //RemoveOnDie
		public string 铭文描述;   //Description
		public byte[] 需要角色等级;   //MinPlayerLevel
		public int[] 需要技能经验;  //MinSkillExp
		public int[] 技能战力加成;  //SkillCombatBonus
		public InscriptionStat[] 铭文属性加成;  //StatsBonus ? InscriptionStat
		public List<ushort> 铭文附带Buff;   //ComesWithBuff
		public List<ushort> 被动技能列表;   //PassiveSkills
		public List<string> 主体技能列表;    //MainSkills
		public List<string> 开关技能列表;   //SwitchSkills

		private Dictionary<GameObjectStats, int>[] _statsBonus;   //_statsBonus

		public ushort Index
		{
			get
			{
				return (ushort)(技能编号 * 10 + (ushort)铭文编号);
			}
		}

		public Dictionary<GameObjectStats, int>[] StatsBonusDictionary
		{
			get
			{
				if (_statsBonus != null)
				{
					return _statsBonus;
				}
				_statsBonus = new Dictionary<GameObjectStats, int>[]
				{
					new Dictionary<GameObjectStats, int>(),
					new Dictionary<GameObjectStats, int>(),
					new Dictionary<GameObjectStats, int>(),
					new Dictionary<GameObjectStats, int>()
				};
				if (铭文属性加成 != null)
				{
					foreach (InscriptionStat 铭文Stat in 铭文属性加成)
					{
						_statsBonus[0][铭文Stat.Stat] = 铭文Stat.零级;
						_statsBonus[1][铭文Stat.Stat] = 铭文Stat.一级;
						_statsBonus[2][铭文Stat.Stat] = 铭文Stat.二级;
						_statsBonus[3][铭文Stat.Stat] = 铭文Stat.三级;
					}
				}
				return _statsBonus;
			}
		}

		public static InscriptionSkill RandomWashing(byte cleanUpRace)
		{
			List<InscriptionSkill> list;
			if (_probabilityTable.TryGetValue(cleanUpRace, out list) && list.Count > 0)
				return list[MainProcess.RandomNumber.Next(list.Count)];
			return null;
		}
		
		public static void LoadData()
		{
			DataSheet = new Dictionary<ushort, InscriptionSkill>();
			string text = Config.GameDataPath + "\\System\\技能数据\\铭文数据\\";
			
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<InscriptionSkill>(text))
					DataSheet.Add(obj.Index, obj);
			}

            var dictionary = new Dictionary<byte, List<InscriptionSkill>>
            {
                [0] = new List<InscriptionSkill>(),
                [1] = new List<InscriptionSkill>(),
                [2] = new List<InscriptionSkill>(),
                [3] = new List<InscriptionSkill>(),
                [4] = new List<InscriptionSkill>(),
                [5] = new List<InscriptionSkill>()
            };

            _probabilityTable = dictionary;
			foreach (InscriptionSkill skill in DataSheet.Values)
			{
				if (skill.铭文编号 != 0)
				{
					for (int j = 0; j < skill.洗练概率; j++)
					{
						_probabilityTable[(byte)skill.技能职业].Add(skill);
					}
				}
			}
			foreach (var list in _probabilityTable.Values)
			{
				for (int k = 0; k < list.Count; k++)
				{
					InscriptionSkill value = list[k];
					int index = MainProcess.RandomNumber.Next(list.Count);
					list[k] = list[index];
					list[index] = value;
				}
			}
		}
	}
}
