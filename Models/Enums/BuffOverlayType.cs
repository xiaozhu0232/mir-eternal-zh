using System;

namespace GameServer.Templates
{
	public enum BuffOverlayType    //Buff叠加类型
	{
		SuperpositionDisabled,  //禁止叠加
		SimilarReplacement,   //同类替换
		HomogeneousStacking,  //同类叠加
		SimilarDelay   //同类延时
	}
}
