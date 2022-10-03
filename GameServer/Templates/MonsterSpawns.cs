using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameServer.Templates
{
	public class MonsterSpawns  //怪物刷新
	{
		public static HashSet<MonsterSpawns> DataSheet; //怪物刷新  数据表

		public byte 所处地图;  //FromMapId
		public string 所处地名;   //FromMapName
		public Point 所处坐标;   //FromCoords
		public string 区域名字;   //RegionName
		public int 区域半径;    //AreaRadius
		public 刷新信息[] 刷新列表;   //刷新信息 Spawns
		public HashSet<Point> 范围坐标;   //RangeCoords

		public static void LoadData()
		{
			DataSheet = new HashSet<MonsterSpawns>();
			string text = Config.GameDataPath + "\\System\\游戏地图\\怪物刷新\\";
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<MonsterSpawns>(text))
					DataSheet.Add(obj);
			}
		}
	}
}
