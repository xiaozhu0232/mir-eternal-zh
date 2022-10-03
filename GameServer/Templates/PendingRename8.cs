using System;
using System.Collections.Generic;
using GameServer.Data;

namespace GameServer.Templates
{
	
	public sealed class 回购排序 : IComparer<ItemData>  //物品数据
	{
		
		public int Compare(ItemData a, ItemData b)  //物品数据  物品数据
		{
			return b.PurchaseId.CompareTo(a.PurchaseId);   //	回购编号  回购编号
		}

		
		public 回购排序()
		{
			
			
		}
	}
}
