using System;
using System.Collections.Generic;
using GameServer.Templates;

namespace GameServer.Data
{
	
	public sealed class SkillData : GameData
	{
		
		public int 技能索引
		{
			get
			{
				return (int)(this.SkillId.V * 100 + (ushort)(this.Id * 10) + (ushort)this.SkillLevel.V);
			}
		}

		
		public SkillData()
		{
			
			
		}

		
		public SkillData(ushort 编号)
		{
			
			
			this.ShorcutField.V = 100;
			this.SkillId.V = 编号;
			this.RemainingTimeLeft.V = this.SkillCount;
			GameDataGateway.SkillData表.AddData(this, true);
		}

		
		public override string ToString()
		{
			InscriptionSkill 铭文模板 = this.铭文模板;
			if (铭文模板 == null)
			{
				return null;
			}
			return 铭文模板.技能名字;
		}

		
		public InscriptionSkill 铭文模板
		{
			get
			{
				return InscriptionSkill.DataSheet[this.SkillIndex];
			}
		}

		
		public bool 自动装配
		{
			get
			{
				return this.铭文模板.被动技能;
			}
		}

		public byte 升级等级
		{
			get
			{
				if (this.铭文模板.需要角色等级 == null || this.铭文模板.需要角色等级.Length <= (int)(this.SkillLevel.V + 1))
				{
					return byte.MaxValue;
				}
				if (this.铭文模板.需要角色等级[(int)this.SkillLevel.V] == 0)
				{
					return byte.MaxValue;
				}
				return this.铭文模板.需要角色等级[(int)this.SkillLevel.V];
			}
		}

		
		public byte SkillCount
		{
			get
			{
				return this.铭文模板.技能计数;
			}
		}

		
		public ushort PeriodCount
		{
			get
			{
				return this.铭文模板.计数周期;
			}
		}

		
		public int 升级经验
		{
			get
			{
				if (this.铭文模板.需要技能经验 != null && this.铭文模板.需要技能经验.Length > (int)this.SkillLevel.V)
				{
					return this.铭文模板.需要技能经验[(int)this.SkillLevel.V];
				}
				return 0;
			}
		}

		
		public ushort SkillIndex
		{
			get
			{
				return (ushort)(this.SkillId.V * 10 + (ushort)this.Id);
			}
		}

		
		public int CombatBonus
		{
			get
			{
				return this.铭文模板.技能战力加成[(int)this.SkillLevel.V];
			}
		}

		
		public List<ushort> 技能Buff
		{
			get
			{
				return this.铭文模板.铭文附带Buff;
			}
		}

		
		public List<ushort> 被动技能
		{
			get
			{
				return this.铭文模板.被动技能列表;
			}
		}

		
		public Dictionary<GameObjectStats, int> Stat加成
		{
			get
			{
				if (this.铭文模板.StatsBonusDictionary != null && this.铭文模板.StatsBonusDictionary.Length > (int)this.SkillLevel.V)
				{
					return this.铭文模板.StatsBonusDictionary[(int)this.SkillLevel.V];
				}
				return null;
			}
		}

		
		public byte Id;

		
		public DateTime 计数时间;

		
		public readonly DataMonitor<ushort> SkillId;

		
		public readonly DataMonitor<ushort> SkillExp;

		
		public readonly DataMonitor<byte> SkillLevel;

		
		public readonly DataMonitor<byte> ShorcutField;

		
		public readonly DataMonitor<byte> RemainingTimeLeft;
	}
}
