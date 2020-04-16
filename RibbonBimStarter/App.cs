#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-NonСommercial-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных 
в некоммерческих целях, при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2020, все права защищены.
This code is listed under the Creative Commons Attribution-NonСommercial-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2020, all rigths reserved.*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Windows.Media.Imaging;

namespace RibbonBimStarter
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class App : IExternalApplication
    {
        public static string assemblyPath = typeof(App).Assembly.Location;
        public static string assemblyFolder = Path.GetDirectoryName(assemblyPath);
        public static string ribbonPath = Path.Combine(assemblyFolder, "RibbonBimStarterData");

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Weandrevit";
            try { application.CreateRibbonTab(tabName); } catch { }

            try
            {
                CreateAboutRibbon(application, tabName);
                CreateViewRibbon(application, tabName);
                CreateRebarRibbon(application, tabName);
                CreateTableRibbon(application, tabName);
                CreateModelingRibbon(application, tabName);
                CreateMasterRibbon(application, tabName);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ribbon Sample", ex.ToString());

                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }


        private void CreateAboutRibbon(UIControlledApplication uiApp, string tabName)
        {
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "About");
            panel.AddItem(CreateButtonData("AboutWeandrevit", "CommandAbout2"));
            panel.AddItem(CreateButtonData("AskBimQuestion", "CommandAskBimQuestion"));
        }




        private void CreateRebarRibbon(UIControlledApplication uiApp, string tabName)
        {
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
            
        }



        private void CreateTableRibbon(UIControlledApplication uiApp, string tabName)
        {
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
        }

        private void CreateViewRibbon(UIControlledApplication uiApp, string tabName)
        {
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "Виды и листы");

            PushButtonData pbdPrint = CreateButtonData("BatchPrintYay", "CommandBatchPrint");
            panel.AddItem(pbdPrint);

            PushButtonData pbdColorize = CreateButtonData("BatchFilterCreation", "CommandViewColoring");
            panel.AddItem(pbdColorize);


            PushButtonData pbdWallHatch = CreateButtonData("BatchFilterCreation", "CommandWallHatch");
            PushButtonData pbdOverrides = CreateButtonData("OverridesClear", "Command");

            SplitButtonData sbdFilters = new SplitButtonData("SplitButtonViewFilters", "Фильтры графики");

            
            

            IList<RibbonItem> filterItems = panel.AddStackedItems(pbdOverrides, pbdWallHatch, sbdFilters);

            SplitButton sbFilters = filterItems[2] as SplitButton;
            PushButtonData pbdCreateFilters = CreateButtonData("BatchFilterCreation", "CommandCreate");
            sbFilters.AddPushButton(pbdCreateFilters);
            PushButtonData pbdDeleteFilters = CreateButtonData("BatchFilterCreation", "CommandBatchDelete");
            sbFilters.AddPushButton(pbdDeleteFilters);

            PushButtonData pbdOpenSheets = CreateButtonData("OpenSheets", "Command");
            PushButtonData pbdViewNumbers = CreateButtonData("ViewportNumbers", "CommandViewportNumbers");
            PushButtonData pbdTemplates = CreateButtonData("ViewTemplateUtils", "CommandCopyTemplate");
            panel.AddStackedItems(pbdOpenSheets, pbdViewNumbers, pbdTemplates);
        }

        private void CreateModelingRibbon(UIControlledApplication uiApp, string tabName)
        {
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

            PushButtonData pbdAutoUnjoin = CreateButtonData("AutoJoin", "CommandBatchUnjoin");
            pbdAutoUnjoin.Text = "Авто\nразделение";
            splitJoin.AddPushButton(pbdAutoUnjoin);

            PushButtonData pbdAutoCut = CreateButtonData("AutoJoin", "CommandAutoCut");
            pbdAutoCut.Text = "Авто\nвырезание";
            splitJoin.AddPushButton(pbdAutoCut);
            splitJoin.AddPushButton(CreateButtonData("AutoJoin", "CommandCreateCope"));

            PushButtonData pbdParameterWriter = CreateButtonData("ParameterWriter", "Command");
            PushButtonData pbdHost = CreateButtonData("PropertiesCopy", "CommandSelectHost");
            PushButtonData pbdParts = CreateButtonData("WriteParametersFormElemsToParts", "CommandWriteParam");
            panel.AddStackedItems(pbdParameterWriter, pbdHost, pbdParts);


        }


        private void CreateMasterRibbon(UIControlledApplication uiApp, string tabName)
        {
            RibbonPanel panel = uiApp.CreateRibbonPanel(tabName, "BIM-мастер");


            SplitButtonData sbdAddParams = new SplitButtonData("FamilyParametersSplitButton", "Добавить параметры");
            PushButtonData pbdClearGuids = CreateButtonData("ClearUnusedGUIDs", "CommandClear");
            PushButtonData pbdFixSlowFile = CreateButtonData("FixSlowFile", "Command");

            IList<RibbonItem> stacked1 = panel.AddStackedItems(sbdAddParams, pbdClearGuids, pbdFixSlowFile);

            SplitButton splitFamParam = stacked1[0] as SplitButton;
            PushButtonData pbdAddParamToFamily = CreateButtonData("ClearUnusedGUIDs", "CommandAddParameters");
            splitFamParam.AddPushButton(pbdAddParamToFamily);

            PushButtonData pbdAddParamToFamilyByAnalog = CreateButtonData("ClearUnusedGUIDs", "CommandAddParamsByAnalog");
            splitFamParam.AddPushButton(pbdAddParamToFamilyByAnalog);

            PushButtonData pbdRebarBds = CreateButtonData("RebarBDS", "Command");
            PushButtonData pbdWorksets = CreateButtonData("RevitWorksets", "Command");
            PushButtonData pbdFamiliesLibrary = CreateButtonData("TestDockable3", "CommandShowDockableWindow");
            panel.AddStackedItems(pbdRebarBds, pbdWorksets, pbdFamiliesLibrary);
        }


        public PushButtonData CreateButtonData(string assemblyName, string className)
        {
            string dllPath = Path.Combine(ribbonPath, assemblyName, assemblyName + ".dll");
            string fullClassname = assemblyName + "." + className;
            string dataPath = Path.Combine(ribbonPath, assemblyName, "data");
            string largeIcon = Path.Combine(dataPath, className + "_large.png");
            string smallIcon = Path.Combine(dataPath, className + "_small.png");
            string textPath = Path.Combine(dataPath, className + ".txt");
            string[] text = File.ReadAllLines(textPath);
            string title = text[0];
            string tooltip = text[1];
            string url = text[2];

            PushButtonData data = new PushButtonData(fullClassname, title, dllPath, fullClassname);

            //PushButton pbutton = null;
            //if(parentItem is RibbonPanel)
            //{
            //    RibbonPanel panel = parentItem as RibbonPanel;
            //    pbutton = panel.AddItem(data) as PushButton;
            //}
            //if(parentItem is SplitButton)
            //{
            //    SplitButton splitBtn = parentItem as SplitButton;
            //    pbutton = splitBtn.AddPushButton(data);
            //}

            data.LargeImage = new BitmapImage(new Uri(largeIcon, UriKind.Absolute));
            data.Image = new BitmapImage(new Uri(smallIcon, UriKind.Absolute));

            data.ToolTip = text[1];

            ContextualHelp chelp = new ContextualHelp(ContextualHelpType.Url, url);
            data.SetContextualHelp(chelp);

            return data;
        }



    }
}
