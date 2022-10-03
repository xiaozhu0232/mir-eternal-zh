using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
	public sealed class GameSkills  //技能实例
	{
		public static Dictionary<string, GameSkills> DataSheet;

		public string 技能名字;  //SkillName
		public GameObjectRace 技能职业;   //Race
		public GameSkillType 技能类型;  //SkillType
		public ushort 自身技能编号;  //OwnSkillId
		public byte 自身铭文编号;  //Id
		public byte 技能分组编号;  //GroupId
		public ushort 绑定等级编号; //BindingLevelId
		public bool 需要正向走位;  //NeedMoveForward
		public byte 技能最远距离;  //MaxDistance
 		public bool 计算幸运概率;  //CalculateLuckyProbability
		public float 计算触发概率;  //CalculateTriggerProbability
		public GameObjectStats 属性提升概率;  //StatBoostProbability 属性提升概率
		public float 属性提升系数;  //StatBoostFactor
		public bool 检查忙绿状态;     //CheckBusyGreen
		public bool 检查硬直状态;         //CheckStiff
		public bool 检查职业武器;   //CheckOccupationalWeapons
		public bool 检查被动标记;     //CheckPassiveTags
		public bool 检查技能标记;      //CheckSkillMarks
		public bool 检查技能计数;     //CheckSkillCount
		public ushort 技能标记编号;        //SkillTagId
		public int[] 需要消耗魔法;     //NeedConsumeMagic
		public HashSet<int> 需要消耗物品;  //NeedConsumeItems
		public int 消耗物品数量;     //NeedConsumeItemsQuantity
		public int 战具扣除点数;      //GearDeductionPoints
		public ushort 验证已学技能;   //ValidateLearnedSkills
		public byte 验证技能铭文;    //VerficationSkillInscription
		public ushort 验证角色Buff;    //VerifyPlayerBuff
		public int 角色Buff层数;     //PlayerBuffLayer
		public 指定目标类型 验证目标类型;   //SpecifyTargetType VerifyTargetType
		public ushort 验证目标Buff;     //VerifyTargetBuff
		public int 目标Buff层数;      //TargetBuffLayers
		public SortedDictionary<int, SkillTask> Nodes;    //SortedDictionary<int, SkillTask> Nodes

		public GameSkills()
		{
			角色Buff层数 = 1;
			目标Buff层数 = 1;
			Nodes = new SortedDictionary<int, SkillTask>();
		}

		public static void LoadData()
		{
			DataSheet = new Dictionary<string, GameSkills>();
			string text = Config.GameDataPath + "\\System\\技能数据\\技能数据\\";
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<GameSkills>(text))
					DataSheet.Add(obj.技能名字, obj);
			}
		}
	}
}
