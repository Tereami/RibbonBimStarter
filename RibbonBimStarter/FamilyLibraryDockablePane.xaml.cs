using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RibbonBimStarter
{
    /// <summary>
    /// Interaction logic for FamilyLibraryDockablePane.xaml
    /// </summary>
    public partial class FamilyLibraryDockablePane : System.Windows.Controls.Page, IDockablePaneProvider
    {

        private ExternalEvent _exEvent;
        private EventLoadFamily _loadfamEvent;

        public Dictionary<string, ObservableCollection<FamilyFileInfo>> familyCollection;

        public FamilyLibraryDockablePane(ExternalEvent exEvent, EventLoadFamily loadFamEvent)
        {
            InitializeComponent();

            ToolTipService.ShowDurationProperty
                .OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(int.MaxValue));

            _exEvent = exEvent;
            _loadfamEvent = loadFamEvent;
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Left;
            data.InitialState.TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
        }

        private void ListView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ListView list = sender as ListView;
            FamilyFileInfo famInfo = (FamilyFileInfo)list.SelectedItem;
            EventLoadFamily.familyPath = famInfo.FilePath;
            _exEvent.Raise();
        }

        private void AddFamilies_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            //if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            //string selectedPath = folderDialog.SelectedPath;

            string selectedPath = @"C:\Users\Alexander\YandexDisk\WeAndRevit\BIMLIBRARY\Окна двери 2017"; 
            familyCollection = FilesScaner.GetInfo(selectedPath);
            this.tabcontrol.ItemsSource = familyCollection;

        }
    }
}
