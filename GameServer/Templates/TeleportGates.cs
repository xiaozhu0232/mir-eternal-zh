using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameServer.Templates
{
	public class TeleportGates  //传送法阵
	{
		public static List<TeleportGates> DataSheet;  //传送法阵	数据表

		public byte 法阵编号;  //TeleportGateNumber
		public byte 所处地图;  //FromMapId
		public byte 跳转地图;   //ToMapId
		public string 法阵名字;  //TeleportGateName
		public string 所处地名;  //FromMapName
		public string 跳转地名;   //ToMapName
		public string 所处别名;  //FromMapFile
		public string 跳转别名;  //ToMapFile
		public Point 所处坐标; //FromCoords
		public Point 跳转坐标; //ToCoords

		public static void LoadData()
		{
			DataSheet = new List<TeleportGates>();
			string text = Config.GameDataPath + "\\System\\游戏地图\\法阵数据\\";
			if (Directory.Exists(text))
			{
				foreach (var obj in Serializer.Deserialize<TeleportGates>(text))
					DataSheet.Add(obj);
			}
		}
	}
}
