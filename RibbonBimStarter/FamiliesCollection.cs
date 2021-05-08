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
#region usings
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace RibbonBimStarter
{
    [Serializable]
    public class FamilyCard
    {
        public string name;
        public string guid;
        public string groupid;
        public string grname;
        public string description;
        public string dateadd;
        public string revitversion;
        public string category;
        public string catshortname;
        public string host;
        public string hostshortname;
    }

    public static class FamiliesCollection
    {
        public static Dictionary<string, ObservableCollection<FamilyFileInfo>> convertJsonToFamilies(string json)
        {
            Dictionary<string, ObservableCollection<FamilyFileInfo>> dictionary =
                new Dictionary<string, ObservableCollection<FamilyFileInfo>>();

            List<FamilyCard> infos = new List<FamilyCard>();

            try
            {
                infos = JsonConvert.DeserializeObject<List<FamilyCard>>(json);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Не удалось десериализовать: " + json);
            }
            foreach (FamilyCard fc in infos)
            {
                FamilyFileInfo familyFileInfo = new FamilyFileInfo();
                familyFileInfo.Guid = fc.guid;
                familyFileInfo.Title = fc.name;
                familyFileInfo.ImagePath = "http://bim-starter3.local/img/families/" + fc.guid + "_small.jpg";
                familyFileInfo.Description = fc.description;
                familyFileInfo.HostType = "Тип основы: " + fc.host;
                familyFileInfo.HostShortName = fc.hostshortname;
                familyFileInfo.CategoryName = fc.category;
                familyFileInfo.CategoryTitleAndName = "Категория: " + fc.category;
                familyFileInfo.CategoryShortName = fc.catshortname;
                familyFileInfo.DateAdd = "Дата создания: " + fc.dateadd;
                familyFileInfo.RevitVersion = "Версия Revit: " + fc.revitversion;

                familyFileInfo.FamilyName = fc.groupid + "_" + fc.name + " (" + fc.catshortname + "_" + fc.hostshortname + ")";

                //familyFileInfo.FamilyName = 

                string folderTitle = fc.groupid + "_" + fc.grname;
                familyFileInfo.FolderTitle = folderTitle;
                if (dictionary.ContainsKey(folderTitle))
                {
                    dictionary[folderTitle].Add(familyFileInfo);
                }
                else
                {
                    dictionary.Add(folderTitle, new ObservableCollection<FamilyFileInfo>
                    {
                        familyFileInfo
                    });
                }
            }
            return dictionary;
        }
    }
}
