using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameServer.Templates
{
	public sealed class MapGuards //守卫刷新
	{
		public static HashSet<MapGuards> DataSheet;  //

		public ushort 守卫编号;  //GuardNumber
		public byte 所处地图;  //FromMapId
		public string 所处地名;  //FromMapName
		public Point 所处坐标; //FromCoords
		public GameDirection 所处方向;  //游戏方向 Direction
		public string 区域名字;  //RegionName

		public static void LoadData()
		{
			DataSheet = new HashSet<MapGuards>();
			string text = Config.GameDataPath + "\\System\\游戏地图\\守卫刷新\\";
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<MapGuards>(text))
					DataSheet.Add(obj);
			}
		}
	}
}
