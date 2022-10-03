using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Templates
{
    public class InscriptionItems   //出生物品
    {
        public static Dictionary<byte, InscriptionItems> DataSheet; //出生物品 数据表
        public static InscriptionItems[] AllInscriptionItems;  //出生物品 所有物品项目

        public byte Id;
        public ItemBackPack 角色背包;  //Backpack
        public GameObjectRace[] 需要职业 = new GameObjectRace[0];  //NeedRace
        public GameObjectGender? 需要性别;  //NeedGender
        public ushort? 数量;   //Quantity   
        public int 物品编号;  //ItemId

        public static void LoadData()
        {
            DataSheet = new Dictionary<byte, InscriptionItems>();
            string path = Config.GameDataPath + "\\System\\物品数据\\出生物品\\";
            AllInscriptionItems = Serializer.Deserialize<InscriptionItems>(path);
            foreach (var inscriptionItem in AllInscriptionItems)
                DataSheet.Add(inscriptionItem.Id, inscriptionItem);
        }
    }
}
