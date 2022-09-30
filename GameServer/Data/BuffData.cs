using System;
using System.Collections.Generic;
using GameServer.Maps;
using GameServer.Templates;

namespace GameServer.Data
{

    public sealed class BuffData : GameData
    {

        public BuffData()
        {


        }


        public BuffData(MapObject 来源, MapObject 目标, ushort 编号)
        {


            this.Buff来源 = 来源;
            this.Id.V = 编号;
            this.当前层数.V = this.Template.Buff初始层数;
            this.持续时间.V = TimeSpan.FromMilliseconds((double)this.Template.Buff持续时间);
            this.处理计时.V = TimeSpan.FromMilliseconds((double)this.Template.Buff处理延迟);
            PlayerObject PlayerObject = 来源 as PlayerObject;
            if (PlayerObject != null)
            {
                SkillData SkillData;
                if (this.Template.绑定技能等级 != 0 && PlayerObject.MainSkills表.TryGetValue(this.Template.绑定技能等级, out SkillData))
                {
                    this.Buff等级.V = SkillData.SkillLevel.V;
                }
                if (this.Template.持续时间延长 && this.Template.技能等级延时)
                {
                    this.持续时间.V += TimeSpan.FromMilliseconds((double)((int)this.Buff等级.V * this.Template.每级延长时间));
                }
                if (this.Template.持续时间延长 && this.Template.角色属性延时)
                {
                    this.持续时间.V += TimeSpan.FromMilliseconds((double)((float)PlayerObject[this.Template.绑定角色属性] * this.Template.属性延时系数));
                }
                SkillData SkillData2;
                if (this.Template.持续时间延长 && this.Template.特定铭文延时 && PlayerObject.MainSkills表.TryGetValue((ushort)(this.Template.特定铭文技能 / 10), out SkillData2) && (int)SkillData2.Id == this.Template.特定铭文技能 % 10)
                {
                    this.持续时间.V += TimeSpan.FromMilliseconds((double)this.Template.铭文延长时间);
                }
            }
            else
            {
                PetObject PetObject = 来源 as PetObject;
                if (PetObject != null)
                {
                    SkillData SkillData3;
                    if (this.Template.绑定技能等级 != 0 && PetObject.PlayerOwner.MainSkills表.TryGetValue(this.Template.绑定技能等级, out SkillData3))
                    {
                        this.Buff等级.V = SkillData3.SkillLevel.V;
                    }
                    if (this.Template.持续时间延长 && this.Template.技能等级延时)
                    {
                        this.持续时间.V += TimeSpan.FromMilliseconds((double)((int)this.Buff等级.V * this.Template.每级延长时间));
                    }
                    if (this.Template.持续时间延长 && this.Template.角色属性延时)
                    {
                        this.持续时间.V += TimeSpan.FromMilliseconds((double)((float)PetObject.PlayerOwner[this.Template.绑定角色属性] * this.Template.属性延时系数));
                    }
                    SkillData SkillData4;
                    if (this.Template.持续时间延长 && this.Template.特定铭文延时 && PetObject.PlayerOwner.MainSkills表.TryGetValue((ushort)(this.Template.特定铭文技能 / 10), out SkillData4) && (int)SkillData4.Id == this.Template.特定铭文技能 % 10)
                    {
                        this.持续时间.V += TimeSpan.FromMilliseconds((double)this.Template.铭文延长时间);
                    }
                }
            }
            this.剩余时间.V = this.持续时间.V;
            if ((this.Buff效果 & Buff效果类型.造成伤害) != Buff效果类型.技能标志)
            {
                int[] Buff伤害基数 = this.Template.Buff伤害基数;
                int? num = (Buff伤害基数 != null) ? new int?(Buff伤害基数.Length) : null;
                int v = (int)this.Buff等级.V;
                int num2 = (num.GetValueOrDefault() > v & num != null) ? this.Template.Buff伤害基数[(int)this.Buff等级.V] : 0;
                float[] Buff伤害系数 = this.Template.Buff伤害系数;
                num = ((Buff伤害系数 != null) ? new int?(Buff伤害系数.Length) : null);
                v = (int)this.Buff等级.V;
                float num3 = (num.GetValueOrDefault() > v & num != null) ? this.Template.Buff伤害系数[(int)this.Buff等级.V] : 0f;
                PlayerObject PlayerObject2 = 来源 as PlayerObject;
                SkillData SkillData5;
                if (PlayerObject2 != null && this.Template.强化铭文编号 != 0 && PlayerObject2.MainSkills表.TryGetValue((ushort)(this.Template.强化铭文编号 / 10), out SkillData5) && (int)SkillData5.Id == this.Template.强化铭文编号 % 10)
                {
                    num2 += this.Template.铭文强化基数;
                    num3 += this.Template.铭文强化系数;
                }
                int num4 = 0;
                switch (this.伤害类型)
                {
                    case SkillDamageType.Attack:
                        num4 = ComputingClass.CalculateAttack(来源[GameObjectStats.MinDC], 来源[GameObjectStats.MaxDC], 来源[GameObjectStats.Luck]);
                        break;
                    case SkillDamageType.Magic:
                        num4 = ComputingClass.CalculateAttack(来源[GameObjectStats.MinMC], 来源[GameObjectStats.MaxMC], 来源[GameObjectStats.Luck]);
                        break;
                    case SkillDamageType.Taoism:
                        num4 = ComputingClass.CalculateAttack(来源[GameObjectStats.MinSC], 来源[GameObjectStats.MaxSC], 来源[GameObjectStats.Luck]);
                        break;
                    case SkillDamageType.Needle:
                        num4 = ComputingClass.CalculateAttack(来源[GameObjectStats.MinNC], 来源[GameObjectStats.MaxNC], 来源[GameObjectStats.Luck]);
                        break;
                    case SkillDamageType.Archery:
                        num4 = ComputingClass.CalculateAttack(来源[GameObjectStats.MinBC], 来源[GameObjectStats.MaxBC], 来源[GameObjectStats.Luck]);
                        break;
                    case SkillDamageType.Toxicity:
                        num4 = 来源[GameObjectStats.MaxSC];
                        break;
                    case SkillDamageType.Sacred:
                        num4 = ComputingClass.CalculateAttack(来源[GameObjectStats.MinHC], 来源[GameObjectStats.MaxHC], 0);
                        break;
                }
                this.伤害基数.V = num2 + (int)((float)num4 * num3);
            }
            if (目标 is PlayerObject)
            {
                GameDataGateway.BuffData表.AddData(this, true);
            }
        }


