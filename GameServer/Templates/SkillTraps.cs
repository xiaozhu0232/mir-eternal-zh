using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
	public sealed class SkillTraps //技能陷阱
	{
		public static Dictionary<string, SkillTraps> DataSheet;  // 技能陷阱> 数据表

		public string 陷阱名字;   //Name
		public ushort 陷阱编号;   //Id
		public ushort 分组编号;  //GroupId
		public 技能范围类型 陷阱体型;  //ObjectSize Size
		public ushort 绑定等级;   //BindingLevel
		public bool 陷阱允许叠加;   //AllowStacking
		public int 陷阱持续时间;       //Duration
		public bool 持续时间延长;    //ExtendedDuration
		public bool 技能等级延时;     //SkillLevelDelay
		public int 每级延长时间;     //ExtendedTimePerLevel
		public bool 角色属性延时;    //PlayerStatDelay
		public GameObjectStats 绑定角色属性;   //游戏对象属性 BoundPlayerStat
		public float 属性延时系数;    //StatDelayFactor
		public bool 特定铭文延时;   //HasSpecificInscriptionDelay
		public InscriptionSkill 绑定铭文技能;    //铭文技能 BindInscriptionSkill
		public int 特定铭文技能;   //SpecificInscriptionSkills
		public int 铭文延长时间;   //InscriptionExtendedTime
		public bool 陷阱能否移动;    //CanMove
		public ushort 陷阱移动速度;   //MoveSpeed
		public byte 限制移动次数;   //LimitMoveSteps
		public bool 当前方向移动;    //MoveInCurrentDirection
		public bool 主动追击敌人;   //ActivelyPursueEnemy
		public byte 陷阱追击范围;    //PursuitRange
 		public string 被动触发技能;   //PassiveTriggerSkill
		public bool 禁止重复触发;  //RetriggeringIsProhibited
		public 指定目标类型 被动指定类型;   //指定目标类型 SpecifyTargetType   PassiveTargetType
		public 游戏对象类型 被动限定类型;    //游戏对象类型 PassiveObjectType;
		public GameObjectRelationship 被动限定关系;   //GameObjectRelationship PassiveType;
		public string 主动触发技能;   //ActivelyTriggerSkills
 		public ushort 主动触发间隔;   //ActivelyTriggerInterval
		public ushort 主动触发延迟;    //ActivelyTriggerDelay

		public static void LoadData()
		{
			DataSheet = new Dictionary<string, SkillTraps>();
			string text = Config.GameDataPath + "\\System\\技能数据\\陷阱数据\\";
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<SkillTraps>(text))
					DataSheet.Add(obj.陷阱名字, obj);
			}
		}
	}
}
