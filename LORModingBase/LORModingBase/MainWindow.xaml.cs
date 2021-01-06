﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using LORModingBase.CustomExtensions;

namespace LORModingBase
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow = null;

        #region Init controls
        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            MainWindowButtonClickEvents(BtnCriticalPage, null);

            try
            {
                LoadAllRelatedDatasAfterChangePath();

                Tools.WindowControls.LocalizeWindowControls(this, DM.LANGUAGE_FILE_NAME.MAIN_WINDOW);
                InitSplCriticalPage();
                InitSplCards();
                InitLbxTextEditor();
            }
            catch(Exception ex)
            {
                Tools.MessageBoxTools.ShowErrorMessageBox(ex, DM.LocalizeCore.GetLanguageData(DM.LANGUAGE_FILE_NAME.MAIN_WINDOW, $"MainWindow_Error"));
            }
        }

        /// <summary>
        /// Load all related datas after path is changed
        /// </summary>
        private void LoadAllRelatedDatasAfterChangePath()
        {
            DM.Config.LoadData();
            DM.LocalizeCore.LoadAllDatas();

            #region Check LOR folder exists. If exists, init LblLORPath
            if (!Directory.Exists(DM.Config.config.LORFolderPath) || !Directory.Exists($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data") 
                || string.IsNullOrEmpty(DM.Config.config.LORFolderPath))
            {
                DM.Config.config.LORFolderPath = "";
                DM.Config.SaveData();
                throw new Exception(DM.LocalizeCore.GetLanguageData(DM.LANGUAGE_FILE_NAME.MAIN_WINDOW, $"LoadAllRelatedDatasAfterChangePath_Error_1"));
            }
            #endregion
            #region Check LOR mode resources
            if(!Directory.Exists(DM.Config.GAME_RESOURCE_PATHS.RESOURCE_ROOT_STATIC))
                throw new Exception(DM.LocalizeCore.GetLanguageData(DM.LANGUAGE_FILE_NAME.MAIN_WINDOW, $"LoadAllRelatedDatasAfterChangePath_Error_2"));
            #endregion

            if (!Directory.Exists(DS.PROGRAM_PATHS.DIC_EXPORT_DATAS))
                Directory.CreateDirectory(DS.PROGRAM_PATHS.DIC_EXPORT_DATAS);

            if (File.Exists(DS.PROGRAM_PATHS.VERSION))
                this.Title = $"LOR Moding Base {File.ReadAllText(DS.PROGRAM_PATHS.VERSION)}";

            DM.GameInfos.LoadAllDatas();

            DM.EditGameData_BookInfos.InitDatas();
            DM.EditGameData_CardInfos.InitDatas();
        }
        #endregion

        #region Main window events
        /// <summary>
        /// Hide all main window menu grids
        /// </summary>
        private void HideAllGrid()
        {
            GrdCriticalPage.Visibility = Visibility.Collapsed;
            BtnCriticalPage.Foreground = Tools.ColorTools.GetSolidColorBrushByHexStr("#FFFFD9A3");

            GrdCards.Visibility = Visibility.Collapsed;
            BtnCards.Foreground = Tools.ColorTools.GetSolidColorBrushByHexStr("#FFFFD9A3");
        }

        /// <summary>
        /// Main window button events
        /// </summary>
        private void MainWindowButtonClickEvents(object sender, RoutedEventArgs e)
        {
            Button clickButton = sender as Button;
            try
            {
                switch (clickButton.Name)
                {
                    case "BtnCriticalPage":
                        HideAllGrid();
                        GrdCriticalPage.Visibility = Visibility.Visible;
                        BtnCriticalPage.Foreground = Tools.ColorTools.GetSolidColorBrushByHexStr("#FFFDC61B");
                        break;
                    case "BtnCards":
                        HideAllGrid();
                        GrdCards.Visibility = Visibility.Visible;
                        BtnCards.Foreground = Tools.ColorTools.GetSolidColorBrushByHexStr("#FFFDC61B");
                        break;

                    case "BtnSetWorkingSpace":
                        new SubWindows.Global_ListSeleteWithEditWindow(null, null, null, null,
                            SubWindows.Global_ListSeleteWithEditWindow_PRESET.WORKING_SPACE).ShowDialog();

                        InitSplCriticalPage();
                        InitSplCards();
                        InitLbxTextEditor();
                        break;

                    case "BtnStartGame":
                        Tools.ProcessTools.StartProcess($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina.exe");
                        if (DM.Config.config.isLogPlusMod)
                        {
                            if (File.Exists($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data\\BaseMods\\debugLogMessage.txt"))
                                File.WriteAllText($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data\\BaseMods\\debugLogMessage.txt", "");
                            if (File.Exists($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data\\BaseMods\\debugErrorMessage.txt"))
                                File.WriteAllText($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data\\BaseMods\\debugErrorMessage.txt", "");

                            new SubWindows.ExtraLogWindow().Show();
                        }
                        break;

                    case "BtnOpenBaseFolder":
                        if (Directory.Exists($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data\\BaseMods"))
                            Tools.ProcessTools.OpenExplorer($"{DM.Config.config.LORFolderPath}\\LibraryOfRuina_Data\\BaseMods");
                        break;
                    case "BtnOpenExportFolder":
                        if (Directory.Exists($"{Tools.ProcessTools.GetWorkingDirectory()}\\exportedModes"))
                            Tools.ProcessTools.OpenExplorer($"{Tools.ProcessTools.GetWorkingDirectory()}\\exportedModes");
                        break;

                    case "BtnExtURL":
                        new SubWindows.Global_ListSeleteWindow(null, SubWindows.Global_ListSeleteWindow_PRESET.EXT_URL).ShowDialog();
                        break;
                    case "BtnTools":
                        new SubWindows.ExtraToolsWindow().ShowDialog();
                        break;
                    case "BtnConfig":
                        new SubWindows.OptionWindow(LoadAllRelatedDatasAfterChangePath).ShowDialog();
                        break;

                }
            }
            catch (Exception ex)
            {
                Tools.MessageBoxTools.ShowErrorMessageBox(ex,
                    DM.LocalizeCore.GetLanguageData(DM.LANGUAGE_FILE_NAME.MAIN_WINDOW, $"MainWindowButtonClickEvents_Error_6"));
            }
        } 
        #endregion


        #region EDIT MENU - Critical Page Infos
        private void InitSplCriticalPage()
        {
            SplCriticalPage.Children.Clear();
            DM.EditGameData_BookInfos.StaticEquipPage.rootDataNode.ActionXmlDataNodesByPath("Book", (DM.XmlDataNode xmlDataNode) =>
            {
                SplCriticalPage.Children.Add(new UC.EditCriticalPage(xmlDataNode, InitSplCriticalPage));
            });
        }

        private void CriticalPageGridButtonClickEvents(object sender, RoutedEventArgs e)
        {
            Button clickButton = sender as Button;
            try
            {
                if (string.IsNullOrEmpty(DM.Config.CurrentWorkingDirectory))
                {
                    MainWindowButtonClickEvents(BtnSetWorkingSpace, null);
                    return;
                }

                switch (clickButton.Name)
                {
                    case "BtnAddCriticalBook":
                        DM.EditGameData_BookInfos.StaticEquipPage.rootDataNode.subNodes.Add(
                            DM.EditGameData_BookInfos.MakeNewStaticEquipPageBase());
                        InitSplCriticalPage();
                        break;
                    case "BtnLoadCriticalBook":
                        new SubWindows.Global_InputInfoWithSearchWindow((string selectedItem) =>
                        {
                            List<DM.XmlDataNode> foundEqNodes = DM.GameInfos.staticInfos["EquipPage"].rootDataNode.GetXmlDataNodesByPathWithXmlInfo("Book",
                                attributeToCheck: new Dictionary<string, string>() { { "ID", selectedItem } });
                            if(foundEqNodes.Count > 0)
                            {
                                DM.EditGameData_BookInfos.StaticEquipPage.rootDataNode.subNodes.Add(foundEqNodes[0].Copy());

                                List<DM.XmlDataNode> foundLocalizeBooks = DM.GameInfos.localizeInfos["Books"].rootDataNode.GetXmlDataNodesByPathWithXmlInfo("bookDescList/BookDesc",
                                    attributeToCheck: new Dictionary<string, string>() { { "BookID", selectedItem } });
                                if(foundLocalizeBooks.Count > 0)
                                {
                                    if (!DM.EditGameData_BookInfos.LocalizedBooks.rootDataNode.CheckIfGivenPathWithXmlInfoExists("bookDescList/BookDesc",
                                               attributeToCheck: new Dictionary<string, string>() { { "BookID", selectedItem } }))
                                    {
                                        DM.EditGameData_BookInfos.LocalizedBooks.rootDataNode.GetXmlDataNodesByPath("bookDescList").ActionOneItemSafe((DM.XmlDataNode bookDescList) =>
                                        {
                                            bookDescList.subNodes.Add(foundLocalizeBooks[0].Copy());
                                        });
                                    }
                                }

                                DM.GameInfos.staticInfos["DropBook"].rootDataNode.GetXmlDataNodesByPath("BookUse").ForEach((DM.XmlDataNode bookUseNode) =>
                                {
                                    List<string> dropItems = new List<string>();
                                    bookUseNode.GetXmlDataNodesByPath("DropItem").ForEach((DM.XmlDataNode dropItemNode) =>
                                    {
                                        dropItems.Add(dropItemNode.innerText);
                                    });
                                    if(dropItems.Contains(selectedItem))
                                    {
                                        if(!DM.EditGameData_BookInfos.StaticDropBook.rootDataNode.CheckIfGivenPathWithXmlInfoExists("BookUse", 
                                            attributeToCheck:new Dictionary<string, string>() { { "ID", bookUseNode.GetAttributesSafe("ID") } }))
                                            DM.EditGameData_BookInfos.StaticDropBook.rootDataNode.subNodes.Add(bookUseNode.Copy());
                                    }
                                });

                                InitSplCriticalPage();
                            }
                        }, SubWindows.InputInfoWithSearchWindow_PRESET.CRITICAL_BOOKS).ShowDialog();
                        break;
                }
            }
            catch (Exception ex)
            {
                Tools.MessageBoxTools.ShowErrorMessageBox(ex,
                    DM.LocalizeCore.GetLanguageData(DM.LANGUAGE_FILE_NAME.MAIN_WINDOW, $"CriticalPageGridButtonClickEvents_Error_1"));
            }
        }
        #endregion   
        #region EDIT MENU - Cards Page
        private void InitSplCards()
        {
            SplCards.Children.Clear();
            DM.EditGameData_CardInfos.StaticCard.rootDataNode.ActionXmlDataNodesByPath("Card", (DM.XmlDataNode xmlDataNode) =>
            {
                SplCards.Children.Add(new UC.EditCard(xmlDataNode, InitSplCards));
            });
        }

        private void BattleCardGridButtonClickEvents(object sender, RoutedEventArgs e)
        {
            Button clickButton = sender as Button;
            try
            {
                if (string.IsNullOrEmpty(DM.Config.CurrentWorkingDirectory))
                {
                    MainWindowButtonClickEvents(BtnSetWorkingSpace, null);
                    return;
                }

                switch (clickButton.Name)
                {
                    case "BtnAddCard":
                        DM.EditGameData_CardInfos.StaticCard.rootDataNode.subNodes.Add(
                            DM.EditGameData_CardInfos.MakeNewCardBase());
                        InitSplCards();
                        break;
                    case "BtnLoadCard":
                        new SubWindows.Global_InputInfoWithSearchWindow((string selectedItem) =>
                        {
                            List<DM.XmlDataNode> foundCardNodes = DM.GameInfos.staticInfos["Card"].rootDataNode.GetXmlDataNodesByPathWithXmlInfo("Card",
                                attributeToCheck: new Dictionary<string, string>() { { "ID", selectedItem } });
                            if (foundCardNodes.Count > 0)
                            {
                                DM.EditGameData_CardInfos.StaticCard.rootDataNode.subNodes.Add(foundCardNodes[0].Copy());

                                List<DM.XmlDataNode> foundLocalizeCards = DM.GameInfos.localizeInfos["BattlesCards"].rootDataNode.GetXmlDataNodesByPathWithXmlInfo("cardDescList/BattleCardDesc",
                                    attributeToCheck: new Dictionary<string, string>() { { "ID", selectedItem } });
                                if (foundLocalizeCards.Count > 0)
                                {
                                    if (!DM.EditGameData_CardInfos.LocalizedBattleCards.rootDataNode.CheckIfGivenPathWithXmlInfoExists("cardDescList/BattleCardDesc",
                                               attributeToCheck: new Dictionary<string, string>() { { "ID", selectedItem } }))
                                    {
                                        DM.EditGameData_CardInfos.LocalizedBattleCards.rootDataNode.GetXmlDataNodesByPath("cardDescList").ActionOneItemSafe((DM.XmlDataNode cardDescList) =>
                                        {
                                            cardDescList.subNodes.Add(foundLocalizeCards[0].Copy());
                                        });
                                    }
                                }

                                DM.GameInfos.staticInfos["CardDropTable"].rootDataNode.GetXmlDataNodesByPath("DropTable").ForEach((DM.XmlDataNode dropTableNode) =>
                                {
                                    List<string> deopCards = new List<string>();
                                    dropTableNode.GetXmlDataNodesByPath("Card").ForEach((DM.XmlDataNode dropCardNode) =>
                                    {
                                        deopCards.Add(dropCardNode.innerText);
                                    });
                                    if (deopCards.Contains(selectedItem))
                                    {
                                        if (!DM.EditGameData_CardInfos.StaticCardDropTable.rootDataNode.CheckIfGivenPathWithXmlInfoExists("DropTable",
                                            attributeToCheck: new Dictionary<string, string>() { { "ID", dropTableNode.GetAttributesSafe("ID") } }))
                                            DM.EditGameData_CardInfos.StaticCardDropTable.rootDataNode.subNodes.Add(dropTableNode.Copy());
                                    }
                                });

                                InitSplCards();
                            }
                        }, SubWindows.InputInfoWithSearchWindow_PRESET.CARDS).ShowDialog();
                        break;
                }
            }
            catch (Exception ex)
            {
                Tools.MessageBoxTools.ShowErrorMessageBox(ex,
                    DM.LocalizeCore.GetLanguageData(DM.LANGUAGE_FILE_NAME.MAIN_WINDOW, $"BattleCardGridButtonClickEvents_Error_1"));
            }
        }
        #endregion


        #region Text editor functions
        List<string> EDITOR_SELECTION_MENU = new List<string>()
        {
            "EquipPage.txt",
            "DropBook.txt",
            "Books.txt",
            "CardInfo.txt",
            "CardDropTable.txt",
            "BattlesCards.txt"
        };
        private void InitLbxTextEditor()
        {
            LbxTextEditor.Items.Clear();
            EDITOR_SELECTION_MENU.ForEach((string menu) =>
            {
                LbxTextEditor.Items.Add(menu);
            });
            if (LbxTextEditor.Items.Count > 0)
                LbxTextEditor.SelectedIndex = 0;
        }

        /// <summary>
        /// Update debugging info
        /// </summary>
        public void UpdateDebugInfo()
        {
            try
            {
                if(!string.IsNullOrEmpty(DM.Config.CurrentWorkingDirectory))
                {
                    DM.EditGameData_BookInfos.StaticEquipPage.SaveNodeData(DM.Config.GetStaticPathToSave(DM.EditGameData_BookInfos.StaticEquipPage, DM.Config.CurrentWorkingDirectory));
                    DM.EditGameData_BookInfos.StaticDropBook.SaveNodeData(DM.Config.GetStaticPathToSave(DM.EditGameData_BookInfos.StaticDropBook, DM.Config.CurrentWorkingDirectory));
                    DM.EditGameData_BookInfos.LocalizedBooks.SaveNodeData(DM.Config.GetLocalizePathToSave(DM.EditGameData_BookInfos.LocalizedBooks, DM.Config.CurrentWorkingDirectory));

                    DM.EditGameData_CardInfos.StaticCard.SaveNodeData(DM.Config.GetStaticPathToSave(DM.EditGameData_CardInfos.StaticCard, DM.Config.CurrentWorkingDirectory));
                    DM.EditGameData_CardInfos.StaticCardDropTable.SaveNodeData(DM.Config.GetStaticPathToSave(DM.EditGameData_CardInfos.StaticCardDropTable, DM.Config.CurrentWorkingDirectory));
                    DM.EditGameData_CardInfos.LocalizedBattleCards.SaveNodeData(DM.Config.GetLocalizePathToSave(DM.EditGameData_CardInfos.LocalizedBattleCards, DM.Config.CurrentWorkingDirectory));

                    string debugFileName = "";
                    switch (LbxTextEditor.SelectedIndex)
                    {
                        case 0:
                            debugFileName = DM.Config.GetStaticPathToSave(DM.EditGameData_BookInfos.StaticEquipPage, DM.Config.CurrentWorkingDirectory);
                            break;
                        case 1:
                            debugFileName = DM.Config.GetStaticPathToSave(DM.EditGameData_BookInfos.StaticDropBook, DM.Config.CurrentWorkingDirectory);
                            break;
                        case 2:
                            debugFileName = DM.Config.GetLocalizePathToSave(DM.EditGameData_BookInfos.LocalizedBooks, DM.Config.CurrentWorkingDirectory);
                            break;

                        case 3:
                            debugFileName = DM.Config.GetStaticPathToSave(DM.EditGameData_CardInfos.StaticCard, DM.Config.CurrentWorkingDirectory);
                            break;
                        case 4:
                            debugFileName = DM.Config.GetStaticPathToSave(DM.EditGameData_CardInfos.StaticCardDropTable, DM.Config.CurrentWorkingDirectory);
                            break;
                        case 5:
                            debugFileName = DM.Config.GetLocalizePathToSave(DM.EditGameData_CardInfos.LocalizedBattleCards, DM.Config.CurrentWorkingDirectory);
                            break;
                    }
                    if (File.Exists(debugFileName))
                        TbxTextEditor.Text = File.ReadAllText(debugFileName);

                    TbxTextEditorLog.Text = $"데이터가 정상적으로 생성됨";
                    TbxTextEditorLog.ToolTip = $"데이터가 정상적으로 생성됨";
                }
            }
            catch(Exception ex)
            {
                TbxTextEditorLog.Text = $"데이터 생성 도중 에러 발생 \n> {ex.Message}";
                TbxTextEditorLog.ToolTip = $"데이터 생성 도중 에러 발생 \n> {ex.Message}";
            }
        }
        #endregion

        private void LbxTextEditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbxTextEditor.SelectedItem != null)
                LblTextEditor.Content = LbxTextEditor.SelectedItem.ToString();
            UpdateDebugInfo();
        }
    }
}
