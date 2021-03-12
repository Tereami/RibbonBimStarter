using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;

namespace RibbonBimStarter
{
    public class EventLoadFamily : IExternalEventHandler
    {
        public static string familyPath;
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            Family fam = null;
            FamilySymbol famSymb = null;
            bool loadSuccess = false;
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Load family");
                loadSuccess = doc.LoadFamily(familyPath, new FamilyLoadOptions(), out fam);

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
                    return;
                }
                fam = fams[0];

                doc.Regenerate();
                famSymb = doc.GetElement(fam.GetFamilySymbolIds().First()) as FamilySymbol;
                if (!famSymb.IsActive)
                {
                    famSymb.Activate();
                }
                t.Commit();
            }
            app.ActiveUIDocument.PostRequestForElementTypePlacement(famSymb);
        }

        public string GetName()
        {
            return "Load family event";
        }
    }
}