        public override string ToString()
        {
            GameBuffs buff模板 = this.Template;
            if (buff模板 == null)
            {
                return null;
            }
            return buff模板.Buff名字;
        }


        public Buff效果类型 Buff效果
        {
            get
            {
                return this.Template.Buff效果;
            }
        }


        public SkillDamageType 伤害类型
        {
            get
            {
                return this.Template.Buff伤害类型;
            }
        }


        public GameBuffs Template
        {
            get
            {
                GameBuffs result;
                if (!GameBuffs.DataSheet.TryGetValue(this.Id.V, out result))
                {
                    return null;
                }
                return result;
            }
        }

        public bool OnReleaseSkillRemove => Template.释放技能消失;


        public bool 增益Buff
        {
            get
            {
                return this.Template.ActionType == BuffActionType.Gain;
            }
        }


        public bool Buff同步
        {
            get
            {
                return this.Template.同步至客户端;
            }
        }


        public bool 到期消失
        {
            get
            {
                GameBuffs buff模板 = this.Template;
                return buff模板 != null && buff模板.到期主动消失;
            }
        }


        public bool 下线消失
        {
            get
            {
                return this.Template.角色下线消失;
            }
        }


        public bool 死亡消失
        {
            get
            {
                return this.Template.角色死亡消失;
            }
        }


        public bool 换图消失
        {
            get
            {
                return this.Template.切换地图消失;
            }
        }


        public bool BoundWeapons
        {
            get
            {
                return this.Template.切换武器消失;
            }
        }


        public bool 添加冷却
        {
            get
            {
                return this.Template.移除添加冷却;
            }
        }


        public ushort 绑定技能
        {
            get
            {
                return this.Template.绑定技能等级;
            }
        }


        public ushort Cooldown
        {
            get
            {
                return this.Template.技能冷却时间;
            }
        }


        public int 处理延迟
        {
            get
            {
                return this.Template.Buff处理延迟;
            }
        }


        public int 处理间隔
        {
            get
            {
                return this.Template.Buff处理间隔;
            }
        }


        public byte 最大层数
        {
            get
            {
                return this.Template.Buff最大层数;
            }
        }


        public ushort Buff分组
        {
            get
            {
                if (this.Template.分组编号 == 0)
                {
                    return this.Id.V;
                }
                return this.Template.分组编号;
            }
        }


        public ushort[] 依存列表
        {
            get
            {
                return this.Template.依存Buff列表;
            }
        }


        public Dictionary<GameObjectStats, int> Stat加成
        {
            get
            {
                if ((this.Buff效果 & Buff效果类型.属性增减) != Buff效果类型.技能标志)
                {
                    return this.Template.基础StatsIncOrDec[(int)this.Buff等级.V];
                }
                return null;
            }
        }


        public MapObject Buff来源;


        public readonly DataMonitor<ushort> Id;


        public readonly DataMonitor<TimeSpan> 持续时间;


        public readonly DataMonitor<TimeSpan> 剩余时间;


        public readonly DataMonitor<TimeSpan> 处理计时;


        public readonly DataMonitor<byte> 当前层数;


        public readonly DataMonitor<byte> Buff等级;


        public readonly DataMonitor<int> 伤害基数;
    }
}
