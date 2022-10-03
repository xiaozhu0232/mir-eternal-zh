using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Templates
{
    public class MapChest  //宝箱刷新
    {
        public static HashSet<MapChest> DataSheet;

        public int 宝箱编号 { get; set; }  //ChestId
        public int 所处地图 { get; set; }    //MapId
        public Point 所处坐标 { get; set; }  //Point Coords
        public GameDirection 所处方向 { get; set; }  //游戏方向 Direction

        public static void LoadData()
        {
            string text = Config.GameDataPath + "\\System\\游戏地图\\宝箱刷新\\";

            if (Directory.Exists(text))
                DataSheet = new HashSet<MapChest>(Serializer.Deserialize<MapChest>(text));
            else
                DataSheet = new HashSet<MapChest>();
        }

    }
}
