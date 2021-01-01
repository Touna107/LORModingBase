﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace LORModingBase.DM
{
    /// <summary>
    /// Game episode data managerment
    /// </summary>
    class StaticInfos
    {
        /// <summary>
        /// Loaded stage infos
        /// </summary>
        public static List<DS.StageInfo> stageInfos = new List<DS.StageInfo>();

        /// <summary>
        /// Loaded passive infos
        /// </summary>
        public static Dictionary<string, List<DS.PassiveInfo>> passiveInfos = new Dictionary<string, List<DS.PassiveInfo>>();
        public static List<string> passiveList = new List<string>();

        /// <summary>
        /// Book skin infos
        /// </summary>
        public static List<DS.BookSkinInfo> bookSkinInfos = new List<DS.BookSkinInfo>();
        /// <summary>
        /// Critical page infos in game
        /// </summary>
        public static List<DS.CriticalPageInfo> gameCriticalPageInfos = new List<DS.CriticalPageInfo>();

        /// <summary>
        /// Book icon infos
        /// </summary>
        public static List<DS.DropBookInfo> dropBookInfos = new List<DS.DropBookInfo>();

        /// <summary>
        /// Load all static datas
        /// </summary>
        public static void LoadAllDatas()
        {
            LoadDatas_PassiveInfo();
            LoadDatas_StageInfo();
            LoadDatas_SkinAndBookIconInfo();
        }

        /// <summary>
        /// Load stage info datas
        /// </summary>
        public static void LoadDatas_StageInfo()
        {
            stageInfos.Clear();
            XmlNodeList stageNodeList =  Tools.XmlFile.SelectNodeLists($"{DM.Config.config.LORFolderPath}\\{DS.PATH.RELATIVE_DIC_LOR_MODE_RESOURCES_STATIC_INFO}\\StageInfo\\StageInfo.txt",
                "//Stage");

            foreach(XmlNode stageNode in stageNodeList)
            {
                if (stageNode.Attributes["id"] == null 
                    || stageNode["Chapter"] == null
                    || stageNode["Name"] == null)
                    continue;

                stageInfos.Add(new DS.StageInfo()
                {
                    stageID = stageNode.Attributes["id"].Value,
                    Chapter = stageNode["Chapter"].InnerText,
                    stageDoc = stageNode["Name"].InnerText
                });
            }
        }
    
        /// <summary>
        /// Load skin and book icon datas
        /// </summary>
        public static void LoadDatas_SkinAndBookIconInfo()
        {
            #region Load book icon infos
            dropBookInfos.Clear();
            string dropBookInfoPath = $"{DM.Config.config.LORFolderPath}\\{DS.PATH.RELATIVE_DIC_LOR_MODE_RESOURCES_LOCALIZE}\\kr\\etc\\KR_Dropbook.txt";
            Directory.GetFiles($"{DM.Config.config.LORFolderPath}\\{DS.PATH.RELATIVE_DIC_LOR_MODE_RESOURCES_STATIC_INFO}\\DropBook").ToList().ForEach((string dropBookPath) =>
            {
                XmlNodeList bookUseNodeList = Tools.XmlFile.SelectNodeLists(dropBookPath, "//BookUse");
                foreach (XmlNode bookUseNode in bookUseNodeList)
                {
                    if (bookUseNode.Attributes["ID"] == null || bookUseNode["TextId"] == null || bookUseNode["BookIcon"] == null || bookUseNode["Chapter"] == null)
                        continue;
                    if (string.IsNullOrEmpty(bookUseNode["BookIcon"].InnerText))
                        continue;

                    XmlNode dropBookInfoNode = Tools.XmlFile.SelectSingleNode(dropBookInfoPath, $"//text[@id='{bookUseNode["TextId"].InnerText}']");
                    if (dropBookInfoNode == null)
                        continue;

                    dropBookInfos.Add(new DS.DropBookInfo()
                    {
                        iconName = bookUseNode["BookIcon"].InnerText,
                        iconDesc = dropBookInfoNode.InnerText,
                        chapter = bookUseNode["Chapter"].InnerText,
                        bookID = bookUseNode.Attributes["ID"].Value
                    });
                }
            });
            #endregion
            #region Load skin icon infos
            bookSkinInfos.Clear();
            gameCriticalPageInfos.Clear();
            Directory.GetFiles($"{DM.Config.config.LORFolderPath}\\{DS.PATH.RELATIVE_DIC_LOR_MODE_RESOURCES_STATIC_INFO}\\EquipPage").ToList().ForEach((string eqPath) =>
            {
                XmlNodeList bookNodeList = Tools.XmlFile.SelectNodeLists(eqPath, "//Book");

                foreach (XmlNode bookNode in bookNodeList)
                {
                    #region Add skin infos
                    if (bookNode["CharacterSkin"] == null)
                        continue;
                    if (string.IsNullOrEmpty(bookNode["CharacterSkin"].InnerText))
                        continue;

                    bookSkinInfos.Add(new DS.BookSkinInfo()
                    {
                        skinDesc = (bookNode["Name"] == null) ? "" : bookNode["Name"].InnerText,
                        skinName = bookNode["CharacterSkin"].InnerText,
                        chapter = (bookNode["Chapter"] == null) ? "" : bookNode["Chapter"].InnerText
                    });
                    #endregion
                    #region Add equiment load infos
                    DS.CriticalPageInfo criticalPageInfo = new DS.CriticalPageInfo();
                    criticalPageInfo.bookID = (bookNode.Attributes["ID"] == null) ? "" : bookNode.Attributes["ID"].Value;

                    criticalPageInfo.name = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Name");
                    criticalPageInfo.rangeType = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "RangeType", "Nomal");
                    criticalPageInfo.rarity = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Rarity");

                    criticalPageInfo.chapter = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Chapter");
                    criticalPageInfo.episode = Tools.XmlFile.GetXmlNodeSafe.ToString(bookNode, "Episode");

                    gameCriticalPageInfos.Add(criticalPageInfo);
                    #endregion
                }
            }); 
            #endregion

        }
    
        /// <summary>
        /// Load passive info datas
        /// </summary>
        public static void LoadDatas_PassiveInfo()
        {
            passiveInfos.Clear();
            passiveList.Clear();

            Directory.GetFiles($"{DM.Config.config.LORFolderPath}\\{DS.PATH.RELATIVE_DIC_LOR_MODE_RESOURCES_LOCALIZE}\\kr\\PassiveDesc").ToList().ForEach((string pvPath) =>
            {
                XmlNodeList passiveDescNodeList = Tools.XmlFile.SelectNodeLists(pvPath, "//PassiveDesc");

                List<DS.PassiveInfo> passives = new List<DS.PassiveInfo>();
                foreach (XmlNode passiveDescNode in passiveDescNodeList)
                {
                    if (passiveDescNode.Attributes["ID"] == null
                        || passiveDescNode["Name"] == null
                        || passiveDescNode["Desc"] == null)
                            continue;

                    passives.Add(new DS.PassiveInfo()
                    {
                        passiveID = passiveDescNode.Attributes["ID"].Value,
                        passiveName = passiveDescNode["Name"].InnerText,
                        passiveDes = passiveDescNode["Desc"].InnerText
                    });
                    passiveList.Add($"{passiveDescNode["Name"].InnerText}:{passiveDescNode["Desc"].InnerText}:{passiveDescNode.Attributes["ID"].Value}");
                }
                string PATH_TO_USE = pvPath.Split('\\').Last().Split('.')[0];
                if(passives.Count > 0) passiveInfos[PATH_TO_USE] = passives;
            });
        }
    
        /// <summary>
        /// Get correct description
        /// </summary>
        public class GetDescription
        {
            /// <summary>
            /// Get episode description for given ID
            /// </summary>
            /// <param name="episodeID">Episode ID to use</param>
            /// <param name="chapterToUse">Chapter If not found</param>
            /// <returns>DES</returns>
            public static string GetEpisodeDescription(string episodeID, string chapterToUse)
            {
                if (!string.IsNullOrEmpty(episodeID))
                {
                    DS.StageInfo foundStageInfo = DM.StaticInfos.stageInfos.Find((DS.StageInfo stageInfo) =>
                    {
                        return stageInfo.stageID == episodeID;
                    });
                    if (foundStageInfo != null)
                        return $"{DS.GameInfo.chapter_Dic[foundStageInfo.Chapter]} / {foundStageInfo.stageDoc}:{foundStageInfo.stageID}";
                    else
                        return $"{DS.GameInfo.chapter_Dic[chapterToUse]} / 커스텀 스테이지:{episodeID}";
                }
                else
                    return "";
            }
        
            /// <summary>
            /// Get skin description for given Name
            /// </summary>
            /// <param name="skinName">Skin name to use</param>
            /// <returns>DES</returns>
            public static string GetSkinDescription(string skinName)
            {
                if (!string.IsNullOrEmpty(skinName))
                {
                    DS.BookSkinInfo foundSkinInfo = DM.StaticInfos.bookSkinInfos.Find((DS.BookSkinInfo skinInfo) =>
                    {
                        return skinInfo.skinName == skinName;
                    });
                    if (foundSkinInfo != null)
                        return $"{foundSkinInfo.skinDesc}:{foundSkinInfo.skinName}";
                    else
                        return $"커스텀 스킨:{skinName}";
                }
                else
                    return "";
            }
        
            /// <summary>
            /// Get icon description for given name
            /// </summary>
            /// <param name="iconName">Icon name to use</param>
            /// <returns>DES</returns>
            public static string GetIconDescription(string iconName)
            {
                if (!string.IsNullOrEmpty(iconName))
                {
                    DS.DropBookInfo foundDropBookInfo = DM.StaticInfos.dropBookInfos.Find((DS.DropBookInfo dropInfo) =>
                    {
                        return dropInfo.iconName == iconName;
                    });
                    if (foundDropBookInfo != null)
                        return $"{foundDropBookInfo.iconDesc}:{foundDropBookInfo.iconName}";
                    else
                        return $"커스텀 아이콘:{iconName}";
                }
                else
                    return "";
            }
        
            /// <summary>
            /// Get passive description
            /// </summary>
            /// <param name="passiveID">Passive ID to use</param>
            /// <returns>DES</returns>
            public static string GetPassiveDescription(string passiveID)
            {
                string foundPassiveDesc = DM.StaticInfos.passiveList.Find((string passiveDesc) =>
                {
                    return passiveDesc.Split(':').Last() == passiveID;
                });
                if (!string.IsNullOrEmpty(foundPassiveDesc))
                    return foundPassiveDesc;
                else
                    return $"커스텀:커스텀 패시브:{passiveID}";
            }
        }
    }
}
