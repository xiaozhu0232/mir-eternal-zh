using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
	public sealed class GameBuffs //游戏Buff
	{
		public static Dictionary<ushort, GameBuffs> DataSheet;  //游戏Buff> 数据表 

		public string Buff名字; //Name
		public ushort Buff编号;  //Id 
		public ushort 分组编号;  //GroupId
		public Buff作用类型 作用类型;  //BuffActionType ActionType;
		public Buff叠加类型 叠加类型;  //BuffOverlayType OverlayType;
		public Buff效果类型 Buff效果;  //BuffEffectType Effect;
		public bool 同步至客户端;  //SyncClient;
		public bool 到期主动消失; //RemoveOnExpire
		public bool 切换地图消失;  //OnChangeMapRemove
		public bool 切换武器消失;  //OnChangeWeaponRemove
		public bool 角色死亡消失;   //OnPlayerDiesRemove
		public bool 角色下线消失;  //OnPlayerDisconnectRemove
		public bool 释放技能消失;  //OnReleaseSkillRemove
		public ushort 绑定技能等级;  //BindingSkillLevel
		public bool 移除添加冷却;  //RemoveAddCooling
		public ushort 技能冷却时间;  //SkillCooldown
		public byte Buff初始层数;  //BuffInitialLayer
		public byte Buff最大层数;  //MaxBuffCount
		public bool Buff允许合成;  //AllowsSynthesis
		public byte Buff合成层数;  //BuffSynthesisLayer
		public ushort Buff合成编号;  //BuffSynthesisId
		public int Buff处理间隔;  //ProcessInterval
		public int Buff处理延迟;   //ProcessDelay
		public int Buff持续时间;       //Duration
		public bool 持续时间延长;  //ExtendedDuration
		public ushort 后接Buff编号;   //FollowedById
		public ushort 连带Buff编号;     //AssociatedId
		public ushort[] 依存Buff列表;      //RequireBuff
		public bool 技能等级延时;   //SkillLevelDelay
		public int 每级延长时间;    //ExtendedTimePerLevel
		public bool 角色属性延时;  //PlayerStatDelay
		public GameObjectStats 绑定角色属性;  //BoundPlayerStat
		public float 属性延时系数;    //StatDelayFactor
		public bool 特定铭文延时;    //HasSpecificInscriptionDelay
		public int 特定铭文技能;   //SpecificInscriptionSkills
		public int 铭文延长时间;   //InscriptionExtendedTime
		public GameObjectState 角色所处状态;  //PlayerState
		public InscriptionStat[] 属性增减;  //StatsIncOrDec 属性增减
		public 技能伤害类型 Buff伤害类型;   //DamageType
		public int[] Buff伤害基数;   //DamageBase
		public float[] Buff伤害系数;  //DamageFactor
		public int 强化铭文编号;  //StrengthenInscriptionId
		public int 铭文强化基数;   //StrengthenInscriptionBase
		public float 铭文强化系数;   //StrengthenInscriptionFactor
		public bool 效果生效移除;   //EffectRemoved
		public ushort 生效后接编号;   //EffectiveFollowedById
		public bool 后接技能来源;   //FollowUpSkillSource
		public BuffDetherminationMethod 效果判定方式;   //BuffDetherminationMethod HowJudgeEffect;
		public bool 限定伤害上限;   //LimitedDamage 
		public int 限定伤害数值;   //LimitedDamageValue
		public Buff判定类型 效果判定类型;  //EffectJudgeType
		public HashSet<ushort> 特定技能编号;   //SpecificSkillId
		public int[] 伤害增减基数;  //DamageIncOrDecBase
		public float[] 伤害增减系数;  //DamageIncOrDecFactor
		public string 触发陷阱技能;  //TriggerTrapSkills
		public ObjectSize 触发陷阱数量; //NumberTrapsTriggered
		public byte[] 体力回复基数;  //PhysicalRecoveryBase
		public int 诱惑时长增加;  //TemptationIncreaseDuration
		public float 诱惑概率增加;  //TemptationIncreaseRate
		public byte 诱惑等级增加;  //TemptationIncreaseLevel

		private Dictionary<GameObjectStats, int>[] _baseStatsIncOrDec; // 

		public static void LoadData()
		{
			DataSheet = new Dictionary<ushort, GameBuffs>();
			string text = Config.GameDataPath + "\\System\\技能数据\\Buff数据\\";
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<GameBuffs>(text))
					DataSheet.Add(obj.Buff编号, obj);
			}
		}
		
		public Dictionary<GameObjectStats, int>[] 基础StatsIncOrDec
		{
			get
			{
				if (_baseStatsIncOrDec != null)
				{
					return _baseStatsIncOrDec;
				}
				_baseStatsIncOrDec = new Dictionary<GameObjectStats, int>[]
				{
					new Dictionary<GameObjectStats, int>(),
					new Dictionary<GameObjectStats, int>(),
					new Dictionary<GameObjectStats, int>(),
					new Dictionary<GameObjectStats, int>()
				};
				if (属性增减 != null)
				{
					foreach (InscriptionStat stat in 属性增减)
					{
						_baseStatsIncOrDec[0][stat.Stat] = stat.Level0;
						_baseStatsIncOrDec[1][stat.Stat] = stat.Level1;
						_baseStatsIncOrDec[2][stat.Stat] = stat.Level2;
						_baseStatsIncOrDec[3][stat.Stat] = stat.Level3;
					}
				}
				return _baseStatsIncOrDec;
			}
		}

		
	}
}
