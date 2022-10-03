using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GameServer.Data;
using GameServer.Templates;
using GameServer.Networking;
using GamePackets.Server;

namespace GameServer.Maps
{

    public abstract class MapObject
    {

        public override string ToString()
        {
            return this.ObjectName;
        }

        public DateTime RecoveryTime { get; set; } //恢复时间
        public DateTime HealTime { get; set; }  //治疗时间
        public DateTime TimeoutTime { get; set; }  //脱战时间
        public DateTime CurrentTime { get; set; }  //处理计时
        public DateTime ProcessTime { get; set; }  //预约时间
        public virtual int ProcessInterval { get; }  //处理间隔 
        public int TreatmentCount { get; set; }   //治疗次数
        public int TreatmentBase { get; set; }   //治疗基数
        public byte ActionId { get; set; }   //动作编号
        public bool FightingStance { get; set; }  //战斗姿态
        public abstract 游戏对象类型 ObjectType { get; }
        public abstract 技能范围类型 ObjectSize { get; }
        public ushort WalkSpeed => (ushort)this[GameObjectStats.行走速度];
        public ushort RunSpeed => (ushort)this[GameObjectStats.奔跑速度];
        public virtual int WalkInterval => WalkSpeed * 60;
        public virtual int RunInterval => RunSpeed * 60;
        public virtual int ObjectId { get; set; }
        public virtual int CurrentHP { get; set; }
        public virtual int CurrentMP { get; set; }
        public virtual byte CurrentLevel { get; set; }
        public virtual bool Died { get; set; }
        public virtual bool Blocking { get; set; }
        public virtual bool CanBeHit => !this.Died;
        public virtual string ObjectName { get; set; }
        public virtual GameDirection CurrentDirection { get; set; }
        public virtual MapInstance CurrentMap { get; set; }
        public virtual Point CurrentPosition { get; set; }
        public virtual ushort CurrentAltitude => CurrentMap.GetTerrainHeight(CurrentPosition);
        public virtual DateTime BusyTime { get; set; }
        public virtual DateTime HardTime { get; set; }
        public virtual DateTime WalkTime { get; set; }
        public virtual DateTime RunTime { get; set; }
        public virtual Dictionary<GameObjectStats, int> Stats { get; }
        public virtual MonitorDictionary<int, DateTime> Coolings { get; }
        public virtual MonitorDictionary<ushort, BuffData> Buffs { get; }

        public bool SecondaryObject;
        public bool ActiveObject;
        public HashSet<MapObject> NeighborsImportant;
        public HashSet<MapObject> NeighborsSneak;
        public HashSet<MapObject> Neighbors;
        public HashSet<SkillInstance> SkillTasks;
        public HashSet<TrapObject> Traps;
        public Dictionary<object, Dictionary<GameObjectStats, int>> StatsBonus;

        public virtual int this[GameObjectStats Stat]
        {
            get
            {
                if (!Stats.ContainsKey(Stat))
                {
                    return 0;
                }
                return Stats[Stat];
            }
            set
            {
                Stats[Stat] = value;
                if (Stat == GameObjectStats.最大体力)
                {
                    CurrentHP = Math.Min(CurrentHP, value);
                    return;
                }
                if (Stat == GameObjectStats.最大魔力)
                {
                    CurrentMP = Math.Min(CurrentMP, value);
                }
            }
        }

        public virtual void RefreshStats()
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            foreach (object obj in Enum.GetValues(typeof(GameObjectStats)))
            {
                int num5 = 0;
                GameObjectStats stat = (GameObjectStats)obj;
                foreach (KeyValuePair<object, Dictionary<GameObjectStats, int>> keyValuePair in StatsBonus)
                {
                    int num6;
                    if (keyValuePair.Value != null && keyValuePair.Value.TryGetValue(stat, out num6) && num6 != 0)
                    {
                        if (keyValuePair.Key is BuffData)
                        {
                            if (stat == GameObjectStats.行走速度)
                            {
                                num2 = Math.Max(num2, num6);
                                num = Math.Min(num, num6);
                            }
                            else if (stat == GameObjectStats.奔跑速度)
                            {
                                num4 = Math.Max(num4, num6);
                                num3 = Math.Min(num3, num6);
                            }
                            else
                            {
                                num5 += num6;
                            }
                        }
                        else
                        {
                            num5 += num6;
                        }
                    }
                }
                if (stat == GameObjectStats.行走速度)
                {
                    this[stat] = Math.Max(1, num5 + num + num2);
                }
                else if (stat == GameObjectStats.奔跑速度)
                {
                    this[stat] = Math.Max(1, num5 + num3 + num4);
                }
                else if (stat == GameObjectStats.幸运等级)
                {
                    this[stat] = num5;
                }
                else
                {
                    this[stat] = Math.Max(0, num5);
                }
            }

            if (this is PlayerObject playerObject)
            {
                foreach (PetObject petObject in playerObject.Pets)
                {
                    if (petObject.Template.继承属性 != null)
                    {
                        var dictionary = new Dictionary<GameObjectStats, int>();
                        foreach (var InheritStat in petObject.Template.继承属性)
                        {
                            dictionary[InheritStat.转换属性] = (int)(this[InheritStat.继承属性] * InheritStat.继承比例);
                        }
                        petObject.StatsBonus[playerObject.CharacterData] = dictionary;
                        petObject.RefreshStats();
                    }
                }
            }
        }

        public virtual void Process()
        {
            CurrentTime = MainProcess.CurrentTime;
            ProcessTime = MainProcess.CurrentTime.AddMilliseconds(ProcessInterval);
        }

        public virtual void Dies(MapObject obj, bool skillKill)
        {
            SendPacket(new ObjectDiesPacket
            {
                ObjectId = ObjectId
            });

            SkillTasks.Clear();
            Died = true;
            Blocking = false;

            foreach (var neightborObject in Neighbors)
                neightborObject.NotifyObjectDies(this);
        }

        public MapObject()
        {
            CurrentTime = MainProcess.CurrentTime;
            SkillTasks = new HashSet<SkillInstance>();
            Traps = new HashSet<TrapObject>();
            NeighborsImportant = new HashSet<MapObject>();
            NeighborsSneak = new HashSet<MapObject>();
            Neighbors = new HashSet<MapObject>();
            Stats = new Dictionary<GameObjectStats, int>();
            Coolings = new MonitorDictionary<int, DateTime>(null);
            Buffs = new MonitorDictionary<ushort, BuffData>(null);
            StatsBonus = new Dictionary<object, Dictionary<GameObjectStats, int>>();
            ProcessTime = MainProcess.CurrentTime.AddMilliseconds(MainProcess.RandomNumber.Next(ProcessInterval));
        }

        public void UnbindGrid()
        {
            foreach (Point location in ComputingClass.GetLocationRange(CurrentPosition, CurrentDirection, ObjectSize))
                CurrentMap[location].Remove(this);
        }

        public void BindGrid()
        {
            foreach (Point location in ComputingClass.GetLocationRange(CurrentPosition, CurrentDirection, ObjectSize))
                CurrentMap[location].Add(this);
        }

        public void Delete()
        {
            NotifyNeightborClear();
            UnbindGrid();
            SecondaryObject = false;
            MapGatewayProcess.RemoveObject(this);
            ActiveObject = false;
            MapGatewayProcess.DeactivateObject(this);
        }

        public int GetDistance(Point location) => ComputingClass.GridDistance(this.CurrentPosition, location);

        public int GetDistance(MapObject obj) => ComputingClass.GridDistance(this.CurrentPosition, obj.CurrentPosition);

        public void SendPacket(GamePacket packet)
        {
            if (packet.PacketInfo.Broadcast)
                BroadcastPacket(packet);

            if (this is PlayerObject playerObj)
                playerObj.ActiveConnection?.SendPacket(packet);
        }

        private void BroadcastPacket(GamePacket packet)
        {
            foreach (MapObject obj in this.Neighbors)
            {
                PlayerObject PlayerObject = obj as PlayerObject;
                if (PlayerObject != null && !PlayerObject.NeighborsSneak.Contains(this) && PlayerObject != null)
                {
                    PlayerObject.ActiveConnection.SendPacket(packet);
                }
            }
        }

        public bool CanBeSeenBy(MapObject obj)
        {
            return Math.Abs(CurrentPosition.X - obj.CurrentPosition.X) <= 20
                && Math.Abs(CurrentPosition.Y - obj.CurrentPosition.Y) <= 20;
        }


        public bool CanAttack(MapObject obj)
        {
            if (obj.Died)
                return false;

            if (this is MonsterObject monsterObject)
            {
                return monsterObject.ActiveAttackTarget && (
                    obj is PlayerObject
                    || obj is PetObject
                    || (obj is GuardObject guardObject && guardObject.CanBeInjured)
                    );
            }
            else if (this is GuardObject guardObject)
            {
                return guardObject.ActiveAttackTarget
                    && (
                        (obj is MonsterObject neightborMonsterObject && neightborMonsterObject.ActiveAttackTarget)
                        || (obj is PlayerObject playerObject && playerObject.红名玩家)
                        || (obj is PetObject && guardObject.MobId == 6734)
                    );
            }
            else if (this is PetObject)
            {
                return obj is MonsterObject neightborMonsterObject
                    && neightborMonsterObject.ActiveAttackTarget;
            }

            return false;
        }

        public bool IsNeightbor(MapObject obj)
        {
            switch (ObjectType)
            {
                case 游戏对象类型.玩家:
                    return true;
                case 游戏对象类型.宠物:
                case 游戏对象类型.怪物:
                    return obj.ObjectType == 游戏对象类型.怪物
                        || obj.ObjectType == 游戏对象类型.玩家
                        || obj.ObjectType == 游戏对象类型.宠物
                        || obj.ObjectType == 游戏对象类型.Npcc
                        || obj.ObjectType == 游戏对象类型.陷阱;
                case 游戏对象类型.Npcc:
                    return obj.ObjectType == 游戏对象类型.怪物
                        || obj.ObjectType == 游戏对象类型.玩家
                        || obj.ObjectType == 游戏对象类型.宠物
                        || obj.ObjectType == 游戏对象类型.陷阱;
                case 游戏对象类型.物品:
                    return obj.ObjectType == 游戏对象类型.玩家;
                case 游戏对象类型.陷阱:
                    return obj.ObjectType == 游戏对象类型.玩家
                        || obj.ObjectType == 游戏对象类型.宠物;
            }

            return false;
        }

        public GameObjectRelationship GetRelationship(MapObject obj)
        {
            if (this == obj)
                return GameObjectRelationship.自身;

            if (obj is TrapObject neightborTrapObject)
                obj = neightborTrapObject.TrapSource;

            if (this is GuardObject)
            {
                if (obj is MonsterObject || obj is PetObject || obj is PlayerObject)
                    return GameObjectRelationship.敌对;
            }
            else if (this is PlayerObject playerObject)
            {
                if (obj is MonsterObject)
                    return GameObjectRelationship.敌对;
                else if (obj is GuardObject)
                    return playerObject.AttackMode == AttackMode.全体 && CurrentMap.MapId != 80
                        ? GameObjectRelationship.敌对
                        : GameObjectRelationship.友方;
                else if (obj is PlayerObject neightborPlayerObject)
                    return playerObject.AttackMode == AttackMode.和平
                        || (
                            playerObject.AttackMode == AttackMode.行会
                            && playerObject.Guild != null
                            && neightborPlayerObject.Guild != null
                            && (
                                playerObject.Guild == neightborPlayerObject.Guild
                                || playerObject.Guild.结盟行会.ContainsKey(neightborPlayerObject.Guild)
                            )
                        )
                        || (
                            playerObject.AttackMode == AttackMode.组队 && (
                                playerObject.Team != null && neightborPlayerObject.Team != null
                                && playerObject.Team == neightborPlayerObject.Team
                            )
                        )
                        || (
                            playerObject.AttackMode == AttackMode.善恶 && (
                                !playerObject.红名玩家 && !neightborPlayerObject.红名玩家
                            )
                        )
                        || (
                            playerObject.AttackMode == AttackMode.敌对 && (
                                playerObject.Guild == null
                                || neightborPlayerObject == null
                                || !playerObject.Guild.Hostility行会.ContainsKey(neightborPlayerObject.Guild)
                            )
                        )
                        ? GameObjectRelationship.友方
                        : GameObjectRelationship.敌对;
                else if (obj is PetObject petObject)
                    return (petObject.PlayerOwner == this && playerObject.AttackMode != AttackMode.全体)
                        || (playerObject.AttackMode == AttackMode.和平)
                        || (playerObject.AttackMode == AttackMode.行会 && playerObject.Guild != null && petObject.PlayerOwner.Guild != null && (playerObject.Guild == petObject.PlayerOwner.Guild || playerObject.Guild.结盟行会.ContainsKey(petObject.PlayerOwner.Guild)))
                        || (playerObject.AttackMode == AttackMode.组队 && playerObject.Team != null && petObject.PlayerOwner.Team != null && playerObject.Team == petObject.PlayerOwner.Team)
                        || (playerObject.AttackMode == AttackMode.善恶 && !petObject.PlayerOwner.红名玩家 && !petObject.PlayerOwner.灰名玩家)
                        || (playerObject.AttackMode != AttackMode.敌对 && (
                            playerObject.Guild == null
                            || petObject.PlayerOwner.Guild == null
                            || !playerObject.Guild.Hostility行会.ContainsKey(petObject.PlayerOwner.Guild)
                        ))
                        ? GameObjectRelationship.友方
                        : petObject.PlayerOwner == this
                            ? GameObjectRelationship.友方 | GameObjectRelationship.敌对
                            : GameObjectRelationship.敌对;

            }
            else if (this is PetObject petObject)
                return petObject.PlayerOwner != obj
                    ? petObject.PlayerOwner.GetRelationship(obj)
                    : GameObjectRelationship.友方;
            else if (this is TrapObject trapObject)
                return trapObject.TrapSource.GetRelationship(obj);
            else if (obj is not MonsterObject)
                return GameObjectRelationship.敌对;

            return GameObjectRelationship.友方;
        }

