#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-NonСommercial-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных 
в некоммерческих целях, при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2021, все права защищены.
This code is listed under the Creative Commons Attribution-NonСommercial-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2021, all rigths reserved.*/
#endregion
#region Usings
using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI.Events;
using System.Diagnostics;
#endregion

namespace RibbonBimStarter
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class App : IExternalApplication
    {
        public static string assemblyPath;
        public static string assemblyFolder;
        public static string ribbonPath;

        public static SettingsStorage settings;
        public static WebConnection connect;

        public static Guid paneGuid = new Guid("8d8207a6-c925-4e93-bb14-487912fb24b5");
        private FamilyLibraryDockablePane pane = null;

        public static string revitVersion = "2020";

        public Result OnStartup(UIControlledApplication application)
        {
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new Logger("Ribbon"));
            Debug.WriteLine("Revit start");
            assemblyPath = typeof(App).Assembly.Location;
            assemblyFolder = Path.GetDirectoryName(assemblyPath);
            string appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //ribbonPath = Path.Combine(assemblyFolder, "RibbonBimStarterData");
            ribbonPath = Path.Combine(appdataFolder, @"Autodesk\Revit\Addins\20xx\BimStarter");
            revitVersion = application.ControlledApplication.VersionNumber;
            Debug.WriteLine("Ribbin path: " + ribbonPath);

            settings = SettingsStorage.LoadSettings();
            if(settings == null)
            {
                TaskDialog.Show("Ошибка", "Не удалось запустить Bim-Starter! Не найден файл настроек");
                return Result.Failed;
            }
            connect = new WebConnection(App.settings.Email, App.settings.Password, App.settings.Website);

            string messageFiles = "";
            string[] addinFiles = System.IO.Directory.GetFiles(assemblyFolder, "*.addin");
            List<string> oldWeandrevitAddins = new List<string> 
            {
                "WeandrevitPanel.addin", 
                "BatchPrintYay.addin", 
                "RebarSketch.addin", 
                "RevitPlatesWeight.addin" 
            };
            foreach(string addinFile in addinFiles)
            {
                string fileTitle = System.IO.Path.GetFileName(addinFile);
                if(oldWeandrevitAddins.Contains(fileTitle))
                {
                    messageFiles += fileTitle + " ";
                }
            }
            if (messageFiles != "")
            {
                string msg = "Обнаружены устаревшие плагины Weandrevit. Закройте Revit, перейдите в папку ";
                msg += assemblyFolder;
                msg += " и удалите файлы: " + messageFiles;
                Debug.WriteLine("Depricated files found: " + msg);
                TaskDialog.Show("Установка BIM-STARTER", msg);
            }

            string tabName = "BIM-STARTER";
            try { application.CreateRibbonTab(tabName); } 
            catch { Debug.WriteLine("Unable to create tab name " + tabName); }

            try
            {
                CreateAboutRibbon(application, tabName);
                CreateViewRibbon(application, tabName);
                CreateRebarRibbon(application, tabName);
                CreateTableRibbon(application, tabName);
                CreateModelingRibbon(application, tabName);
                CreateMasterRibbon(application, tabName);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ribbon Sample", ex.Message);
                Debug.WriteLine("Exception: " + ex.Message);
                return Result.Failed;
            }
            RegisterDockablepage(application);

            Debug.WriteLine("Ribbon is created succesfully");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }


        private void CreateAboutRibbon(UIControlledApplication uiApp, string tabName)
        {
            Debug.WriteLine("AboutPanel started...");
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "About");
            panel.AddItem(CreateButtonData("AboutWeandrevit", "CommandAbout2"));
            //panel.AddItem(CreateButtonData("AskBimQuestion", "CommandAskBimQuestion"));
            Debug.WriteLine("AboutPanel is created");
        }




        private void CreateRebarRibbon(UIControlledApplication uiApp, string tabName)
        {
            Debug.WriteLine("RebarPanel started...");
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "Армирование");
            SplitButton areaRebarSplitButton = panel
                .AddItem(new SplitButtonData("AreaRebarSplitButton", "Фоновое"))
                as SplitButton;

            areaRebarSplitButton.AddPushButton(CreateButtonData("RevitAreaReinforcement", "CommandCreateAreaRebar"));
            areaRebarSplitButton.AddPushButton(CreateButtonData("RevitAreaReinforcement", "CommandCreateFloorRebar"));
            areaRebarSplitButton.AddSeparator();
            areaRebarSplitButton.AddPushButton(CreateButtonData("RevitAreaReinforcement", "CommandRestoreRebarArea"));

            panel.AddSeparator();

            panel.AddItem(CreateButtonData("RebarVisibility", "Command"));

            PushButtonData dataAreaMark = CreateButtonData("AreaRebarMark", "CommandManualStart");
            PushButtonData dataHideRebars = CreateButtonData("RebarPresentation", "Command");
            PushButtonData dataExplodeRebars = CreateButtonData("ExplodeRebarSet", "CommandExplode");
            panel.AddStackedItems(dataAreaMark, dataHideRebars, dataExplodeRebars);
            Debug.WriteLine("RebarPanel is created");
        }



        private void CreateTableRibbon(UIControlledApplication uiApp, string tabName)
        {
            Debug.WriteLine("TablePanel started...");
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "Таблицы");

            PushButtonData dataRebarSketch = CreateButtonData("RebarSketch", "CommandCreatePictures3");
            dataRebarSketch.Text = "Вед-ть\nдеталей";
            panel.AddItem(dataRebarSketch);

            PushButtonData dataAutonumber = CreateButtonData("Autonumber", "CommandStart");
            panel.AddItem(dataAutonumber);

            PushButtonData dataCollapseRebarSchedule = CreateButtonData("CollapseRebarSchedule", "Command");
            dataCollapseRebarSchedule.Text = "Подчистить\nВРС";
            panel.AddItem(dataCollapseRebarSchedule);

            PushButtonData pbdRefreshSchedules = CreateButtonData("BatchPrintYay", "CommandRefreshSchedules");
            PushButtonData dataSchedulesTable = CreateButtonData("SchedulesTable", "CommandCreateTable");
            PushButtonData dataRevisions = CreateButtonData("RevisionClouds", "Command");
            panel.AddStackedItems(pbdRefreshSchedules, dataSchedulesTable, dataRevisions);

            panel.AddSlideOut();
            PushButtonData dataScetchConstructor = CreateButtonData("RebarSketch", "CommandFormGenerator");
            panel.AddItem(dataScetchConstructor);
            Debug.WriteLine("TablePanel is created");
        }

        private void CreateViewRibbon(UIControlledApplication uiApp, string tabName)
        {
            Debug.WriteLine("ViewPanel started...");
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "Виды и листы");

            PushButtonData pbdPrint = CreateButtonData("BatchPrintYay", "CommandBatchPrint");
            panel.AddItem(pbdPrint);

            PushButtonData pbdColorize = CreateButtonData("RevitViewFilters", "CommandViewColoring");
            panel.AddItem(pbdColorize);


            PushButtonData pbdWallHatch = CreateButtonData("RevitViewFilters", "CommandWallHatch");
            PushButtonData pbdOverrides = CreateButtonData("RevitGraphicsOverride", "Command");

            SplitButtonData sbdFilters = new SplitButtonData("SplitButtonViewFilters", "Фильтры графики");

            IList<RibbonItem> filterItems = panel.AddStackedItems(pbdOverrides, pbdWallHatch, sbdFilters);

            SplitButton sbFilters = filterItems[2] as SplitButton;
            PushButtonData pbdCreateFilters = CreateButtonData("RevitViewFilters", "CommandCreate");
            sbFilters.AddPushButton(pbdCreateFilters);
            PushButtonData pbdDeleteFilters = CreateButtonData("RevitViewFilters", "CommandBatchDelete");
            sbFilters.AddPushButton(pbdDeleteFilters);

            PushButtonData pbdOpenSheets = CreateButtonData("OpenSheets", "Command");
            PushButtonData pbdViewNumbers = CreateButtonData("ViewportNumbers", "CommandViewportNumbers");
            PushButtonData pbdTemplates = CreateButtonData("ViewTemplateUtils", "CommandCopyTemplate");
            panel.AddStackedItems(pbdOpenSheets, pbdViewNumbers, pbdTemplates);
            Debug.WriteLine("ViewPanel is created");
        }

        private void CreateModelingRibbon(UIControlledApplication uiApp, string tabName)
        {
            Debug.WriteLine("ModelingPanel started...");
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "Моделирование");

            SplitButton splitHolesElev = panel
                .AddItem(new SplitButtonData("HolesElevSplitButton", "Отверстия"))
                as SplitButton;
            PushButtonData pbdElevations = CreateButtonData("RevitElementsElevation", "Command");
            pbdElevations.Text = "Определить\nотметки";
            splitHolesElev.AddPushButton(pbdElevations);

            splitHolesElev.AddSeparator();
            PushButtonData pbdHolesSettings = CreateButtonData("RevitElementsElevation", "CommandConfig");
            splitHolesElev.AddPushButton(pbdHolesSettings);

            PushButtonData pbdPropertiesCopy = CreateButtonData("PropertiesCopy", "CommandPropertiesCopy");
            pbdPropertiesCopy.Text = "Супер-\nкисточка";
            panel.AddItem(pbdPropertiesCopy);

            PushButtonData pbdGroupedAssembly = CreateButtonData("GroupedAssembly", "CommandSuperAssembly");
            pbdGroupedAssembly.Text = "Сборка-\nгруппа";
            panel.AddItem(pbdGroupedAssembly);

            //PushButtonData pbd = CreateButtonData("", "");

            SplitButton splitJoin = panel
                .AddItem(new SplitButtonData("JoingeometrySplitButton", "Геометрия"))
                as SplitButton;

            PushButtonData pbdAutoJoin = CreateButtonData("AutoJoin", "CommandAutoJoin");
            pbdAutoJoin.Text = "Авто\nсоединение";
            splitJoin.AddPushButton(pbdAutoJoin);

            PushButtonData pbdJoinByOrder = CreateButtonData("AutoJoin", "CommandJoinByOrder");
            pbdJoinByOrder.Text = "Задать\nприоритет";
            splitJoin.AddPushButton(pbdJoinByOrder);

            PushButtonData pbdAutoUnjoin = CreateButtonData("AutoJoin", "CommandBatchUnjoin");
            pbdAutoUnjoin.Text = "Авто\nразделение";
            splitJoin.AddPushButton(pbdAutoUnjoin);

            PushButtonData pbdAutoCut = CreateButtonData("AutoJoin", "CommandAutoCut");
            pbdAutoCut.Text = "Авто\nвырезание";
            splitJoin.AddPushButton(pbdAutoCut);
            splitJoin.AddPushButton(CreateButtonData("AutoJoin", "CommandCreateCope"));

            
            PushButtonData pbdHost = CreateButtonData("PropertiesCopy", "CommandSelectHost");
            SplitButtonData sbdPiles = new SplitButtonData("Piles", "Сваи");
            IList<RibbonItem> stacked1 = panel.AddStackedItems(pbdHost, sbdPiles);
            
            SplitButton splitPiles = stacked1[1] as SplitButton;
            splitPiles.AddPushButton(CreateButtonData("PilesCoords", "PilesNumberingCommand"));
            splitPiles.AddPushButton(CreateButtonData("PilesCoords", "PileCutCommand"));
            splitPiles.AddPushButton(CreateButtonData("PilesCoords", "PilesElevationCommand"));
            splitPiles.AddPushButton(CreateButtonData("PilesCoords", "PilesCalculateRangeCommand"));
            splitPiles.AddSeparator();
            splitPiles.AddPushButton(CreateButtonData("PilesCoords", "SettingsCommand"));
            Debug.WriteLine("ModelingPanel is created");
        }


        private void CreateMasterRibbon(UIControlledApplication uiApp, string tabName)
        {
            Debug.WriteLine("MasterPanel started...");
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "BIM-мастер");

            SplitButtonData sbdAddParams = new SplitButtonData("FamilyParametersSplitButton", "Добавить параметры");
            PushButtonData pbdClearGuids = CreateButtonData("ClearUnusedGUIDs", "CommandClear");
            PushButtonData pbdFixSlowFile = CreateButtonData("FixSlowFile", "Command");
            IList<RibbonItem> stacked1 = panel.AddStackedItems(sbdAddParams, pbdClearGuids, pbdFixSlowFile);

            SplitButton splitFamParam = stacked1[0] as SplitButton;
            splitFamParam.AddPushButton(CreateButtonData("ClearUnusedGUIDs", "CommandAddParameters"));
            splitFamParam.AddPushButton(CreateButtonData("ClearUnusedGUIDs", "CommandAddParamsByAnalog"));



            SplitButtonData sbdParametrization = new SplitButtonData("ModelParametrizationSplitButton", "Параметризация");
            PushButtonData pbdWorksets = CreateButtonData("RevitWorksets", "Command");
            SplitButtonData sbdFamilies = new SplitButtonData("FamiliesSplitButton", "Библиотека семейств");

            IList<RibbonItem> stacked2 = panel.AddStackedItems(sbdParametrization, pbdWorksets, sbdFamilies);

            SplitButton splitParametrization = stacked2[0] as SplitButton;
            splitParametrization.AddPushButton(CreateButtonData("ParameterWriter", "Command"));
            splitParametrization.AddPushButton(CreateButtonData("RebarParametrisation", "Command"));
            splitParametrization.AddPushButton(CreateButtonData("WriteParametersFormElemsToParts", "CommandWriteParam"));
            if (revitVersion != "2017" && revitVersion != "2018")
            {
                splitParametrization.AddPushButton(CreateButtonData("RevitPlatesWeight", "Command"));
            }
            splitParametrization.AddPushButton(CreateButtonData("IngradParametrisation", "Cmd"));
            splitParametrization.AddPushButton(CreateButtonData("ColumnsParametrisation", "Command"));


            PushButtonData pbdFamiliesLibrary = new PushButtonData("ShowFamiliesCatalog", "Семейства", assemblyPath, "RibbonBimStarter.CommandShowPane");
            pbdFamiliesLibrary.ToolTip = "Открыть палитру библиотеки семейств";
            string famLibIconsPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "FamilyLibrary_data");
            pbdFamiliesLibrary.LargeImage = new BitmapImage(new Uri(Path.Combine(famLibIconsPath, "FamilyLibrary_large.png")));
            pbdFamiliesLibrary.Image = new BitmapImage(new Uri(Path.Combine(famLibIconsPath, "FamilyLibrary_small.png")));

            PushButtonData pbdCheckFamilies = new PushButtonData("CheckFamilies", "Проверить проект", assemblyPath, "RibbonBimStarter.CommandCheckFamilies");
            pbdCheckFamilies.ToolTip = "Проверить проект на наличие устаревших, дублированных, сторонних и неверно названных семейств";
            pbdCheckFamilies.LargeImage = new BitmapImage(new Uri(Path.Combine(famLibIconsPath, "FamilyCheck_large.png")));
            pbdCheckFamilies.Image = new BitmapImage(new Uri(Path.Combine(famLibIconsPath, "FamilyCheck_small.png")));

            PushButtonData pbdUploadFamily = new PushButtonData("UploadFamily", "Загрузить в базу", assemblyPath, "RibbonBimStarter.CommandUploadFamily");
            pbdUploadFamily.ToolTip = "Загрузить семейство в библиотеку";
            pbdUploadFamily.LargeImage = new BitmapImage(new Uri(Path.Combine(famLibIconsPath, "FamilyUpload_large.png")));
            pbdUploadFamily.Image = new BitmapImage(new Uri(Path.Combine(famLibIconsPath, "FamilyUpload_small.png")));

            //PushButtonData pbdFamilySettings = new PushButtonData("FamilySettings", "Настройки", assemblyPath, "RibbonBimStarter.CommandFamilySettings");
            //pbdFamilySettings.ToolTip = "Настройки библиотеки семейств";

            SplitButton splitFamilies = stacked2[2] as SplitButton;
            splitFamilies.AddPushButton(pbdFamiliesLibrary);
            splitFamilies.AddPushButton(pbdCheckFamilies);
            splitFamilies.AddPushButton(pbdUploadFamily);
            //splitFamilies.AddPushButton(pbdFamilySettings);


            Debug.WriteLine("MasterPanel is created");
        }


        public PushButtonData CreateButtonData(string assemblyName, string className)
        {
            string dllPath = Path.Combine(ribbonPath, assemblyName + ".dll");
            if(!File.Exists(dllPath))
            {
                dllPath = Path.Combine(ribbonPath, assemblyName + "_" + revitVersion + ".dll");
                if(!File.Exists(dllPath))
                {
                    throw new Exception("File not found " + dllPath.Replace(@"\", @" \ "));
                }
            }
            string fullClassname = assemblyName + "." + className;
            string dataPath = Path.Combine(ribbonPath, assemblyName + "_data");
            string largeIcon = Path.Combine(dataPath, className + "_large.png");
            string smallIcon = Path.Combine(dataPath, className + "_small.png");
            string textPath = Path.Combine(dataPath, className + ".txt");
            string[] text = File.ReadAllLines(textPath);
            string title = text[0];
            string tooltip = text[1];
            string url = text[2];

            PushButtonData data = new PushButtonData(fullClassname, title, dllPath, fullClassname);

            data.LargeImage = new BitmapImage(new Uri(largeIcon, UriKind.Absolute));
            data.Image = new BitmapImage(new Uri(smallIcon, UriKind.Absolute));

            data.ToolTip = text[1];

            ContextualHelp chelp = new ContextualHelp(ContextualHelpType.Url, url);
            data.SetContextualHelp(chelp);

            return data;
        }

        private void RegisterDockablepage(UIControlledApplication uiapp)
        {
            Debug.WriteLine("Start register dockable pane");
            EventLoadFamily eventLoad = new EventLoadFamily();
            ExternalEvent exEvent = ExternalEvent.Create(eventLoad);
            DockablePaneProviderData providerData = new DockablePaneProviderData();
            FamilyLibraryDockablePane famPane = new FamilyLibraryDockablePane(exEvent, eventLoad);
            this.pane = famPane;

            providerData.FrameworkElement = famPane;
            DockablePaneState state = new DockablePaneState();
            state.DockPosition = DockPosition.Tabbed;
            state.TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
            providerData.InitialState = state;

            DockablePaneId paneId = new DockablePaneId(paneGuid);

            uiapp.RegisterDockablePane(paneId, "Библиотека семейств", famPane);
            //uiapp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(App_ViewActivated);
            Debug.WriteLine("Dockable pane is registered");
        }

        private void App_ViewActivated(object sender, ViewActivatedEventArgs args)
        {
            //some actions 
        }
    }
}
