using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Templates
{
    public class GameItemTreasure  //GameItem宝盒
    {
        public string 物品名字 { get; set; }  //ItemName
        public GameObjectRace? 需要职业 { get; set; } = null;  //游戏对象职业? NeedRace
        public int? 概率 { get; set; } = null;   //Rate
    }
}
