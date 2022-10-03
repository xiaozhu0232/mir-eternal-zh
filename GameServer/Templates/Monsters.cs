using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Templates
{
    public sealed class Monsters //游戏怪物
    {
        public static Dictionary<string, Monsters> DataSheet;  //游戏怪物> 数据表

        public string 怪物名字;  //MonsterName
        public ushort 怪物编号;   //Id
        public byte 怪物等级;   //Level
        public 技能范围类型 怪物体型;   //ObjectSize Size
        public MonsterRaceType 怪物分类;   //怪物种族分类 Race
        public MonsterLevelType 怪物级别;  //Category
        public bool 怪物禁止移动;   //ForbbidenMove
        public bool 脱战自动石化;   //OutWarAutomaticPetrochemical
        public ushort 石化状态编号;  //PetrochemicalStatusId
        public bool 可见隐身目标;  //VisibleStealthTargets
        public bool 可被技能推动;  //CanBeDrivenBySkills
        public bool 可被技能控制;  //CanBeControlledBySkills
        public bool 可被技能诱惑;   //CanBeSeducedBySkills
        public float 基础诱惑概率;   //BaseTemptationProbability
        public ushort 怪物移动间隔;  //MoveInterval
        public ushort 怪物漫游间隔;  //RoamInterval
        public ushort 尸体保留时长; //CorpsePreservationDuration
        public bool 主动攻击目标;  //ActiveAttackTarget
        public byte 怪物仇恨范围;   //RangeHate
        public ushort 怪物仇恨时间;   //HateTime
        public string 普通攻击技能;   //NormalAttackSkills
        public string 概率触发技能;  //ProbabilityTriggerSkills
        public string 进入战斗技能;   //EnterCombatSkills
        public string 退出战斗技能;   //ExitCombatSkills
        public string 瞬移释放技能;   //MoveReleaseSkill
        public string 复活释放技能;   //BirthReleaseSkill
        public string 死亡释放技能;   //DeathReleaseSkill
        public 基础属性[] 怪物基础;   //基础属性   Stats
        public 成长属性[] 怪物成长;   //  GrowthStat Grows
        public 属性继承[] 继承属性;   //InheritStat InheritsStats
        public ushort 怪物提供经验;   //ProvideExperience
        public List<MonsterDrop> 怪物掉落物品;   //怪物掉落 Drops
        public Dictionary<GameItems, long> 掉落统计 = new Dictionary<GameItems, long>(); //DropStats  

        private Dictionary<GameObjectStats, int> _基础属性;   //_basicStats
        private Dictionary<GameObjectStats, int>[] _成长属性;  //_growStats

        public static void LoadData()
        {
            DataSheet = new Dictionary<string, Monsters>();
            string text = Config.GameDataPath + "\\System\\Npc数据\\怪物数据\\";
            if (Directory.Exists(text))
            {
                var array = Serializer.Deserialize<Monsters>(text);
                for (int i = 0; i < array.Length; i++)
                    DataSheet.Add(array[i].怪物名字, array[i]);
            }
        }

        public Dictionary<GameObjectStats, int> 基础属性  //BasicStats
        {
            get
            {
                if (_基础属性 != null)
                {
                    return _基础属性;
                }
                _基础属性 = new Dictionary<GameObjectStats, int>();
                if (怪物基础 != null)
                {
                    foreach (基础属性 start in 怪物基础)
                    {
                        _基础属性[start.属性] = start.数值;
                    }
                }
                return _基础属性;
            }
        }

        public Dictionary<GameObjectStats, int>[] GrowStats
        {
            get
            {
                if (_成长属性 != null)
                {
                    return _成长属性;
                }
                _成长属性 = new Dictionary<GameObjectStats, int>[]
                {
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>(),
                    new Dictionary<GameObjectStats, int>()
                };
                if (怪物成长 != null)
                {
                    foreach (成长属性 stat in 怪物成长)
                    {
                        _成长属性[0][stat.Stat] = stat.零级;
                        _成长属性[1][stat.Stat] = stat.一级;
                        _成长属性[2][stat.Stat] = stat.二级;
                        _成长属性[3][stat.Stat] = stat.三级;
                        _成长属性[4][stat.Stat] = stat.四级;
                        _成长属性[5][stat.Stat] = stat.五级;
                        _成长属性[6][stat.Stat] = stat.六级;
                        _成长属性[7][stat.Stat] = stat.七级;
                    }
                }
                return _成长属性;
            }
        }
    }
}