        public bool IsSpecificType(MapObject obj, 指定目标类型 targetType)
        {
            if (obj is TrapObject trapObject)
                obj = trapObject.TrapSource;

            var targetDirection = ComputingClass.GetDirection(obj.CurrentPosition, CurrentPosition);

            if (this is MonsterObject monsterObject)
            {
                return targetType == 指定目标类型.无
                    || (targetType & 指定目标类型.低级目标) == 指定目标类型.低级目标 && CurrentLevel < obj.CurrentLevel
                    || (targetType & 指定目标类型.所有怪物) == 指定目标类型.所有怪物
                    || (targetType & 指定目标类型.低级怪物) == 指定目标类型.低级怪物 && CurrentLevel < obj.CurrentLevel
                    || ((targetType & 指定目标类型.低血怪物) == 指定目标类型.低血怪物 && (float)this.CurrentHP / (float)this[GameObjectStats.最大体力] < 0.4f)
                    || ((targetType & 指定目标类型.普通怪物) == 指定目标类型.普通怪物 && monsterObject.Category == MonsterLevelType.普通怪物)
                    || ((targetType & 指定目标类型.不死生物) == 指定目标类型.不死生物 && monsterObject.怪物种族 == MonsterRaceType.不死生物)
                    || ((targetType & 指定目标类型.虫族生物) == 指定目标类型.虫族生物 && monsterObject.怪物种族 == MonsterRaceType.虫族生物)
                    || ((targetType & 指定目标类型.沃玛怪物) == 指定目标类型.沃玛怪物 && monsterObject.怪物种族 == MonsterRaceType.沃玛怪物)
                    || ((targetType & 指定目标类型.猪类怪物) == 指定目标类型.猪类怪物 && monsterObject.怪物种族 == MonsterRaceType.猪类怪物)
                    || ((targetType & 指定目标类型.祖玛怪物) == 指定目标类型.祖玛怪物 && monsterObject.怪物种族 == MonsterRaceType.祖玛怪物)
                    || ((targetType & 指定目标类型.魔龙怪物) == 指定目标类型.魔龙怪物 && monsterObject.怪物种族 == MonsterRaceType.魔龙怪物)
                    || ((targetType & 指定目标类型.精英怪物) == 指定目标类型.精英怪物 && (monsterObject.Category == MonsterLevelType.精英干将 || monsterObject.Category == MonsterLevelType.头目首领))
                    || (((targetType & 指定目标类型.背刺目标) == 指定目标类型.背刺目标) && (
                            (CurrentDirection == GameDirection.上方 && (targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上))
                            || (CurrentDirection == GameDirection.左上 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方))
                            || (CurrentDirection == GameDirection.左方 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右方 && (targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下))
                            || (CurrentDirection == GameDirection.右上 && (targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方))
                            || (CurrentDirection == GameDirection.左下 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.下方 && (targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右下 && (targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方))
                        )
                    );
            }
            else if (this is GuardObject)
            {
                return targetType == 指定目标类型.无
                    || ((targetType & 指定目标类型.低级目标) == 指定目标类型.低级目标 && CurrentLevel < obj.CurrentLevel)
                    || (((targetType & 指定目标类型.背刺目标) == 指定目标类型.背刺目标) && (
                       (CurrentDirection == GameDirection.上方 && (targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上))
                            || (CurrentDirection == GameDirection.左上 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方))
                            || (CurrentDirection == GameDirection.左方 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右方 && (targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下))
                            || (CurrentDirection == GameDirection.右上 && (targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方))
                            || (CurrentDirection == GameDirection.左下 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.下方 && (targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右下 && (targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方))
                    ));
            }
            else if (this is PetObject petObject)
            {
                return targetType == 指定目标类型.无
                    || ((targetType & 指定目标类型.低级目标) == 指定目标类型.低级目标 && this.CurrentLevel < obj.CurrentLevel)
                    || ((targetType & 指定目标类型.不死生物) == 指定目标类型.不死生物 && petObject.宠物种族 == MonsterRaceType.不死生物)
                    || ((targetType & 指定目标类型.虫族生物) == 指定目标类型.虫族生物 && petObject.宠物种族 == MonsterRaceType.虫族生物)
                    || ((targetType & 指定目标类型.所有宠物) == 指定目标类型.所有宠物)
                    || (((targetType & 指定目标类型.背刺目标) == 指定目标类型.背刺目标) && (
                     (CurrentDirection == GameDirection.上方 && (targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上))
                            || (CurrentDirection == GameDirection.左上 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方))
                            || (CurrentDirection == GameDirection.左方 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右方 && (targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下))
                            || (CurrentDirection == GameDirection.右上 && (targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方))
                            || (CurrentDirection == GameDirection.左下 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.下方 && (targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右下 && (targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方))
                    ));
            }
            else if (this is PlayerObject playerObject)
            {
                return targetType == 指定目标类型.无
                || ((targetType & 指定目标类型.低级目标) == 指定目标类型.低级目标 && this.CurrentLevel < obj.CurrentLevel)
                    || ((targetType & 指定目标类型.带盾法师) == 指定目标类型.带盾法师 && playerObject.CharRole == GameObjectRace.法师 && playerObject.Buffs.ContainsKey(25350))
                    || (((targetType & 指定目标类型.背刺目标) == 指定目标类型.背刺目标) && (
                     (CurrentDirection == GameDirection.上方 && (targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上))
                            || (CurrentDirection == GameDirection.左上 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.上方))
                            || (CurrentDirection == GameDirection.左方 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.左上 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右方 && (targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下))
                            || (CurrentDirection == GameDirection.右上 && (targetDirection == GameDirection.上方 || targetDirection == GameDirection.右上 || targetDirection == GameDirection.右方))
                            || (CurrentDirection == GameDirection.左下 && (targetDirection == GameDirection.左方 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.下方 && (targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方 || targetDirection == GameDirection.左下))
                            || (CurrentDirection == GameDirection.右下 && (targetDirection == GameDirection.右方 || targetDirection == GameDirection.右下 || targetDirection == GameDirection.下方))
                    ));
            }

            return false;
        }


        public virtual bool CanMove()
        {
            return !Died && !(MainProcess.CurrentTime < BusyTime) && !(MainProcess.CurrentTime < WalkTime) && !CheckStatus(GameObjectState.忙绿状态 | GameObjectState.定身状态 | GameObjectState.麻痹状态 | GameObjectState.失神状态);
        }

        public virtual bool CanRun()
        {
            return !Died && !(MainProcess.CurrentTime < BusyTime) && !(MainProcess.CurrentTime < this.RunTime) && !CheckStatus(GameObjectState.忙绿状态 | GameObjectState.残废状态 | GameObjectState.定身状态 | GameObjectState.麻痹状态 | GameObjectState.失神状态);
        }

        public virtual bool CanBeTurned()
        {
            return !Died && !(MainProcess.CurrentTime < BusyTime) && !(MainProcess.CurrentTime < WalkTime) && !CheckStatus(GameObjectState.忙绿状态 | GameObjectState.麻痹状态 | GameObjectState.失神状态);
        }

        public virtual bool CanBePushed(MapObject obj)
        {
            return obj == this
                || (
                    this is MonsterObject monsterObject
                    && monsterObject.CanBeDrivenBySkills
                    && obj.GetRelationship(this) == GameObjectRelationship.敌对
                );
        }


