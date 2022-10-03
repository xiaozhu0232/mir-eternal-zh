using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Templates
{
    public class ChestTemplate  //地图宝箱
    {
        public static Dictionary<int, ChestTemplate> DataSheet;

        public int 宝箱编号 { get; set; } //Id
        public string 宝箱名字 { get; set; }  //Name
        public GameItemTreasure[] 物品 { get; set; } //Items


        public static void LoadData()
        {
            string text = Config.GameDataPath + "\\System\\Npc数据\\宝箱数据\\";

            if (Directory.Exists(text))
                DataSheet = Serializer.Deserialize<ChestTemplate>(text).ToDictionary(x => x.宝箱编号);
            else
                DataSheet = new Dictionary<int, ChestTemplate>();
        }
    }
}
