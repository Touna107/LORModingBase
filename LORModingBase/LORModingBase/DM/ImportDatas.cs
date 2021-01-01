﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LORModingBase.DM
{
    /// <summary>
    /// Import all datas
    /// </summary>
    class ImportDatas
    {
        public static string TARGET_MODE_DIC = "";

        /// <summary>
        /// Import all datas
        /// </summary>
        /// <param name="dicToLoad">Target directory to load</param>
        public static void ImportAllDatas(string dicToLoad)
        {
            MainWindow.criticalPageInfos.Clear();
            TARGET_MODE_DIC = dicToLoad;

            ImportDatas_CriticalPages();
            ImportDatas_CriticalPageDescription();
        }

        /// <summary>
        /// Import ciritical pages info datas
        /// </summary>
        public static void ImportDatas_CriticalPages()
        {
            string EQUIP_PAGE_PATH = $"{TARGET_MODE_DIC}\\StaticInfo\\EquipPage\\EquipPage.txt";
            if (!File.Exists(EQUIP_PAGE_PATH))
                return;

            XmlNodeList bookNodes = Tools.XmlFile.SelectNodeLists(EQUIP_PAGE_PATH, "//Book");
            foreach (XmlNode bookNode in bookNodes)
            {
                DS.CriticalPageInfo criticalPageInfo = new DS.CriticalPageInfo();
                if (bookNode.Attributes["ID"] != null)
                    criticalPageInfo.bookID = bookNode.Attributes["ID"].Value;
                criticalPageInfo.name = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Name");

                criticalPageInfo.iconName = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "BookIcon");
                if(!string.IsNullOrEmpty(criticalPageInfo.iconName))
                {
                    DS.DropBookInfo foundDropBookInfo = DM.StaticInfos.dropBookInfos.Find((DS.DropBookInfo dropInfo) =>
                    {
                        return dropInfo.iconName == criticalPageInfo.iconName;
                    });
                    if (foundDropBookInfo != null)
                        criticalPageInfo.iconDes = $"{foundDropBookInfo.iconDesc}:{criticalPageInfo.iconName}";
                }

                criticalPageInfo.episode = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Episode");
                if (!string.IsNullOrEmpty(criticalPageInfo.episode))
                {
                    DS.StageInfo foundStageInfo = DM.StaticInfos.stageInfos.Find((DS.StageInfo stageInfo) =>
                    {
                        return stageInfo.stageID == criticalPageInfo.episode;
                    });
                    if (foundStageInfo != null)
                    {
                        criticalPageInfo.chapter = foundStageInfo.Chapter;
                        criticalPageInfo.episodeDes = $"{DS.GameInfo.chapter_Dic[foundStageInfo.Chapter]} / {foundStageInfo.stageDoc}:{foundStageInfo.stageID}";
                    }
                }

                criticalPageInfo.rarity = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Rarity");

                criticalPageInfo.skinName = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "CharacterSkin");
                if (!string.IsNullOrEmpty(criticalPageInfo.skinName))
                {
                    DS.BookSkinInfo foundSkinInfo = DM.StaticInfos.bookSkinInfos.Find((DS.BookSkinInfo skinInfo) =>
                    {
                        return skinInfo.skinName == criticalPageInfo.skinName;
                    });
                    if (foundSkinInfo != null)
                        criticalPageInfo.skinDes = $"{foundSkinInfo.skinDesc}:{foundSkinInfo.skinName}";
                }


                XmlNode equipEffectNode = bookNode.SelectSingleNode("//EquipEffect");
                if(equipEffectNode != null)
                {
                    criticalPageInfo.HP = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "HP");
                    criticalPageInfo.breakNum = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "Break");
                    criticalPageInfo.minSpeedCount = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "SpeedMin");
                    criticalPageInfo.maxSpeedCount = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "Speed");

                    criticalPageInfo.SResist = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "SResist");
                    criticalPageInfo.PResist = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "PResist");
                    criticalPageInfo.HResist = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "HResist");

                    criticalPageInfo.BSResist = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "SBResist");
                    criticalPageInfo.BPResist = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "PBResist");
                    criticalPageInfo.BHResist = Tools.XmlFile.GetXmlNodeSafe.ToString(equipEffectNode, "HBResist");

                    XmlNodeList passiveNodes = bookNode.SelectNodes("//Passive");
                    foreach (XmlNode passiveNode in passiveNodes)
                    {
                        if (string.IsNullOrEmpty(passiveNode.InnerText)) 
                            continue;
                        if (DS.FilterDatas.EXCLUDE_PASSIVE_CODE.Contains(passiveNode.InnerText))
                            continue;

                        string foundPassiveDesc = DM.StaticInfos.passiveList.Find((string passiveDesc) =>
                        {
                            return passiveDesc.Split(':').Last() == passiveNode.InnerText;
                        });
                        if (!string.IsNullOrEmpty(foundPassiveDesc))
                            criticalPageInfo.passiveIDs.Add(foundPassiveDesc);
                    }
                }
                MainWindow.criticalPageInfos.Add(criticalPageInfo);
            }
        }

        /// <summary>
        /// Import page description
        /// </summary>
        public static void ImportDatas_CriticalPageDescription()
        {
            string BOOKS_PATH = $"{TARGET_MODE_DIC}\\Localize\\kr\\Books\\_Books.txt";
            if (!File.Exists(BOOKS_PATH))
                return;

            XmlNodeList bookDescNodes = Tools.XmlFile.SelectNodeLists(BOOKS_PATH, "//BookDesc");
            foreach (XmlNode bookDescNode in bookDescNodes)
            {
                if (bookDescNode.Attributes["BookID"] == null)
                    continue;

                DS.CriticalPageInfo foundCriticalPageInfo = MainWindow.criticalPageInfos.Find((DS.CriticalPageInfo criticalPageInfo) =>
                {
                    return criticalPageInfo.bookID == bookDescNode.Attributes["BookID"].Value;
                });
                if (foundCriticalPageInfo == null)
                    continue;

                XmlNodeList descNodes = bookDescNode.SelectNodes("//Desc");
                if (descNodes.Count > 0)
                    foundCriticalPageInfo.description = "";

                for (int descNodeIndex=0; descNodeIndex<descNodes.Count; descNodeIndex++)
                {
                    if (descNodeIndex > 0)
                        foundCriticalPageInfo.description += "\r\n\r\n";
                    foundCriticalPageInfo.description += descNodes[descNodeIndex].InnerText;
                }
            }
        }
    }
}
