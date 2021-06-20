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

using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RibbonBimStarter
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CommandCheckFamilies : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Debug.Listeners.Clear();
            Debug.Listeners.Add(new Logger("CheckFamilies"));

            //найти семейства без гуидов

            //найти семейства с одинаковым гуидом - значит они продублировались

            //для семейств с гуидами проверить, есть ли такие гуиды в базе, показать семейства с гуидами, которых нет в базе

            //по гуидам сравнить номера версий, показать те у которых не совпадают номера версий

            //по информации из бд проверить, соответствуют ли имена семейств тем что прописаны в базе


            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Family> families = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Select(i => i.Symbol.Family)
                .GroupBy(i => i.Id.IntegerValue)
                .Select(grp => grp.FirstOrDefault())
                .OrderBy(i => i.Id.IntegerValue)
                .ToList();

            Dictionary<string, List<Family>> guidsAndFamilies = new Dictionary<string, List<Family>>();
            List<Family> invalidSymbols = new List<Family>();
            List<Family> noGuids = new List<Family>();
            

            foreach(Family fam in families)
            {
                IEnumerable<ElementId> symbIds = fam.GetFamilySymbolIds();
                if(symbIds.Count() == 0)
                {
                    invalidSymbols.Add(fam);
                    continue;
                }
                FamilySymbol symb = doc.GetElement(symbIds.First()) as FamilySymbol;
                Parameter guidParam = symb.LookupParameter("RBS_GUID");
                Parameter versionParam = symb.LookupParameter("RBS_VERSION");
                if (guidParam == null || versionParam == null)
                {
                    noGuids.Add(fam);
                    continue;
                }
                string guid = guidParam.AsString();
                if(guidsAndFamilies.ContainsKey(guid))
                {
                    guidsAndFamilies[guid].Add(fam);
                }
                else
                {
                    guidsAndFamilies.Add(guid, new List<Family> { fam });
                }
            }

            Dictionary<string, List<Family>> duplicates = guidsAndFamilies
                .Where(i => i.Value.Count > 1)
                .ToDictionary(i => i.Key, i => i.Value);

            string guids = string.Join(",", guidsAndFamilies.Keys);

            WebConnection connect = new WebConnection(App.settings.Email, App.settings.Password, App.settings.Website);
            ServerResponse famInfoResponse = connect.Request("familygetinfo", new Dictionary<string, string>() { ["guid"] = guids });
            if (famInfoResponse.Statuscode >= 400)
            {
                message = famInfoResponse.Message;
                return Result.Failed;
            }

            Dictionary<string, FamilyCard> checkInfos = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, FamilyCard>>(famInfoResponse.Message);

            List<string[]> incorrectNames = new List<string[]>();
            List<string[]> obsoleteVersions = new List<string[]>();

            foreach (var kvp in checkInfos)
            {
                Family fam = guidsAndFamilies[kvp.Key][0];
                FamilyCard info = kvp.Value;
                if(info.exists == false)
                {
                    noGuids.Add(fam);
                    continue;
                }
                string famnameInProject = fam.Name;
                string famNameByServer = info.shortinfo.GetFamilyName();
                if(famnameInProject != famNameByServer)
                {
                    incorrectNames.Add(new[] { fam.Id.IntegerValue.ToString(), famnameInProject, famNameByServer, kvp.Key });
                }
                IEnumerable<ElementId> symbIds = fam.GetFamilySymbolIds();
                if (symbIds.Count() == 0)
                {
                    invalidSymbols.Add(fam);
                    continue;
                }
                FamilySymbol symb = doc.GetElement(fam.GetFamilySymbolIds().First()) as FamilySymbol;
                int versionInProject = symb.LookupParameter("RBS_VERSION").AsInteger();
                FamilyVersion lastVersion = info.GetLastActualVersion();
                int versionInLibrary = lastVersion.version;
                if(versionInLibrary > versionInProject)
                {
                    string[] obsoleteInfo = new[]
                    {
                        fam.Id.IntegerValue.ToString(),
                        fam.Name,
                        versionInProject.ToString(),
                        versionInLibrary.ToString(),
                        lastVersion.changes,
                        kvp.Key
                    };
                    obsoleteVersions.Add(obsoleteInfo);
                }
            }

            FormCheckFamilies form = new FormCheckFamilies(duplicates, obsoleteVersions, noGuids, incorrectNames);
            form.ShowDialog();

            return Result.Succeeded;
        }
    }
}
