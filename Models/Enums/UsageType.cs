using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Enums
{
    public enum UsageType  //物品使用类型
    {
        无 = 0,  //Unknown
        恢复HP = 1,   //RecoveryHP
        恢复MP = 2,    //RecoveryMP
        药物 = 3,     //Medicine
        随机传送 = 4,   //RandomTeleport
        宝盒 = 5,     //Treasure
        捆绑解包 = 6,   //UnpackStack 
        获得元宝 = 7,    //GainIngots
        城镇传送 = 8,   //TownTeleport
        祝福油 = 9,     //Blessing
        切换技能 = 10,  //切换技能 SwitchSkill
        获取坐骑 = 11, //获取坐骑   AdquireMount
        高级祝福油 = 12,  //
        获得经验 = 13,   //
        惩罚PK值 = 14 
    }
}
