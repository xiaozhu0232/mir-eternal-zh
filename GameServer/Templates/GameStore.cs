using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameServer.Data;

namespace GameServer.Templates
{
    public sealed class GameStore  //游戏商店
    {
        public static byte[] 商店文件数据;  //StoreBuffer
        public static int 商店文件效验;   //StoreVersion
        public static int 商店物品数量;  //StoreItemsCounts
        public static int 商店回购排序;   //ItemsSort
        public static Dictionary<int, GameStore> DataSheet;  //游戏商店> 数据表;

        public int 商店编号;  //StoreId
        public string 商店名字;   //Name
        public ItemsForSale 回收类型;  //物品出售分类 RecyclingType
        public List<GameStoreItem> 商品列表;  //游戏商品  Products
        public SortedSet<ItemData> AvailableItems = new SortedSet<ItemData>(new 回购排序()); //AvailableItems  ItemData 物品数据

        public static void LoadData()
        {
            DataSheet = new Dictionary<int, GameStore>();
            string text = Config.GameDataPath + "\\System\\物品数据\\游戏商店\\";
            if (Directory.Exists(text))
            {
                foreach (var obj in Serializer.Deserialize<GameStore>(text))
                    DataSheet.Add(obj.商店编号, obj);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                var items = (from X in DataSheet.Values.ToList()
                             orderby X.商店编号
                             select X).ToList();

                foreach (GameStore store in items)
                {
                    foreach (GameStoreItem product in store.商品列表) //Products
                    {
                        var name = Encoding.UTF8.GetBytes(store.商店名字);

                        binaryWriter.Write(store.商店编号);
                        binaryWriter.Write(name);
                        binaryWriter.Write(new byte[64 - name.Length]);
                        binaryWriter.Write(product.商品编号);
                        binaryWriter.Write(product.单位数量);
                        binaryWriter.Write(product.货币类型);
                        binaryWriter.Write(product.商品价格);
                        binaryWriter.Write(-1);
                        binaryWriter.Write(0);
                        binaryWriter.Write(-1);
                        binaryWriter.Write(0);
                        binaryWriter.Write(0);
                        binaryWriter.Write(0);
                        binaryWriter.Write((int)store.回收类型);
                        binaryWriter.Write(0);
                        binaryWriter.Write(0);
                        binaryWriter.Write((ushort)0);
                        binaryWriter.Write(-1);
                        binaryWriter.Write(-1);
                        商店物品数量++;
                    }
                }

                var buffer = memoryStream.ToArray();

                商店文件数据 = Serializer.Compress(buffer);

                商店文件效验 = 0;

                foreach (byte b in GameStore.商店文件数据)
                    商店文件效验 += (int)b;
            }
        }

        public bool BuyItem(ItemData item)
        {
            return this.AvailableItems.Remove(item);
        }

        public void SellItem(ItemData item)
        {
            item.PurchaseId = ++商店回购排序;
            if (this.AvailableItems.Add(item) && this.AvailableItems.Count > 50)
            {
                ItemData ItemData = this.AvailableItems.Last<ItemData>();
                this.AvailableItems.Remove(ItemData);
                ItemData.Delete();
            }
        }
    }
}