        public virtual bool CanBeDisplaced(MapObject obj, Point location, int distance, int qty, bool throughtWall, out Point displacedLocation, out MapObject[] targets)
        {
            displacedLocation = CurrentPosition;
            targets = null;

            if (!(CurrentPosition == location) && CanBePushed(obj))
            {
                List<MapObject> list = new List<MapObject>();
                for (int i = 1; i <= distance; i++)
                {
                    if (throughtWall)
                    {
                        Point point = ComputingClass.GetFrontPosition(CurrentPosition, location, i);
                        if (CurrentMap.CanPass(point))
                        {
                            displacedLocation = point;
                        }
                        continue;
                    }
                    GameDirection 方向 = ComputingClass.GetDirection(CurrentPosition, location);
                    Point point2 = ComputingClass.GetFrontPosition(CurrentPosition, location, i);
                    if (CurrentMap.IsBlocked(point2))
                    {
                        break;
                    }
                    bool flag = false;
                    if (CurrentMap.CellBlocked(point2))
                    {
                        foreach (MapObject item in CurrentMap[point2].Where((MapObject O) => O.Blocking))
                        {
                            if (list.Count >= qty)
                            {
                                flag = true;
                                break;
                            }
                            if (!item.CanBeDisplaced(obj, ComputingClass.前方坐标(item.CurrentPosition, 方向, 1), 1, qty - list.Count - 1, throughtWall: false, out var _, out var targets2))
                            {
                                flag = true;
                                break;
                            }
                            list.Add(item);
                            list.AddRange(targets2);
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                    displacedLocation = point2;
                }
                targets = list.ToArray();
                return displacedLocation != CurrentPosition;
            }
            return false;
        }

        public virtual bool CheckStatus(GameObjectState state)
        {
            foreach (BuffData BuffData in this.Buffs.Values)
            {
                if ((BuffData.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志 && (BuffData.Template.角色所处状态 & state) != GameObjectState.正常状态)
                {
                    return true;
                }
            }
            return false;
        }


        public void OnAddBuff(ushort buffId, MapObject obj)
        {
            if (this is ItemObject || this is TrapObject || (this is GuardObject guardObject && !guardObject.CanBeInjured))
                return;

            if (obj is TrapObject trapObject)
                obj = trapObject.TrapSource;


            if (!GameBuffs.DataSheet.TryGetValue(buffId, out var 游戏Buff))
                return;

            if ((游戏Buff.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志)
            {
                if (((游戏Buff.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态 || (游戏Buff.角色所处状态 & GameObjectState.潜行状态) != GameObjectState.正常状态) && this.CheckStatus(GameObjectState.暴露状态))
                    return;

                if ((游戏Buff.角色所处状态 & GameObjectState.暴露状态) != GameObjectState.正常状态)
                {
                    foreach (BuffData BuffData in this.Buffs.Values.ToList<BuffData>())
                    {
                        if ((BuffData.Template.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态 || (BuffData.Template.角色所处状态 & GameObjectState.潜行状态) != GameObjectState.正常状态)
                        {
                            移除Buff时处理(BuffData.Id.V);
                        }
                    }
                }
            }

            if ((游戏Buff.Buff效果 & Buff效果类型.造成伤害) != Buff效果类型.技能标志 && 游戏Buff.Buff伤害类型 == 技能伤害类型.灼烧 && this.Buffs.ContainsKey(25352))
                return;

            ushort 分组编号 = (游戏Buff.分组编号 != 0) ? 游戏Buff.分组编号 : 游戏Buff.Buff编号;
            BuffData BuffData2 = null;
            switch (游戏Buff.叠加类型)
            {
                case Buff叠加类型.禁止叠加:
                    if (this.Buffs.Values.FirstOrDefault((BuffData O) => O.Buff分组 == 分组编号) == null)
                    {
                        BuffData2 = (this.Buffs[游戏Buff.Buff编号] = new BuffData(obj, this, 游戏Buff.Buff编号));
                    }
                    break;
                case Buff叠加类型.同类替换:
                    {
                        foreach (var BuffData3 in Buffs.Values.Where(O => O.Buff分组 == 分组编号).ToList())
                        {
                            移除Buff时处理(BuffData3.Id.V);
                        }
                        BuffData2 = (this.Buffs[游戏Buff.Buff编号] = new BuffData(obj, this, 游戏Buff.Buff编号));
                        break;
                    }
                case Buff叠加类型.同类叠加:
                    {
                        if (!Buffs.TryGetValue(buffId, out var BuffData4))
                        {
                            BuffData2 = (this.Buffs[游戏Buff.Buff编号] = new BuffData(obj, this, 游戏Buff.Buff编号));
                            break;
                        }

                        BuffData4.当前层数.V = (byte)Math.Min(BuffData4.当前层数.V + 1, BuffData4.最大层数);
                        if (游戏Buff.Buff允许合成 && BuffData4.当前层数.V >= 游戏Buff.Buff合成层数 && GameBuffs.DataSheet.TryGetValue(游戏Buff.Buff合成编号, out var 游戏Buff2))
                        {
                            移除Buff时处理(BuffData4.Id.V);
                            OnAddBuff(游戏Buff.Buff合成编号, obj);
                            break;
                        }

                        BuffData4.剩余时间.V = BuffData4.持续时间.V;
                        if (BuffData4.Buff同步)
                        {
                            SendPacket(new ObjectStateChangePacket
                            {
                                对象编号 = this.ObjectId,
                                Id = BuffData4.Id.V,
                                Buff索引 = (int)BuffData4.Id.V,
                                当前层数 = BuffData4.当前层数.V,
                                剩余时间 = (int)BuffData4.剩余时间.V.TotalMilliseconds,
                                持续时间 = (int)BuffData4.持续时间.V.TotalMilliseconds
                            });
                        }
                        break;
                    }
                case Buff叠加类型.同类延时:
                    {
                        if (Buffs.TryGetValue(buffId, out var BuffData5))
                        {
                            BuffData5.剩余时间.V += BuffData5.持续时间.V;
                            if (BuffData5.Buff同步)
                            {
                                SendPacket(new ObjectStateChangePacket
                                {
                                    对象编号 = this.ObjectId,
                                    Id = BuffData5.Id.V,
                                    Buff索引 = (int)BuffData5.Id.V,
                                    当前层数 = BuffData5.当前层数.V,
                                    剩余时间 = (int)BuffData5.剩余时间.V.TotalMilliseconds,
                                    持续时间 = (int)BuffData5.持续时间.V.TotalMilliseconds
                                });
                            }
                        }
                        else
                        {
                            BuffData2 = (this.Buffs[游戏Buff.Buff编号] = new BuffData(obj, this, 游戏Buff.Buff编号));
                        }
                        break;
                    }
            }

            if (BuffData2 == null)
                return;


            if (BuffData2.Buff同步)
            {
                SendPacket(new ObjectAddStatePacket
                {
                    SourceObjectId = this.ObjectId,
                    TargetObjectId = obj.ObjectId,
                    BuffId = BuffData2.Id.V,
                    BuffIndex = (int)BuffData2.Id.V,
                    Duration = (int)BuffData2.持续时间.V.TotalMilliseconds,
                    BuffLayers = BuffData2.当前层数.V,
                });
            }

            if ((游戏Buff.Buff效果 & Buff效果类型.属性增减) != Buff效果类型.技能标志)
            {
                StatsBonus.Add(BuffData2, BuffData2.Stat加成);
                RefreshStats();
            }

            if ((游戏Buff.Buff效果 & Buff效果类型.Riding) != Buff效果类型.技能标志 && this is PlayerObject playerObject)
            {
                if (GameMounts.DataSheet.TryGetValue(playerObject.CharacterData.CurrentMount.V, out GameMounts mount))
                {
                    playerObject.Riding = true;
                    StatsBonus.Add(BuffData2, mount.Stats);
                    RefreshStats();
                    if (mount.SoulAuraID > 0)
                        playerObject.OnAddBuff(mount.SoulAuraID, this);
                }

                SendPacket(new SyncObjectMountPacket
                {
                    ObjectId = ObjectId,
                    MountId = (byte)playerObject.CharacterData.CurrentMount.V
                });
            }

            if ((游戏Buff.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志)
            {
                if ((游戏Buff.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态)
                {
                    foreach (MapObject MapObject in this.Neighbors.ToList<MapObject>())
                    {
                        MapObject.对象隐身时处理(this);
                    }
                }
                if ((游戏Buff.角色所处状态 & GameObjectState.潜行状态) != GameObjectState.正常状态)
                {
                    foreach (MapObject MapObject2 in this.Neighbors.ToList<MapObject>())
                    {
                        MapObject2.对象潜行时处理(this);
                    }
                }
            }
            if (游戏Buff.连带Buff编号 != 0)
            {
                OnAddBuff(游戏Buff.连带Buff编号, obj);
            }
        }


        public void 移除Buff时处理(ushort 编号)
        {
            BuffData BuffData;
            if (this.Buffs.TryGetValue(编号, out BuffData))
            {
                MapObject MapObject;
                if (BuffData.Template.后接Buff编号 != 0 && BuffData.Buff来源 != null && MapGatewayProcess.Objects.TryGetValue(BuffData.Buff来源.ObjectId, out MapObject) && MapObject == BuffData.Buff来源)
                {
                    this.OnAddBuff(BuffData.Template.后接Buff编号, BuffData.Buff来源);
                }
                if (BuffData.依存列表 != null)
                {
                    foreach (ushort 编号2 in BuffData.依存列表)
                    {
                        this.删除Buff时处理(编号2);
                    }
                }
                if (BuffData.添加冷却 && BuffData.绑定技能 != 0 && BuffData.Cooldown != 0)
                {
                    PlayerObject PlayerObject = this as PlayerObject;
                    if (PlayerObject != null && PlayerObject.MainSkills表.ContainsKey(BuffData.绑定技能))
                    {
                        DateTime dateTime = MainProcess.CurrentTime.AddMilliseconds((double)BuffData.Cooldown);
                        DateTime t = this.Coolings.ContainsKey((int)BuffData.绑定技能 | 16777216) ? this.Coolings[(int)BuffData.绑定技能 | 16777216] : default(DateTime);
                        if (dateTime > t)
                        {
                            this.Coolings[(int)BuffData.绑定技能 | 16777216] = dateTime;
                            this.SendPacket(new AddedSkillCooldownPacket
                            {
                                CoolingId = ((int)BuffData.绑定技能 | 16777216),
                                Cooldown = (int)BuffData.Cooldown
                            });
                        }
                    }
                }
                this.Buffs.Remove(编号);
                BuffData.Delete();
                if (BuffData.Buff同步)
                {
                    this.SendPacket(new ObjectRemovalStatusPacket
                    {
                        对象编号 = this.ObjectId,
                        Buff索引 = (int)编号
                    });
                }
                if ((BuffData.Buff效果 & Buff效果类型.属性增减) != Buff效果类型.技能标志)
                {
                    this.StatsBonus.Remove(BuffData);
                    this.RefreshStats();
                }

                if ((BuffData.Buff效果 & Buff效果类型.Riding) != Buff效果类型.技能标志 && this is PlayerObject playerObject)
                {
                    playerObject.Riding = false;
                    this.StatsBonus.Remove(BuffData);
                    this.RefreshStats();
                    if (GameMounts.DataSheet.TryGetValue(playerObject.CharacterData.CurrentMount.V, out GameMounts mount))
                        if (mount.SoulAuraID > 0) playerObject.移除Buff时处理(mount.SoulAuraID);
                }

                if ((BuffData.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志)
                {
                    if ((BuffData.Template.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态)
                    {
                        foreach (MapObject MapObject2 in this.Neighbors.ToList<MapObject>())
                        {
                            MapObject2.对象显隐时处理(this);
                        }
                    }
                    if ((BuffData.Template.角色所处状态 & GameObjectState.潜行状态) != GameObjectState.正常状态)
                    {
                        foreach (MapObject MapObject3 in this.Neighbors.ToList<MapObject>())
                        {
                            MapObject3.对象显行时处理(this);
                        }
                    }
                }
            }
        }


        public void 删除Buff时处理(ushort 编号)
        {
            BuffData BuffData;
            if (this.Buffs.TryGetValue(编号, out BuffData))
            {
                if (BuffData.依存列表 != null)
                {
                    foreach (ushort 编号2 in BuffData.依存列表)
                    {
                        this.删除Buff时处理(编号2);
                    }
                }
                this.Buffs.Remove(编号);
                BuffData.Delete();
                if (BuffData.Buff同步)
                {
                    this.SendPacket(new ObjectRemovalStatusPacket
                    {
                        对象编号 = this.ObjectId,
                        Buff索引 = (int)编号
                    });
                }
                if ((BuffData.Buff效果 & Buff效果类型.属性增减) != Buff效果类型.技能标志)
                {
                    this.StatsBonus.Remove(BuffData);
                    this.RefreshStats();
                }
                if ((BuffData.Buff效果 & Buff效果类型.Riding) != Buff效果类型.技能标志 && this is PlayerObject playerObject)
                {
                    playerObject.Riding = false;
                    this.StatsBonus.Remove(BuffData);
                    this.RefreshStats();
                    if (GameMounts.DataSheet.TryGetValue(playerObject.CharacterData.CurrentMount.V, out GameMounts mount))
                        if (mount.SoulAuraID > 0) playerObject.移除Buff时处理(mount.SoulAuraID);
                }
                if ((BuffData.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志)
                {
                    if ((BuffData.Template.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态)
                    {
                        foreach (MapObject MapObject in this.Neighbors.ToList<MapObject>())
                        {
                            MapObject.对象显隐时处理(this);
                        }
                    }
                    if ((BuffData.Template.角色所处状态 & GameObjectState.潜行状态) != GameObjectState.正常状态)
                    {
                        foreach (MapObject MapObject2 in this.Neighbors.ToList<MapObject>())
                        {
                            MapObject2.对象显行时处理(this);
                        }
                    }
                }
            }
        }


        public void 轮询Buff时处理(BuffData 数据)
        {
            if (数据.到期消失 && (数据.剩余时间.V -= MainProcess.CurrentTime - this.CurrentTime) < TimeSpan.Zero)
            {
                this.移除Buff时处理(数据.Id.V);
                return;
            }
            if ((数据.处理计时.V -= MainProcess.CurrentTime - this.CurrentTime) < TimeSpan.Zero)
            {
                数据.处理计时.V += TimeSpan.FromMilliseconds((double)数据.处理间隔);
                if ((数据.Buff效果 & Buff效果类型.造成伤害) != Buff效果类型.技能标志)
                {
                    this.被动受伤时处理(数据);
                }
                if ((数据.Buff效果 & Buff效果类型.生命回复) != Buff效果类型.技能标志)
                {
                    this.被动回复时处理(数据);
                }
            }
        }


        public void ProcessSkillHit(SkillInstance skill, C_01_计算命中目标 info)
        {
            MapObject obj = skill.CasterObject is TrapObject trap
                ? trap.TrapSource
                : skill.CasterObject;

            if (skill.Hits.ContainsKey(ObjectId) || !CanBeHit)
                return;

            if (this != obj && !Neighbors.Contains(obj))
                return;

            if (skill.Hits.Count >= info.限定命中数量)
                return;

            if ((info.限定目标关系 & obj.GetRelationship(this)) == 0)
                return;

            if ((info.限定目标类型 & ObjectType) == 0)
                return;

            if (!IsSpecificType(skill.CasterObject, info.限定特定类型))
                return;

            if ((info.限定目标关系 & GameObjectRelationship.敌对) != 0)
            {
                if (CheckStatus(GameObjectState.无敌状态))
                    return;

                if ((this is PlayerObject || this is PetObject) && (obj is PlayerObject || obj is PetObject) && (CurrentMap.IsSafeZone(CurrentPosition) || obj.CurrentMap.IsSafeZone(obj.CurrentPosition)))
                    return;

                if (obj is MonsterObject && CurrentMap.IsSafeZone(CurrentPosition))
                    return;
            }

            // TODO: Sabak Gates (move some flag to database to remove hardcoded MonsterId)
            if (this is MonsterObject monsterObject && (monsterObject.MonsterId == 8618 || monsterObject.MonsterId == 8621))
            {
                if (obj is PlayerObject playerObject && playerObject.Guild != null && playerObject.Guild == SystemData.Data.OccupyGuild.V)
                    return;

                if (obj is PetObject petObject && petObject.PlayerOwner != null && petObject.PlayerOwner.Guild != null && petObject.PlayerOwner.Guild == SystemData.Data.OccupyGuild.V)
                    return;
            }

            int num = 0;
            float num2 = 0f;
            int num3 = 0;
            float num4 = 0f;

            switch (info.技能闪避方式)
            {
                case 技能闪避类型.技能无法闪避:
                    num = 1;
                    break;
                case 技能闪避类型.可被物理闪避:
                    num3 = this[GameObjectStats.物理敏捷];
                    num = obj[GameObjectStats.物理准确];
                    if (this is MonsterObject)
                    {
                        num2 += obj[GameObjectStats.怪物命中] / 10000f;
                        num4 += this[GameObjectStats.怪物闪避] / 10000f;
                    }
                    break;
                case 技能闪避类型.可被魔法闪避:
                    num4 = this[GameObjectStats.魔法闪避] / 10000f;
                    if (this is MonsterObject)
                    {
                        num2 += obj[GameObjectStats.怪物命中] / 10000f;
                        num4 += this[GameObjectStats.怪物闪避] / 10000f;
                    }
                    break;
                case 技能闪避类型.可被中毒闪避:
                    num4 = this[GameObjectStats.中毒躲避] / 10000f;
                    break;
                case 技能闪避类型.非怪物可闪避:
                    if (this is MonsterObject)
                        num = 1;
                    else
                    {
                        num3 = this[GameObjectStats.物理敏捷];
                        num = obj[GameObjectStats.物理准确];
                    }
                    break;
            }

            var value = new HitDetail(this)
            {
                Feedback = ComputingClass.IsHit(num, num3, num2, num4) ? info.技能命中反馈 : 技能命中反馈.闪避
            };

            skill.Hits.Add(this.ObjectId, value);
        }


        public void 被动受伤时处理(SkillInstance 技能, C_02_计算目标伤害 参数, HitDetail 详情, float 伤害系数)
        {
            TrapObject TrapObject = 技能.CasterObject as TrapObject;
            MapObject MapObject = (TrapObject != null) ? TrapObject.TrapSource : 技能.CasterObject;
            if (this.Died)
            {
                详情.Feedback = 技能命中反馈.丢失;
            }
            else if (!this.Neighbors.Contains(MapObject))
            {
                详情.Feedback = 技能命中反馈.丢失;
            }
            else if ((MapObject.GetRelationship(this) & GameObjectRelationship.敌对) == (GameObjectRelationship)0)
            {
                详情.Feedback = 技能命中反馈.丢失;
            }
            else
            {
                MonsterObject MonsterObject = this as MonsterObject;
                if (MonsterObject != null && (MonsterObject.MonsterId == 8618 || MonsterObject.MonsterId == 8621) && this.GetDistance(MapObject) >= 4)
                {
                    详情.Feedback = 技能命中反馈.丢失;
                }
            }
            if ((详情.Feedback & 技能命中反馈.免疫) == 技能命中反馈.正常 && (详情.Feedback & 技能命中反馈.丢失) == 技能命中反馈.正常)
            {
                if ((详情.Feedback & 技能命中反馈.闪避) == 技能命中反馈.正常)
                {
                    if (参数.技能斩杀类型 != 指定目标类型.无 && ComputingClass.CheckProbability(参数.技能斩杀概率) && this.IsSpecificType(MapObject, 参数.技能斩杀类型))
                    {
                        详情.Damage = this.CurrentHP;
                    }
                    else
                    {
                        int[] 技能伤害基数 = 参数.技能伤害基数;
                        int? num = (技能伤害基数 != null) ? new int?(技能伤害基数.Length) : null;
                        int num2 = (int)技能.SkillLevel;
                        int num3 = (num.GetValueOrDefault() > num2 & num != null) ? 参数.技能伤害基数[(int)技能.SkillLevel] : 0;
                        float[] 技能伤害系数 = 参数.技能伤害系数;
                        num = ((技能伤害系数 != null) ? new int?(技能伤害系数.Length) : null);
                        num2 = (int)技能.SkillLevel;
                        float num4 = (num.GetValueOrDefault() > num2 & num != null) ? 参数.技能伤害系数[(int)技能.SkillLevel] : 0f;
                        if (this is MonsterObject)
                        {
                            num3 += MapObject[GameObjectStats.怪物伤害];
                        }
                        int num5 = 0;
                        float num6 = 0f;
                        if (参数.技能增伤类型 != 指定目标类型.无 && this.IsSpecificType(MapObject, 参数.技能增伤类型))
                        {
                            num5 = 参数.技能增伤基数;
                            num6 = 参数.技能增伤系数;
                        }
                        int num7 = 0;
                        float num8 = 0f;
                        if (参数.技能破防概率 > 0f && ComputingClass.CheckProbability(参数.技能破防概率))
                        {
                            num7 = 参数.技能破防基数;
                            num8 = 参数.技能破防系数;
                        }
                        int num9 = 0;
                        int num10 = 0;
                        switch (参数.技能伤害类型)
                        {
                            case 技能伤害类型.攻击:
                                num10 = ComputingClass.计算防御(this[GameObjectStats.最小防御], this[GameObjectStats.最大防御]);
                                num9 = ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小攻击], MapObject[GameObjectStats.最大攻击], MapObject[GameObjectStats.幸运等级]);
                                break;
                            case 技能伤害类型.魔法:
                                num10 = ComputingClass.计算防御(this[GameObjectStats.最小魔防], this[GameObjectStats.最大魔防]);
                                num9 = ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小魔法], MapObject[GameObjectStats.最大魔法], MapObject[GameObjectStats.幸运等级]);
                                break;
                            case 技能伤害类型.道术:
                                num10 = ComputingClass.计算防御(this[GameObjectStats.最小魔防], this[GameObjectStats.最大魔防]);
                                num9 = ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小道术], MapObject[GameObjectStats.最大道术], MapObject[GameObjectStats.幸运等级]);
                                break;
                            case 技能伤害类型.刺术:
                                num10 = ComputingClass.计算防御(this[GameObjectStats.最小防御], this[GameObjectStats.最大防御]);
                                num9 = ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小刺术], MapObject[GameObjectStats.最大刺术], MapObject[GameObjectStats.幸运等级]);
                                break;
                            case 技能伤害类型.弓术:
                                num10 = ComputingClass.计算防御(this[GameObjectStats.最小防御], this[GameObjectStats.最大防御]);
                                num9 = ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小弓术], MapObject[GameObjectStats.最大弓术], MapObject[GameObjectStats.幸运等级]);
                                break;
                            case 技能伤害类型.毒性:
                                num9 = MapObject[GameObjectStats.最大道术];
                                break;
                            case 技能伤害类型.神圣:
                                num9 = ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小圣伤], MapObject[GameObjectStats.最大圣伤], 0);
                                break;
                        }
                        if (this is MonsterObject)
                        {
                            num10 = Math.Max(0, num10 - (int)((float)(num10 * MapObject[GameObjectStats.怪物破防]) / 10000f));
                        }
                        int num11 = 0;
                        float num12 = 0f;
                        int num13 = int.MaxValue;
                        foreach (BuffData BuffData in MapObject.Buffs.Values.ToList<BuffData>())
                        {
                            if ((BuffData.Buff效果 & Buff效果类型.伤害增减) != Buff效果类型.技能标志 && (BuffData.Template.效果判定方式 == BuffDetherminationMethod.主动攻击增伤 || BuffData.Template.效果判定方式 == BuffDetherminationMethod.主动攻击减伤))
                            {
                                bool flag = false;
                                switch (参数.技能伤害类型)
                                {
                                    case 技能伤害类型.攻击:
                                    case 技能伤害类型.刺术:
                                    case 技能伤害类型.弓术:
                                        {
                                            Buff判定类型 效果判定类型 = BuffData.Template.效果判定类型;
                                            if (效果判定类型 > Buff判定类型.所有物理伤害)
                                            {
                                                if (效果判定类型 == Buff判定类型.所有特定伤害)
                                                {
                                                    HashSet<ushort> 特定技能编号 = BuffData.Template.特定技能编号;
                                                    flag = (特定技能编号 != null && 特定技能编号.Contains(技能.SkillId));
                                                }
                                            }
                                            else
                                            {
                                                flag = true;
                                            }
                                            break;
                                        }
                                    case 技能伤害类型.魔法:
                                    case 技能伤害类型.道术:
                                        switch (BuffData.Template.效果判定类型)
                                        {
                                            case Buff判定类型.所有技能伤害:
                                            case Buff判定类型.所有魔法伤害:
                                                flag = true;
                                                break;
                                            case Buff判定类型.所有特定伤害:
                                                {
                                                    HashSet<ushort> 特定技能编号2 = BuffData.Template.特定技能编号;
                                                    flag = (特定技能编号2 != null && 特定技能编号2.Contains(技能.SkillId));
                                                    break;
                                                }
                                        }
                                        break;
                                    case 技能伤害类型.毒性:
                                    case 技能伤害类型.神圣:
                                    case 技能伤害类型.灼烧:
                                    case 技能伤害类型.撕裂:
                                        if (BuffData.Template.效果判定类型 == Buff判定类型.所有特定伤害)
                                        {
                                            HashSet<ushort> 特定技能编号3 = BuffData.Template.特定技能编号;
                                            flag = (特定技能编号3 != null && 特定技能编号3.Contains(技能.SkillId));
                                        }
                                        break;
                                }
                                if (flag)
                                {
                                    int v = (int)BuffData.当前层数.V;
                                    int[] 伤害增减基数 = BuffData.Template.伤害增减基数;
                                    num = ((伤害增减基数 != null) ? new int?(伤害增减基数.Length) : null);
                                    num2 = (int)BuffData.Buff等级.V;
                                    int num14 = v * ((num.GetValueOrDefault() > num2 & num != null) ? BuffData.Template.伤害增减基数[(int)BuffData.Buff等级.V] : 0);
                                    float num15 = (float)BuffData.当前层数.V;
                                    float[] 伤害增减系数 = BuffData.Template.伤害增减系数;
                                    num = ((伤害增减系数 != null) ? new int?(伤害增减系数.Length) : null);
                                    num2 = (int)BuffData.Buff等级.V;
                                    float num16 = num15 * ((num.GetValueOrDefault() > num2 & num != null) ? BuffData.Template.伤害增减系数[(int)BuffData.Buff等级.V] : 0f);
                                    num11 += ((BuffData.Template.效果判定方式 == BuffDetherminationMethod.主动攻击增伤) ? num14 : (-num14));
                                    num12 += ((BuffData.Template.效果判定方式 == BuffDetherminationMethod.主动攻击增伤) ? num16 : (-num16));
                                    MapObject MapObject2;
                                    if (BuffData.Template.生效后接编号 != 0 && BuffData.Buff来源 != null && MapGatewayProcess.Objects.TryGetValue(BuffData.Buff来源.ObjectId, out MapObject2) && MapObject2 == BuffData.Buff来源)
                                    {
                                        if (BuffData.Template.后接技能来源)
                                        {
                                            MapObject.OnAddBuff(BuffData.Template.生效后接编号, BuffData.Buff来源);
                                        }
                                        else
                                        {
                                            this.OnAddBuff(BuffData.Template.生效后接编号, BuffData.Buff来源);
                                        }
                                    }
                                    if (BuffData.Template.效果生效移除)
                                    {
                                        MapObject.移除Buff时处理(BuffData.Id.V);
                                    }
                                }
                            }
                        }
                        foreach (BuffData BuffData2 in this.Buffs.Values.ToList<BuffData>())
                        {
                            if ((BuffData2.Buff效果 & Buff效果类型.伤害增减) != Buff效果类型.技能标志 && (BuffData2.Template.效果判定方式 == BuffDetherminationMethod.被动受伤增伤 || BuffData2.Template.效果判定方式 == BuffDetherminationMethod.被动受伤减伤))
                            {
                                bool flag2 = false;
                                switch (参数.技能伤害类型)
                                {
                                    case 技能伤害类型.攻击:
                                    case 技能伤害类型.刺术:
                                    case 技能伤害类型.弓术:
                                        {
                                            Buff判定类型 效果判定类型 = BuffData2.Template.效果判定类型;
                                            if (效果判定类型 <= Buff判定类型.所有特定伤害)
                                            {
                                                if (效果判定类型 > Buff判定类型.所有物理伤害)
                                                {
                                                    if (效果判定类型 == Buff判定类型.所有特定伤害)
                                                    {
                                                        HashSet<ushort> 特定技能编号4 = BuffData2.Template.特定技能编号;
                                                        flag2 = (特定技能编号4 != null && 特定技能编号4.Contains(技能.SkillId));
                                                    }
                                                }
                                                else
                                                {
                                                    flag2 = true;
                                                }
                                            }
                                            else if (效果判定类型 != Buff判定类型.来源技能伤害 && 效果判定类型 != Buff判定类型.来源物理伤害)
                                            {
                                                if (效果判定类型 == Buff判定类型.来源特定伤害)
                                                {
                                                    bool flag3;
                                                    if (MapObject == BuffData2.Buff来源)
                                                    {
                                                        HashSet<ushort> 特定技能编号5 = BuffData2.Template.特定技能编号;
                                                        flag3 = (特定技能编号5 != null && 特定技能编号5.Contains(技能.SkillId));
                                                    }
                                                    else
                                                    {
                                                        flag3 = false;
                                                    }
                                                    flag2 = flag3;
                                                }
                                            }
                                            else
                                            {
                                                flag2 = (MapObject == BuffData2.Buff来源);
                                            }
                                            break;
                                        }
                                    case 技能伤害类型.魔法:
                                    case 技能伤害类型.道术:
                                        {
                                            Buff判定类型 效果判定类型 = BuffData2.Template.效果判定类型;
                                            if (效果判定类型 <= Buff判定类型.来源技能伤害)
                                            {
                                                switch (效果判定类型)
                                                {
                                                    case Buff判定类型.所有技能伤害:
                                                    case Buff判定类型.所有魔法伤害:
                                                        flag2 = true;
                                                        goto IL_953;
                                                    case Buff判定类型.所有物理伤害:
                                                    case (Buff判定类型)3:
                                                        goto IL_953;
                                                    case Buff判定类型.所有特定伤害:
                                                        flag2 = BuffData2.Template.特定技能编号.Contains(技能.SkillId);
                                                        goto IL_953;
                                                    default:
                                                        if (效果判定类型 != Buff判定类型.来源技能伤害)
                                                        {
                                                            goto IL_953;
                                                        }
                                                        break;
                                                }
                                            }
                                            else if (效果判定类型 != Buff判定类型.来源魔法伤害)
                                            {
                                                if (效果判定类型 != Buff判定类型.来源特定伤害)
                                                {
                                                    break;
                                                }
                                                bool flag4;
                                                if (MapObject == BuffData2.Buff来源)
                                                {
                                                    HashSet<ushort> 特定技能编号6 = BuffData2.Template.特定技能编号;
                                                    flag4 = (特定技能编号6 != null && 特定技能编号6.Contains(技能.SkillId));
                                                }
                                                else
                                                {
                                                    flag4 = false;
                                                }
                                                flag2 = flag4;
                                                break;
                                            }
                                            flag2 = (MapObject == BuffData2.Buff来源);
                                            break;
                                        }
                                    case 技能伤害类型.毒性:
                                    case 技能伤害类型.神圣:
                                    case 技能伤害类型.灼烧:
                                    case 技能伤害类型.撕裂:
                                        {
                                            Buff判定类型 效果判定类型 = BuffData2.Template.效果判定类型;
                                            if (效果判定类型 != Buff判定类型.所有特定伤害)
                                            {
                                                if (效果判定类型 == Buff判定类型.来源特定伤害)
                                                {
                                                    bool flag5;
                                                    if (MapObject == BuffData2.Buff来源)
                                                    {
                                                        HashSet<ushort> 特定技能编号7 = BuffData2.Template.特定技能编号;
                                                        flag5 = (特定技能编号7 != null && 特定技能编号7.Contains(技能.SkillId));
                                                    }
                                                    else
                                                    {
                                                        flag5 = false;
                                                    }
                                                    flag2 = flag5;
                                                }
                                            }
                                            else
                                            {
                                                HashSet<ushort> 特定技能编号8 = BuffData2.Template.特定技能编号;
                                                flag2 = (特定技能编号8 != null && 特定技能编号8.Contains(技能.SkillId));
                                            }
                                            break;
                                        }
                                }
                            IL_953:
                                if (flag2)
                                {
                                    int v2 = (int)BuffData2.当前层数.V;
                                    int[] 伤害增减基数2 = BuffData2.Template.伤害增减基数;
                                    num = ((伤害增减基数2 != null) ? new int?(伤害增减基数2.Length) : null);
                                    num2 = (int)BuffData2.Buff等级.V;
                                    int num17 = v2 * ((num.GetValueOrDefault() > num2 & num != null) ? BuffData2.Template.伤害增减基数[(int)BuffData2.Buff等级.V] : 0);
                                    float num18 = (float)BuffData2.当前层数.V;
                                    float[] 伤害增减系数2 = BuffData2.Template.伤害增减系数;
                                    num = ((伤害增减系数2 != null) ? new int?(伤害增减系数2.Length) : null);
                                    num2 = (int)BuffData2.Buff等级.V;
                                    float num19 = num18 * ((num.GetValueOrDefault() > num2 & num != null) ? BuffData2.Template.伤害增减系数[(int)BuffData2.Buff等级.V] : 0f);
                                    num11 += ((BuffData2.Template.效果判定方式 == BuffDetherminationMethod.被动受伤增伤) ? num17 : (-num17));
                                    num12 += ((BuffData2.Template.效果判定方式 == BuffDetherminationMethod.被动受伤增伤) ? num19 : (-num19));
                                    MapObject MapObject3;
                                    if (BuffData2.Template.后接Buff编号 != 0 && BuffData2.Buff来源 != null && MapGatewayProcess.Objects.TryGetValue(BuffData2.Buff来源.ObjectId, out MapObject3) && MapObject3 == BuffData2.Buff来源)
                                    {
                                        if (BuffData2.Template.后接技能来源)
                                        {
                                            MapObject.OnAddBuff(BuffData2.Template.后接Buff编号, BuffData2.Buff来源);
                                        }
                                        else
                                        {
                                            this.OnAddBuff(BuffData2.Template.后接Buff编号, BuffData2.Buff来源);
                                        }
                                    }
                                    if (BuffData2.Template.效果判定方式 == BuffDetherminationMethod.被动受伤减伤 && BuffData2.Template.限定伤害上限)
                                    {
                                        num13 = Math.Min(num13, BuffData2.Template.限定伤害数值);
                                    }
                                    if (BuffData2.Template.效果生效移除)
                                    {
                                        this.移除Buff时处理(BuffData2.Id.V);
                                    }
                                }
                            }
                        }
                        float num20 = (num4 + num6) * (float)num9 + (float)num3 + (float)num5 + (float)num11;
                        float val = (float)(num10 - num7) - (float)num10 * num8;
                        float val2 = (num20 - Math.Max(0f, val)) * (1f + num12) * 伤害系数;
                        int 技能伤害 = (int)Math.Min((float)num13, Math.Max(0f, val2));
                        详情.Damage = 技能伤害;
                    }
                }
                this.TimeoutTime = MainProcess.CurrentTime.AddSeconds(10.0);
                MapObject.TimeoutTime = MainProcess.CurrentTime.AddSeconds(10.0);
                if ((详情.Feedback & 技能命中反馈.闪避) == 技能命中反馈.正常)
                {
                    foreach (BuffData BuffData3 in this.Buffs.Values.ToList<BuffData>())
                    {
                        if ((BuffData3.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志 && (BuffData3.Template.角色所处状态 & GameObjectState.失神状态) != GameObjectState.正常状态)
                        {
                            this.移除Buff时处理(BuffData3.Id.V);
                        }
                    }
                }
                MonsterObject MonsterObject2 = this as MonsterObject;
                if (MonsterObject2 != null)
                {
                    MonsterObject2.HardTime = MainProcess.CurrentTime.AddMilliseconds((double)参数.目标硬直时间);
                    if (MapObject is PlayerObject || MapObject is PetObject)
                    {
                        MonsterObject2.HateObject.添加仇恨(MapObject, MainProcess.CurrentTime.AddMilliseconds((double)MonsterObject2.HateTime), 详情.Damage);
                    }
                }
                else
                {
                    PlayerObject PlayerObject = this as PlayerObject;
                    if (PlayerObject != null)
                    {
                        if (详情.Damage > 0)
                        {
                            PlayerObject.装备损失持久(详情.Damage);
                        }
                        if (详情.Damage > 0)
                        {
                            PlayerObject.扣除护盾时间(详情.Damage);
                        }
                        if (PlayerObject.GetRelationship(MapObject) == GameObjectRelationship.敌对)
                        {
                            foreach (PetObject PetObject in PlayerObject.Pets.ToList<PetObject>())
                            {
                                if (PetObject.Neighbors.Contains(MapObject) && !MapObject.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                                {
                                    PetObject.HateObject.添加仇恨(MapObject, MainProcess.CurrentTime.AddMilliseconds((double)PetObject.HateTime), 0);
                                }
                            }
                        }
                        PlayerObject PlayerObject2 = MapObject as PlayerObject;
                        if (PlayerObject2 != null && !this.CurrentMap.自由区内(this.CurrentPosition) && !PlayerObject.灰名玩家 && !PlayerObject.红名玩家)
                        {
                            if (PlayerObject2.红名玩家)
                            {
                                PlayerObject2.减PK时间 = TimeSpan.FromMinutes(1.0);
                            }
                            else
                            {
                                PlayerObject2.灰名时间 = TimeSpan.FromMinutes(1.0);
                            }
                        }
                        else
                        {
                            PetObject PetObject2 = MapObject as PetObject;
                            if (PetObject2 != null && !this.CurrentMap.自由区内(this.CurrentPosition) && !PlayerObject.灰名玩家 && !PlayerObject.红名玩家)
                            {
                                if (PetObject2.PlayerOwner.红名玩家)
                                {
                                    PetObject2.PlayerOwner.减PK时间 = TimeSpan.FromMinutes(1.0);
                                }
                                else
                                {
                                    PetObject2.PlayerOwner.灰名时间 = TimeSpan.FromMinutes(1.0);
                                }
                            }
                        }
                    }
                    else
                    {
                        PetObject PetObject3 = this as PetObject;
                        if (PetObject3 != null)
                        {
                            if (MapObject != PetObject3.PlayerOwner && PetObject3.GetRelationship(MapObject) == GameObjectRelationship.敌对)
                            {
                                PlayerObject 宠物主人 = PetObject3.PlayerOwner;
                                foreach (PetObject PetObject4 in ((宠物主人 != null) ? 宠物主人.Pets.ToList<PetObject>() : null))
                                {
                                    if (PetObject4.Neighbors.Contains(MapObject) && !MapObject.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                                    {
                                        PetObject4.HateObject.添加仇恨(MapObject, MainProcess.CurrentTime.AddMilliseconds((double)PetObject4.HateTime), 0);
                                    }
                                }
                            }
                            if (MapObject != PetObject3.PlayerOwner)
                            {
                                PlayerObject PlayerObject3 = MapObject as PlayerObject;
                                if (PlayerObject3 != null && !this.CurrentMap.自由区内(this.CurrentPosition) && !PetObject3.PlayerOwner.灰名玩家 && !PetObject3.PlayerOwner.红名玩家)
                                {
                                    PlayerObject3.灰名时间 = TimeSpan.FromMinutes(1.0);
                                }
                            }
                        }
                        else
                        {
                            GuardObject GuardInstance = this as GuardObject;
                            if (GuardInstance != null && GuardInstance.GetRelationship(MapObject) == GameObjectRelationship.敌对)
                            {
                                GuardInstance.HateObject.添加仇恨(MapObject, default(DateTime), 0);
                            }
                        }
                    }
                }
                PlayerObject PlayerObject4 = MapObject as PlayerObject;
                if (PlayerObject4 != null)
                {
                    if (PlayerObject4.GetRelationship(this) == GameObjectRelationship.敌对 && !this.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                    {
                        foreach (PetObject PetObject5 in PlayerObject4.Pets.ToList<PetObject>())
                        {
                            if (PetObject5.Neighbors.Contains(this))
                            {
                                PetObject5.HateObject.添加仇恨(this, MainProcess.CurrentTime.AddMilliseconds((double)PetObject5.HateTime), 参数.增加宠物仇恨 ? 详情.Damage : 0);
                            }
                        }
                    }
                    EquipmentData EquipmentData;
                    if (MainProcess.CurrentTime > PlayerObject4.战具计时 && !PlayerObject4.Died && PlayerObject4.CurrentHP < PlayerObject4[GameObjectStats.最大体力] && PlayerObject4.Equipment.TryGetValue(15, out EquipmentData) && EquipmentData.当前持久.V > 0 && (EquipmentData.Id == 99999106 || EquipmentData.Id == 99999107))
                    {
                        PlayerObject4.CurrentHP += ((this is MonsterObject) ? 20 : 10);
                        PlayerObject4.战具损失持久(1);
                        PlayerObject4.战具计时 = MainProcess.CurrentTime.AddMilliseconds(1000.0);
                    }
                }
                if ((this.CurrentHP = Math.Max(0, this.CurrentHP - 详情.Damage)) == 0)
                {
                    详情.Feedback |= 技能命中反馈.死亡;
                    this.Dies(MapObject, true);
                }
                return;
            }
        }


        public void 被动受伤时处理(BuffData 数据)
        {
            int num = 0;
            switch (数据.伤害类型)
            {
                case 技能伤害类型.攻击:
                case 技能伤害类型.刺术:
                case 技能伤害类型.弓术:
                    num = ComputingClass.计算防御(this[GameObjectStats.最小防御], this[GameObjectStats.最大防御]);
                    break;
                case 技能伤害类型.魔法:
                case 技能伤害类型.道术:
                    num = ComputingClass.计算防御(this[GameObjectStats.最小魔防], this[GameObjectStats.最大魔防]);
                    break;
            }
            int num2 = Math.Max(0, 数据.伤害基数.V * (int)数据.当前层数.V - num);
            this.CurrentHP = Math.Max(0, this.CurrentHP - num2);
            触发状态效果 触发状态效果 = new 触发状态效果();
            触发状态效果.Id = 数据.Id.V;
            MapObject buff来源 = 数据.Buff来源;
            触发状态效果.Buff来源 = ((buff来源 != null) ? buff来源.ObjectId : 0);
            触发状态效果.Buff目标 = this.ObjectId;
            触发状态效果.血量变化 = -num2;
            this.SendPacket(触发状态效果);
            if (this.CurrentHP == 0)
            {
                this.Dies(数据.Buff来源, false);
            }
        }


        public void 被动回复时处理(SkillInstance 技能, C_05_计算目标回复 参数)
        {
            if (!this.Died)
            {
                if (this.CurrentMap == 技能.CasterObject.CurrentMap)
                {
                    if (this != 技能.CasterObject && !this.Neighbors.Contains(技能.CasterObject))
                    {
                        return;
                    }
                    TrapObject TrapObject = 技能.CasterObject as TrapObject;
                    MapObject MapObject = (TrapObject != null) ? TrapObject.TrapSource : 技能.CasterObject;
                    int[] 体力回复次数 = 参数.体力回复次数;
                    int? num = (体力回复次数 != null) ? new int?(体力回复次数.Length) : null;
                    int SkillLevel = (int)技能.SkillLevel;
                    int num2 = (num.GetValueOrDefault() > SkillLevel & num != null) ? 参数.体力回复次数[(int)技能.SkillLevel] : 0;
                    byte[] 体力回复基数 = 参数.体力回复基数;
                    num = ((体力回复基数 != null) ? new int?(体力回复基数.Length) : null);
                    SkillLevel = (int)技能.SkillLevel;
                    int num3 = (int)((num.GetValueOrDefault() > SkillLevel & num != null) ? 参数.体力回复基数[(int)技能.SkillLevel] : 0);
                    float[] 道术叠加次数 = 参数.道术叠加次数;
                    num = ((道术叠加次数 != null) ? new int?(道术叠加次数.Length) : null);
                    SkillLevel = (int)技能.SkillLevel;
                    float num4 = (num.GetValueOrDefault() > SkillLevel & num != null) ? 参数.道术叠加次数[(int)技能.SkillLevel] : 0f;
                    float[] 道术叠加基数 = 参数.道术叠加基数;
                    num = ((道术叠加基数 != null) ? new int?(道术叠加基数.Length) : null);
                    SkillLevel = (int)技能.SkillLevel;
                    float num5 = (num.GetValueOrDefault() > SkillLevel & num != null) ? 参数.道术叠加基数[(int)技能.SkillLevel] : 0f;
                    int[] 立即回复基数 = 参数.立即回复基数;
                    num = ((立即回复基数 != null) ? new int?(立即回复基数.Length) : null);
                    SkillLevel = (int)技能.SkillLevel;
                    int num6;
                    if (num.GetValueOrDefault() > SkillLevel & num != null)
                    {
                        if (MapObject == this)
                        {
                            num6 = 参数.立即回复基数[(int)技能.SkillLevel];
                            goto IL_1F1;
                        }
                    }
                    num6 = 0;
                IL_1F1:
                    int num7 = num6;
                    float[] 立即回复系数 = 参数.立即回复系数;
                    num = ((立即回复系数 != null) ? new int?(立即回复系数.Length) : null);
                    SkillLevel = (int)技能.SkillLevel;
                    float num8;
                    if (num.GetValueOrDefault() > SkillLevel & num != null)
                    {
                        if (MapObject == this)
                        {
                            num8 = 参数.立即回复系数[(int)技能.SkillLevel];
                            goto IL_249;
                        }
                    }
                    num8 = 0f;
                IL_249:
                    float num9 = num8;
                    if (num4 > 0f)
                    {
                        num2 += (int)(num4 * (float)ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小道术], MapObject[GameObjectStats.最大道术], MapObject[GameObjectStats.幸运等级]));
                    }
                    if (num5 > 0f)
                    {
                        num3 += (int)(num5 * (float)ComputingClass.CalculateAttack(MapObject[GameObjectStats.最小道术], MapObject[GameObjectStats.最大道术], MapObject[GameObjectStats.幸运等级]));
                    }
                    if (num7 > 0)
                    {
                        this.CurrentHP += num7;
                    }
                    if (num9 > 0f)
                    {
                        this.CurrentHP += (int)((float)this[GameObjectStats.最大体力] * num9);
                    }
                    if (num2 > this.TreatmentCount && num3 > 0)
                    {
                        this.TreatmentCount = (int)((byte)num2);
                        this.TreatmentBase = num3;
                        this.HealTime = MainProcess.CurrentTime.AddMilliseconds(500.0);
                    }
                    return;
                }
            }
        }


        public void 被动回复时处理(BuffData 数据)
        {
            if (数据.Template.体力回复基数 == null)
            {
                return;
            }
            if (数据.Template.体力回复基数.Length <= (int)数据.Buff等级.V)
            {
                return;
            }
            byte b = 数据.Template.体力回复基数[(int)数据.Buff等级.V];
            this.CurrentHP += (int)b;
            触发状态效果 触发状态效果 = new 触发状态效果();
            触发状态效果.Id = 数据.Id.V;
            MapObject buff来源 = 数据.Buff来源;
            触发状态效果.Buff来源 = ((buff来源 != null) ? buff来源.ObjectId : 0);
            触发状态效果.Buff目标 = this.ObjectId;
            触发状态效果.血量变化 = (int)b;
            this.SendPacket(触发状态效果);
        }


        public void ItSelf移动时处理(Point 坐标)
        {
            PlayerObject PlayerObject = this as PlayerObject;
            if (PlayerObject != null)
            {
                PlayerDeals 当前交易 = PlayerObject.当前交易;
                if (当前交易 != null)
                {
                    当前交易.结束交易();
                }
                using (List<BuffData>.Enumerator enumerator = this.Buffs.Values.ToList<BuffData>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        BuffData BuffData = enumerator.Current;
                        SkillTraps 陷阱模板;
                        if ((BuffData.Buff效果 & Buff效果类型.创建陷阱) != Buff效果类型.技能标志 && SkillTraps.DataSheet.TryGetValue(BuffData.Template.触发陷阱技能, out 陷阱模板))
                        {
                            int num = 0;

                            for (; ; )
                            {
                                Point point = ComputingClass.GetFrontPosition(this.CurrentPosition, 坐标, num);
                                if (point == 坐标)
                                {
                                    break;
                                }
                                foreach (Point 坐标2 in ComputingClass.GetLocationRange(point, this.CurrentDirection, BuffData.Template.触发陷阱数量))
                                {
                                    if (!this.CurrentMap.IsBlocked(坐标2))
                                    {
                                        IEnumerable<MapObject> source = this.CurrentMap[坐标2];
                                        Func<MapObject, bool> predicate = null;
                                        if (predicate == null)
                                        {
                                            predicate = delegate (MapObject O)
                                          {
                                              TrapObject TrapObject = O as TrapObject;
                                              return TrapObject != null && TrapObject.陷阱GroupId != 0 && TrapObject.陷阱GroupId == 陷阱模板.分组编号;
                                          };
                                        }
                                        if (source.FirstOrDefault(predicate) == null)
                                        {
                                            this.Traps.Add(new TrapObject(this, 陷阱模板, this.CurrentMap, 坐标2));
                                        }
                                    }
                                }
                                num++;
                            }
                        }
                        if ((BuffData.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志 && (BuffData.Template.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态)
                        {
                            this.移除Buff时处理(BuffData.Id.V);
                        }
                    }
                    goto IL_30E;
                }
            }
            if (this is PetObject)
            {
                foreach (BuffData BuffData2 in this.Buffs.Values.ToList<BuffData>())
                {
                    SkillTraps 陷阱模板;
                    if ((BuffData2.Buff效果 & Buff效果类型.创建陷阱) != Buff效果类型.技能标志 && SkillTraps.DataSheet.TryGetValue(BuffData2.Template.触发陷阱技能, out 陷阱模板))
                    {
                        int num2 = 0;

                        for (; ; )
                        {
                            Point point2 = ComputingClass.GetFrontPosition(this.CurrentPosition, 坐标, num2);
                            if (point2 == 坐标)
                            {
                                break;
                            }
                            foreach (Point 坐标3 in ComputingClass.GetLocationRange(point2, this.CurrentDirection, BuffData2.Template.触发陷阱数量))
                            {
                                if (!this.CurrentMap.IsBlocked(坐标3))
                                {
                                    IEnumerable<MapObject> source2 = this.CurrentMap[坐标3];
                                    Func<MapObject, bool> predicate2 = null;
                                    if (predicate2 == null)
                                    {
                                        predicate2 = delegate (MapObject O)
                                       {
                                           TrapObject TrapObject = O as TrapObject;
                                           return TrapObject != null && TrapObject.陷阱GroupId != 0 && TrapObject.陷阱GroupId == 陷阱模板.分组编号;
                                       };
                                    }
                                    if (source2.FirstOrDefault(predicate2) == null)
                                    {
                                        this.Traps.Add(new TrapObject(this, 陷阱模板, this.CurrentMap, 坐标3));
                                    }
                                }
                            }
                            num2++;
                        }
                    }
                    if ((BuffData2.Buff效果 & Buff效果类型.状态标志) != Buff效果类型.技能标志 && (BuffData2.Template.角色所处状态 & GameObjectState.隐身状态) != GameObjectState.正常状态)
                    {
                        this.移除Buff时处理(BuffData2.Id.V);
                    }
                }
            }
        IL_30E:
            this.UnbindGrid();
            this.CurrentPosition = 坐标;
            this.BindGrid();
            this.更新邻居时处理();
            foreach (MapObject MapObject in this.Neighbors.ToList<MapObject>())
            {
                MapObject.对象移动时处理(this);
            }
        }


        public void NotifyNeightborClear()
        {
            foreach (MapObject MapObject in this.Neighbors.ToList<MapObject>())
            {
                MapObject.对象消失时处理(this);
            }
            this.Neighbors.Clear();
            this.NeighborsImportant.Clear();
            this.NeighborsSneak.Clear();
        }


        public void 更新邻居时处理()
        {
            foreach (MapObject MapObject in this.Neighbors.ToList<MapObject>())
            {
                if (this.CurrentMap != MapObject.CurrentMap || !this.CanBeSeenBy(MapObject))
                {
                    MapObject.对象消失时处理(this);
                    this.对象消失时处理(MapObject);
                }
            }
            for (int i = -20; i <= 20; i++)
            {
                for (int j = -20; j <= 20; j++)
                {
                    this.CurrentMap[new Point(this.CurrentPosition.X + i, this.CurrentPosition.Y + j)].ToList<MapObject>();
                    try
                    {
                        foreach (MapObject MapObject2 in this.CurrentMap[new Point(this.CurrentPosition.X + i, this.CurrentPosition.Y + j)])
                        {
                            if (MapObject2 != this)
                            {
                                if (!this.Neighbors.Contains(MapObject2) && this.IsNeightbor(MapObject2))
                                {
                                    this.对象出现时处理(MapObject2);
                                }
                                if (!MapObject2.Neighbors.Contains(this) && MapObject2.IsNeightbor(this))
                                {
                                    MapObject2.对象出现时处理(this);
                                }
                            }
                        }
                        goto IL_15C;
                    }
                    catch
                    {
                        goto IL_15C;
                    }
                    break;
                IL_15C:;
                }
            }
        }


        public void 对象移动时处理(MapObject 对象)
        {
            if (!(this is ItemObject))
            {
                PetObject PetObject = this as PetObject;
                if (PetObject != null)
                {
                    HateObject.仇恨详情 仇恨详情;
                    if (PetObject.CanAttack(对象) && this.GetDistance(对象) <= PetObject.RangeHate && !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                    {
                        PetObject.HateObject.添加仇恨(对象, default(DateTime), 0);
                    }
                    else if (this.GetDistance(对象) > PetObject.RangeHate && PetObject.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情) && 仇恨详情.仇恨时间 < MainProcess.CurrentTime)
                    {
                        PetObject.HateObject.移除仇恨(对象);
                    }
                }
                else
                {
                    MonsterObject MonsterObject = this as MonsterObject;
                    if (MonsterObject != null)
                    {
                        HateObject.仇恨详情 仇恨详情2;
                        if (this.GetDistance(对象) <= MonsterObject.RangeHate && MonsterObject.CanAttack(对象) && (MonsterObject.VisibleStealthTargets || !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态)))
                        {
                            MonsterObject.HateObject.添加仇恨(对象, default(DateTime), 0);
                        }
                        else if (this.GetDistance(对象) > MonsterObject.RangeHate && MonsterObject.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情2) && 仇恨详情2.仇恨时间 < MainProcess.CurrentTime)
                        {
                            MonsterObject.HateObject.移除仇恨(对象);
                        }
                    }
                    else
                    {
                        TrapObject TrapObject = this as TrapObject;
                        if (TrapObject != null)
                        {
                            if (ComputingClass.GetLocationRange(TrapObject.CurrentPosition, TrapObject.CurrentDirection, TrapObject.ObjectSize).Contains(对象.CurrentPosition))
                            {
                                TrapObject.被动触发陷阱(对象);
                            }
                        }
                        else
                        {
                            GuardObject GuardInstance = this as GuardObject;
                            if (GuardInstance != null)
                            {
                                if (GuardInstance.CanAttack(对象) && this.GetDistance(对象) <= GuardInstance.RangeHate)
                                {
                                    GuardInstance.HateObject.添加仇恨(对象, default(DateTime), 0);
                                }
                                else if (this.GetDistance(对象) > GuardInstance.RangeHate)
                                {
                                    GuardInstance.HateObject.移除仇恨(对象);
                                }
                            }
                        }
                    }
                }
            }
            if (!(对象 is ItemObject))
            {
                PetObject PetObject2 = 对象 as PetObject;
                if (PetObject2 != null)
                {
                    if (PetObject2.GetDistance(this) <= PetObject2.RangeHate && PetObject2.CanAttack(this) && !this.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                    {
                        PetObject2.HateObject.添加仇恨(this, default(DateTime), 0);
                        return;
                    }
                    HateObject.仇恨详情 仇恨详情3;
                    if (PetObject2.GetDistance(this) > PetObject2.RangeHate && PetObject2.HateObject.仇恨列表.TryGetValue(this, out 仇恨详情3) && 仇恨详情3.仇恨时间 < MainProcess.CurrentTime)
                    {
                        PetObject2.HateObject.移除仇恨(this);
                        return;
                    }
                }
                else
                {
                    MonsterObject MonsterObject2 = 对象 as MonsterObject;
                    if (MonsterObject2 != null)
                    {
                        if (MonsterObject2.GetDistance(this) <= MonsterObject2.RangeHate && MonsterObject2.CanAttack(this) && (MonsterObject2.VisibleStealthTargets || !this.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态)))
                        {
                            MonsterObject2.HateObject.添加仇恨(this, default(DateTime), 0);
                            return;
                        }
                        HateObject.仇恨详情 仇恨详情4;
                        if (MonsterObject2.GetDistance(this) > MonsterObject2.RangeHate && MonsterObject2.HateObject.仇恨列表.TryGetValue(this, out 仇恨详情4) && 仇恨详情4.仇恨时间 < MainProcess.CurrentTime)
                        {
                            MonsterObject2.HateObject.移除仇恨(this);
                            return;
                        }
                    }
                    else
                    {
                        TrapObject TrapObject2 = 对象 as TrapObject;
                        if (TrapObject2 != null)
                        {
                            if (ComputingClass.GetLocationRange(TrapObject2.CurrentPosition, TrapObject2.CurrentDirection, TrapObject2.ObjectSize).Contains(this.CurrentPosition))
                            {
                                TrapObject2.被动触发陷阱(this);
                                return;
                            }
                        }
                        else
                        {
                            GuardObject GuardInstance2 = 对象 as GuardObject;
                            if (GuardInstance2 != null)
                            {
                                if (GuardInstance2.CanAttack(this) && GuardInstance2.GetDistance(this) <= GuardInstance2.RangeHate)
                                {
                                    GuardInstance2.HateObject.添加仇恨(this, default(DateTime), 0);
                                    return;
                                }
                                if (GuardInstance2.GetDistance(this) > GuardInstance2.RangeHate)
                                {
                                    GuardInstance2.HateObject.移除仇恨(this);
                                }
                            }
                        }
                    }
                }
            }
        }


        public void 对象出现时处理(MapObject 对象)
        {
            if (this.NeighborsSneak.Remove(对象))
            {
                if (this is PlayerObject PlayerObject)
                {
                    游戏对象类型 对象类型 = 对象.ObjectType;
                    if (对象类型 <= 游戏对象类型.Npcc)
                    {
                        switch (对象类型)
                        {
                            case 游戏对象类型.玩家:
                            case 游戏对象类型.怪物:
                                break;
                            case 游戏对象类型.宠物:
                                PlayerObject.ActiveConnection.SendPacket(new ObjectCharacterStopPacket
                                {
                                    对象编号 = 对象.ObjectId,
                                    对象坐标 = 对象.CurrentPosition,
                                    对象高度 = 对象.CurrentAltitude
                                });
                                PlayerObject.ActiveConnection.SendPacket(new ObjectComesIntoViewPacket
                                {
                                    出现方式 = 1,
                                    对象编号 = 对象.ObjectId,
                                    现身坐标 = 对象.CurrentPosition,
                                    现身高度 = 对象.CurrentAltitude,
                                    现身方向 = (ushort)对象.CurrentDirection,
                                    现身姿态 = ((byte)(对象.Died ? 13 : 1)),
                                    体力比例 = (byte)(对象.CurrentHP * 100 / 对象[GameObjectStats.最大体力])
                                });
                                PlayerObject.ActiveConnection.SendPacket(new SyncObjectHP
                                {
                                    ObjectId = 对象.ObjectId,
                                    CurrentHP = 对象.CurrentHP,
                                    MaxHP = 对象[GameObjectStats.最大体力]
                                });
                                PlayerObject.ActiveConnection.SendPacket(new ObjectTransformTypePacket
                                {
                                    改变类型 = 2,
                                    对象编号 = 对象.ObjectId
                                });
                                goto IL_356;
                            case (游戏对象类型)3:
                                goto IL_356;
                            default:
                                if (对象类型 != 游戏对象类型.Npcc)
                                {
                                    goto IL_356;
                                }
                                break;
                        }
                        PlayerObject.ActiveConnection.SendPacket(new ObjectCharacterStopPacket
                        {
                            对象编号 = 对象.ObjectId,
                            对象坐标 = 对象.CurrentPosition,
                            对象高度 = 对象.CurrentAltitude
                        });
                        SConnection 网络连接 = PlayerObject.ActiveConnection;
                        ObjectComesIntoViewPacket ObjectComesIntoViewPacket = new ObjectComesIntoViewPacket();
                        ObjectComesIntoViewPacket.出现方式 = 1;
                        ObjectComesIntoViewPacket.对象编号 = 对象.ObjectId;
                        ObjectComesIntoViewPacket.现身坐标 = 对象.CurrentPosition;
                        ObjectComesIntoViewPacket.现身高度 = 对象.CurrentAltitude;
                        ObjectComesIntoViewPacket.现身方向 = (ushort)对象.CurrentDirection;
                        ObjectComesIntoViewPacket.现身姿态 = ((byte)(对象.Died ? 13 : 1));
                        ObjectComesIntoViewPacket.体力比例 = (byte)(对象.CurrentHP * 100 / 对象[GameObjectStats.最大体力]);
                        PlayerObject PlayerObject2 = 对象 as PlayerObject;
                        ObjectComesIntoViewPacket.AdditionalParam = ((byte)((PlayerObject2 == null || !PlayerObject2.灰名玩家) ? 0 : 2));
                        网络连接.SendPacket(ObjectComesIntoViewPacket);
                        PlayerObject.ActiveConnection.SendPacket(new SyncObjectHP
                        {
                            ObjectId = 对象.ObjectId,
                            CurrentHP = 对象.CurrentHP,
                            MaxHP = 对象[GameObjectStats.最大体力]
                        });
                    }
                    else if (对象类型 != 游戏对象类型.物品)
                    {
                        if (对象类型 == 游戏对象类型.陷阱)
                        {
                            PlayerObject.ActiveConnection.SendPacket(new TrapComesIntoViewPacket
                            {
                                MapId = 对象.ObjectId,
                                陷阱坐标 = 对象.CurrentPosition,
                                陷阱高度 = 对象.CurrentAltitude,
                                来源编号 = (对象 as TrapObject).TrapSource.ObjectId,
                                Id = (对象 as TrapObject).Id,
                                持续时间 = (对象 as TrapObject).陷阱剩余时间
                            });
                        }
                    }
                    else if (对象 is ItemObject dropObject)
                    {
                        PlayerObject.ActiveConnection.SendPacket(new ObjectDropItemsPacket
                        {
                            DropperObjectId = dropObject.DropperObjectId,
                            ItemObjectId = dropObject.ObjectId,
                            掉落坐标 = dropObject.CurrentPosition,
                            掉落高度 = dropObject.CurrentAltitude,
                            ItemId = dropObject.Id,
                            物品数量 = dropObject.堆叠数量,
                            OwnerPlayerId = dropObject.GetOwnerPlayerIdForDrop(PlayerObject),
                        });
                    }
                IL_356:
                    if (对象.Buffs.Count > 0)
                    {
                        PlayerObject.ActiveConnection.SendPacket(new 同步对象Buff
                        {
                            字节描述 = 对象.对象Buff简述()
                        });
                        return;
                    }
                }
                else if (this is TrapObject TrapObject)
                {
                    if (ComputingClass.GetLocationRange(TrapObject.CurrentPosition, TrapObject.CurrentDirection, TrapObject.ObjectSize).Contains(对象.CurrentPosition))
                    {
                        TrapObject.被动触发陷阱(对象);
                        return;
                    }
                }
                else if (this is PetObject PetObject)
                {
                    if (this.GetDistance(对象) <= PetObject.RangeHate && PetObject.CanAttack(对象) && !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                    {
                        PetObject.HateObject.添加仇恨(对象, default(DateTime), 0);
                        return;
                    }
                    HateObject.仇恨详情 仇恨详情;
                    if (this.GetDistance(对象) > PetObject.RangeHate && PetObject.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情) && 仇恨详情.仇恨时间 < MainProcess.CurrentTime)
                    {
                        PetObject.HateObject.移除仇恨(对象);
                        return;
                    }
                }
                else if (this is MonsterObject MonsterObject)
                {
                    if (this.GetDistance(对象) <= MonsterObject.RangeHate && MonsterObject.CanAttack(对象) && (MonsterObject.VisibleStealthTargets || !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态)))
                    {
                        MonsterObject.HateObject.添加仇恨(对象, default(DateTime), 0);
                        return;
                    }
                    HateObject.仇恨详情 仇恨详情2;
                    if (this.GetDistance(对象) > MonsterObject.RangeHate && MonsterObject.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情2) && 仇恨详情2.仇恨时间 < MainProcess.CurrentTime)
                    {
                        MonsterObject.HateObject.移除仇恨(对象);
                        return;
                    }
                }
            }
            else if (this.Neighbors.Add(对象))
            {
                if (对象 is PlayerObject || 对象 is PetObject)
                    this.NeighborsImportant.Add(对象);

                if (this is PlayerObject PlayerObject3)
                {
                    游戏对象类型 对象类型 = 对象.ObjectType;
                    if (对象类型 <= 游戏对象类型.Npcc)
                    {
                        switch (对象类型)
                        {
                            case 游戏对象类型.玩家:
                            case 游戏对象类型.怪物:
                                break;
                            case 游戏对象类型.宠物:
                                PlayerObject3.ActiveConnection.SendPacket(new ObjectCharacterStopPacket
                                {
                                    对象编号 = 对象.ObjectId,
                                    对象坐标 = 对象.CurrentPosition,
                                    对象高度 = 对象.CurrentAltitude
                                });
                                PlayerObject3.ActiveConnection.SendPacket(new ObjectComesIntoViewPacket
                                {
                                    出现方式 = 1,
                                    对象编号 = 对象.ObjectId,
                                    现身坐标 = 对象.CurrentPosition,
                                    现身高度 = 对象.CurrentAltitude,
                                    现身方向 = (ushort)对象.CurrentDirection,
                                    现身姿态 = ((byte)(对象.Died ? 13 : 1)),
                                    体力比例 = (byte)(对象.CurrentHP * 100 / 对象[GameObjectStats.最大体力])
                                });
                                PlayerObject3.ActiveConnection.SendPacket(new SyncObjectHP
                                {
                                    ObjectId = 对象.ObjectId,
                                    CurrentHP = 对象.CurrentHP,
                                    MaxHP = 对象[GameObjectStats.最大体力]
                                });
                                PlayerObject3.ActiveConnection.SendPacket(new ObjectTransformTypePacket
                                {
                                    改变类型 = 2,
                                    对象编号 = 对象.ObjectId
                                });
                                goto IL_866;
                            case (游戏对象类型)3:
                                goto IL_866;
                            default:
                                if (对象类型 != 游戏对象类型.Npcc)
                                {
                                    goto IL_866;
                                }
                                break;
                        }
                        PlayerObject3.ActiveConnection.SendPacket(new ObjectCharacterStopPacket
                        {
                            对象编号 = 对象.ObjectId,
                            对象坐标 = 对象.CurrentPosition,
                            对象高度 = 对象.CurrentAltitude
                        });
                        SConnection 网络连接2 = PlayerObject3.ActiveConnection;
                        ObjectComesIntoViewPacket ObjectComesIntoViewPacket2 = new ObjectComesIntoViewPacket();
                        ObjectComesIntoViewPacket2.出现方式 = 1;
                        ObjectComesIntoViewPacket2.对象编号 = 对象.ObjectId;
                        ObjectComesIntoViewPacket2.现身坐标 = 对象.CurrentPosition;
                        ObjectComesIntoViewPacket2.现身高度 = 对象.CurrentAltitude;
                        ObjectComesIntoViewPacket2.现身方向 = (ushort)对象.CurrentDirection;
                        ObjectComesIntoViewPacket2.现身姿态 = ((byte)(对象.Died ? 13 : 1));
                        ObjectComesIntoViewPacket2.体力比例 = (byte)(对象.CurrentHP * 100 / 对象[GameObjectStats.最大体力]);
                        PlayerObject PlayerObject4 = 对象 as PlayerObject;
                        ObjectComesIntoViewPacket2.AdditionalParam = ((byte)((PlayerObject4 == null || !PlayerObject4.灰名玩家) ? 0 : 2));
                        网络连接2.SendPacket(ObjectComesIntoViewPacket2);
                        PlayerObject3.ActiveConnection.SendPacket(new SyncObjectHP
                        {
                            ObjectId = 对象.ObjectId,
                            CurrentHP = 对象.CurrentHP,
                            MaxHP = 对象[GameObjectStats.最大体力]
                        });
                    }
                    else if (对象类型 != 游戏对象类型.物品)
                    {
                        if (对象类型 == 游戏对象类型.陷阱)
                        {
                            PlayerObject3.ActiveConnection.SendPacket(new TrapComesIntoViewPacket
                            {
                                MapId = 对象.ObjectId,
                                陷阱坐标 = 对象.CurrentPosition,
                                陷阱高度 = 对象.CurrentAltitude,
                                来源编号 = (对象 as TrapObject).TrapSource.ObjectId,
                                Id = (对象 as TrapObject).Id,
                                持续时间 = (对象 as TrapObject).陷阱剩余时间
                            });
                        }
                        else if (对象 is ChestObject chestObject && chestObject.IsAlredyOpened(PlayerObject3))
                        {
                            PlayerObject3.ActiveConnection.SendPacket(new ChestComesIntoViewPacket
                            {
                                ObjectId = 对象.ObjectId,
                                Direction = (ushort)对象.CurrentDirection,
                                Position = 对象.CurrentPosition,
                                Altitude = 对象.CurrentAltitude,
                                NPCTemplateId = chestObject.Template.宝箱编号,
                            });
                            chestObject.ActivateObject();
                        }
                    }
                    else if (对象 is ItemObject dropObject)
                    {
                        PlayerObject3.ActiveConnection.SendPacket(new ObjectDropItemsPacket
                        {
                            DropperObjectId = dropObject.DropperObjectId,
                            ItemObjectId = dropObject.ObjectId,
                            掉落坐标 = dropObject.CurrentPosition,
                            掉落高度 = dropObject.CurrentAltitude,
                            ItemId = dropObject.Id,
                            物品数量 = dropObject.堆叠数量,
                            OwnerPlayerId = dropObject.GetOwnerPlayerIdForDrop(PlayerObject3),
                        });
                    }
                IL_866:
                    if (对象.Buffs.Count > 0)
                    {
                        PlayerObject3.ActiveConnection.SendPacket(new 同步对象Buff
                        {
                            字节描述 = 对象.对象Buff简述()
                        });
                        return;
                    }
                }
                else if (this is TrapObject TrapObject2)
                {
                    if (ComputingClass.GetLocationRange(TrapObject2.CurrentPosition, TrapObject2.CurrentDirection, TrapObject2.ObjectSize).Contains(对象.CurrentPosition))
                    {
                        TrapObject2.被动触发陷阱(对象);
                        return;
                    }
                }
                else if (this is PetObject PetObject2)
                {
                    if (!this.Died)
                    {
                        if (this.GetDistance(对象) <= PetObject2.RangeHate && PetObject2.CanAttack(对象) && !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                        {
                            PetObject2.HateObject.添加仇恨(对象, default(DateTime), 0);
                            return;
                        }
                        HateObject.仇恨详情 仇恨详情3;
                        if (this.GetDistance(对象) > PetObject2.RangeHate && PetObject2.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情3) && 仇恨详情3.仇恨时间 < MainProcess.CurrentTime)
                        {
                            PetObject2.HateObject.移除仇恨(对象);
                            return;
                        }
                    }
                }
                else if (this is MonsterObject MonsterObject2)
                {
                    if (!this.Died)
                    {
                        HateObject.仇恨详情 仇恨详情4;
                        if (this.GetDistance(对象) <= MonsterObject2.RangeHate && MonsterObject2.CanAttack(对象) && (MonsterObject2.VisibleStealthTargets || !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态)))
                        {
                            MonsterObject2.HateObject.添加仇恨(对象, default(DateTime), 0);
                        }
                        else if (this.GetDistance(对象) > MonsterObject2.RangeHate && MonsterObject2.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情4) && 仇恨详情4.仇恨时间 < MainProcess.CurrentTime)
                        {
                            MonsterObject2.HateObject.移除仇恨(对象);
                        }
                        if (this.NeighborsImportant.Count != 0)
                        {
                            MonsterObject2.怪物激活处理();
                            return;
                        }
                    }
                }
                else if (this is GuardObject GuardInstance)
                {
                    if (!this.Died)
                    {
                        if (GuardInstance.CanAttack(对象) && this.GetDistance(对象) <= GuardInstance.RangeHate)
                        {
                            GuardInstance.HateObject.添加仇恨(对象, default(DateTime), 0);
                        }
                        else if (this.GetDistance(对象) > GuardInstance.RangeHate)
                        {
                            GuardInstance.HateObject.移除仇恨(对象);
                        }
                        if (this.NeighborsImportant.Count != 0)
                        {
                            GuardInstance.守卫激活处理();
                        }
                    }
                }
            }
        }


        public void 对象消失时处理(MapObject 对象)
        {
            if (this.Neighbors.Remove(对象))
            {
                this.NeighborsSneak.Remove(对象);
                this.NeighborsImportant.Remove(对象);
                if (!(this is ItemObject))
                {
                    PlayerObject PlayerObject = this as PlayerObject;
                    if (PlayerObject != null)
                    {
                        PlayerObject.ActiveConnection.SendPacket(new ObjectOutOfViewPacket
                        {
                            对象编号 = 对象.ObjectId
                        });
                        return;
                    }
                    PetObject PetObject = this as PetObject;
                    if (PetObject != null)
                    {
                        PetObject.HateObject.移除仇恨(对象);
                        return;
                    }
                    MonsterObject MonsterObject = this as MonsterObject;
                    if (MonsterObject != null)
                    {
                        if (!this.Died)
                        {
                            MonsterObject.HateObject.移除仇恨(对象);
                            if (MonsterObject.NeighborsImportant.Count == 0)
                            {
                                MonsterObject.怪物沉睡处理();
                                return;
                            }
                        }
                    }
                    else
                    {
                        GuardObject GuardInstance = this as GuardObject;
                        if (GuardInstance != null && !this.Died)
                        {
                            GuardInstance.HateObject.移除仇恨(对象);
                            if (GuardInstance.NeighborsImportant.Count == 0)
                            {
                                GuardInstance.守卫沉睡处理();
                            }
                        }
                    }
                }
            }
        }


        public void NotifyObjectDies(MapObject 对象)
        {
            MonsterObject MonsterObject = this as MonsterObject;
            if (MonsterObject != null)
            {
                MonsterObject.HateObject.移除仇恨(对象);
                return;
            }
            PetObject PetObject = this as PetObject;
            if (PetObject != null)
            {
                PetObject.HateObject.移除仇恨(对象);
                return;
            }
            GuardObject GuardInstance = this as GuardObject;
            if (GuardInstance != null)
            {
                GuardInstance.HateObject.移除仇恨(对象);
                return;
            }
        }


        public void 对象隐身时处理(MapObject 对象)
        {
            PetObject PetObject = this as PetObject;
            if (PetObject != null && PetObject.HateObject.仇恨列表.ContainsKey(对象))
            {
                PetObject.HateObject.移除仇恨(对象);
            }
            MonsterObject MonsterObject = this as MonsterObject;
            if (MonsterObject != null && MonsterObject.HateObject.仇恨列表.ContainsKey(对象) && !MonsterObject.VisibleStealthTargets)
            {
                MonsterObject.HateObject.移除仇恨(对象);
            }
        }


        public void 对象潜行时处理(MapObject 对象)
        {
            PetObject PetObject = this as PetObject;
            if (PetObject != null)
            {
                if (PetObject.HateObject.仇恨列表.ContainsKey(对象))
                {
                    PetObject.HateObject.移除仇恨(对象);
                }
                this.NeighborsSneak.Add(对象);
            }
            MonsterObject MonsterObject = this as MonsterObject;
            if (MonsterObject != null && !MonsterObject.VisibleStealthTargets)
            {
                if (MonsterObject.HateObject.仇恨列表.ContainsKey(对象))
                {
                    MonsterObject.HateObject.移除仇恨(对象);
                }
                this.NeighborsSneak.Add(对象);
            }
            PlayerObject PlayerObject = this as PlayerObject;
            if (PlayerObject != null && (this.GetRelationship(对象) == GameObjectRelationship.敌对 || 对象.GetRelationship(this) == GameObjectRelationship.敌对))
            {
                this.NeighborsSneak.Add(对象);
                PlayerObject.ActiveConnection.SendPacket(new ObjectOutOfViewPacket
                {
                    对象编号 = 对象.ObjectId
                });
            }
        }


        public void 对象显隐时处理(MapObject 对象)
        {
            PetObject PetObject = this as PetObject;
            if (PetObject != null)
            {
                HateObject.仇恨详情 仇恨详情;
                if (this.GetDistance(对象) <= PetObject.RangeHate && PetObject.CanAttack(对象) && !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态))
                {
                    PetObject.HateObject.添加仇恨(对象, default(DateTime), 0);
                }
                else if (this.GetDistance(对象) > PetObject.RangeHate && PetObject.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情) && 仇恨详情.仇恨时间 < MainProcess.CurrentTime)
                {
                    PetObject.HateObject.移除仇恨(对象);
                }
            }
            MonsterObject MonsterObject = this as MonsterObject;
            if (MonsterObject != null)
            {
                if (this.GetDistance(对象) <= MonsterObject.RangeHate && MonsterObject.CanAttack(对象) && (MonsterObject.VisibleStealthTargets || !对象.CheckStatus(GameObjectState.隐身状态 | GameObjectState.潜行状态)))
                {
                    MonsterObject.HateObject.添加仇恨(对象, default(DateTime), 0);
                    return;
                }
                HateObject.仇恨详情 仇恨详情2;
                if (this.GetDistance(对象) > MonsterObject.RangeHate && MonsterObject.HateObject.仇恨列表.TryGetValue(对象, out 仇恨详情2) && 仇恨详情2.仇恨时间 < MainProcess.CurrentTime)
                {
                    MonsterObject.HateObject.移除仇恨(对象);
                }
            }
        }


        public void 对象显行时处理(MapObject 对象)
        {
            if (this.NeighborsSneak.Contains(对象))
            {
                this.对象出现时处理(对象);
            }
        }


        public byte[] 对象Buff详述()
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream(34))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((byte)this.Buffs.Count);
                    foreach (KeyValuePair<ushort, BuffData> keyValuePair in this.Buffs)
                    {
                        binaryWriter.Write(keyValuePair.Value.Id.V);
                        binaryWriter.Write((int)keyValuePair.Value.Id.V);
                        binaryWriter.Write(keyValuePair.Value.当前层数.V);
                        binaryWriter.Write((int)keyValuePair.Value.剩余时间.V.TotalMilliseconds);
                        binaryWriter.Write((int)keyValuePair.Value.持续时间.V.TotalMilliseconds);
                    }
                    result = memoryStream.ToArray();
                }
            }
            return result;
        }


        public byte[] 对象Buff简述()
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream(34))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(ObjectId);
                    int num = 0;
                    foreach (KeyValuePair<ushort, BuffData> keyValuePair in this.Buffs)
                    {
                        binaryWriter.Write(keyValuePair.Value.Id.V);
                        binaryWriter.Write((int)keyValuePair.Value.Id.V);
                        if (++num >= 5)
                        {
                            break;
                        }
                    }
                    result = memoryStream.ToArray();
                }
            }
            return result;
        }
    }
}
