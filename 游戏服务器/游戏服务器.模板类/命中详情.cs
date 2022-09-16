using System.Collections.Generic;
using System.IO;
using System.Linq;
using 游戏服务器.地图类;

namespace 游戏服务器.模板类;

public class 命中详情
{
	public int 技能伤害;

	public ushort 招架伤害;

	public 地图对象 技能目标;

	public 技能命中反馈 技能反馈;

	public 命中详情(地图对象 目标)
	{
		技能目标 = 目标;
	}

	public 命中详情(地图对象 目标, 技能命中反馈 反馈)
	{
		技能目标 = 目标;
		技能反馈 = 反馈;
	}

	public static byte[] 命中描述(Dictionary<int, 命中详情> 命中列表, int 命中延迟)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((byte)命中列表.Count);
		foreach (KeyValuePair<int, 命中详情> item in 命中列表.ToList())
		{
			binaryWriter.Write(item.Value.技能目标.地图编号);
			binaryWriter.Write((ushort)item.Value.技能反馈);
			binaryWriter.Write((ushort)命中延迟);
		}
		return memoryStream.ToArray();
	}

	static 命中详情()
	{
	}
}
