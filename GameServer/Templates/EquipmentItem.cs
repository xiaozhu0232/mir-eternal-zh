using System;

namespace GameServer.Templates
{
	public class EquipmentItem : GameItems  //游戏装备 游戏物品
	{
		public bool 死亡销毁;  //DestroyOnDeath
		public bool 禁止卸下;   //DisableDismount
		public bool 能否修理;   //CanRepair
		public int 修理花费;   //RepairCost
		public int 特修花费;   //SpecialRepairCost
		public int 需要攻击;   //NeedAttack
		public int 需要魔法;   //NeedMagic
		public int 需要道术;   //NeedTaoism
		public int 需要刺术;   //NeedAcupuncture
		public int 需要弓术;   //NeedArchery
		public int 基础战力;   //BasicPowerCombat
		public int 最小攻击;   //MinDC
		public int 最大攻击;     //MaxDC
		public int 最小魔法;    //MinMC
		public int 最大魔法;    //MaxMC
		public int 最小道术;    //MinSC
		public int 最大道术;    //MaxSC
		public int 最小刺术;    //MinNC
		public int 最大刺术;    //MaxNC
		public int 最小弓术;    //MinBC
		public int 最大弓术;    //MaxBC
		public int 最小圣伤;   //MinHC
		public int 最大圣伤;   //MaxHC
		public int 最小防御;   //MinDef
		public int 最大防御;   //MaxDef
		public int 最小魔防;  //MinMCDef
		public int 最大魔防;   //MaxMCDef
		public int 最大体力;     //MaxHP
		public int 最大魔力;     //MaxMP
		public int 物理准确;  //PhysicallyAccurate
		public int 物理敏捷;   //PhysicalAgility
		public int 攻击速度;   //AttackSpeed
		public int 魔法闪避;   //MagicDodge
		public int 打孔上限;  //PunchUpperLimit
		public int 一孔花费;   //CostPerHole
		public int 二孔花费;   //TwoHoleCost
		public int 重铸灵石;  //ReforgedSpiritStone
		public int 灵石数量;  //NumberSpiritStones
		public int 金币数量;  //NumberGoldCoins
		public GameEquipmentSet 装备套装;  //游戏装备套装 EquipSet

		public byte Location
		{
			get
			{
				switch (物品分类)
				{
					case 物品使用分类.武器:
						return 0;
					case 物品使用分类.衣服:
						return 1;

				}
				return byte.MaxValue;
			}
		}

	}
}
