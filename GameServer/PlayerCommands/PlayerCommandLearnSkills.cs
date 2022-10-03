using GameServer.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.PlayerCommands
{
    public class PlayerCommandLearnSkills : PlayerCommand
    {
        public override void Execute()
        {
            foreach (var item in GameItems.DataSheet)
            {
                if (item.Value.物品分类 != 物品使用分类.技能书籍) continue;
                if (item.Value.需要职业 != Player.CharRole) continue;
                if (item.Value.附加技能 <= 0) continue;

                Player.LearnSkill(item.Value.附加技能);
            }
        }
    }
}
