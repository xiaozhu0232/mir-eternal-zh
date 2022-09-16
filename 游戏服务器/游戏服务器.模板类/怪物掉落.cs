namespace 游戏服务器.模板类;

public class 怪物掉落
{
	public string 物品名字;

	public string 怪物名字;

	public int 掉落概率;

	public int 最小数量;

	public int 最大数量;

	public override string ToString()
	{
		return $"{怪物名字} - {物品名字} - {掉落概率} - {最小数量}/{最大数量}";
	}

	static 怪物掉落()
	{
	}
}
