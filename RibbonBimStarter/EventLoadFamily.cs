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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
#endregion

namespace RibbonBimStarter
{
    public class EventLoadFamily : IExternalEventHandler
    {
        public static string familyPath;
        public void Execute(UIApplication app)
        {
            Debug.WriteLine("Start load family, path: " + familyPath);
            Document doc = app.ActiveUIDocument.Document;
            Family fam = null;
            FamilySymbol famSymb = null;
            bool loadSuccess = false;
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Load family");
                try
                {
                    loadSuccess = doc.LoadFamily(familyPath, new FamilyLoadOptions(), out fam);
                }
                catch
                {
                    Debug.WriteLine("Unable to load family: " + familyPath);
                    return;
                }
                if (!loadSuccess)
                {
                    TaskDialog.Show("Ошибка", "Не удалось загрузить " + familyPath);
                    return;
                }
                string famName = Path.GetFileNameWithoutExtension(familyPath);
                List<Family> fams = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(Family))
                    .Where(i => i.Name.Equals(famName))
                    .Cast<Family>()
                    .ToList();
                if (fams.Count == 0)
                {
                    TaskDialog.Show("Ошибка", "Не удалось найти загруженное семейство " + famName);
                    Debug.WriteLine("Loaded family isnt found: " + famName);
                    return;
                }
                fam = fams[0];

                doc.Regenerate();
                famSymb = doc.GetElement(fam.GetFamilySymbolIds().First()) as FamilySymbol;
                if (!famSymb.IsActive)
                {
                    famSymb.Activate();
                    Debug.WriteLine("Family symbol has activated");
                }
                t.Commit();
            }
            app.ActiveUIDocument.PostRequestForElementTypePlacement(famSymb);
            Debug.WriteLine("Family is loaded succesfully");
        }

        public string GetName()
        {
            return "Load family event";
        }
    }
}
