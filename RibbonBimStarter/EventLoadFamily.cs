﻿#region License
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
        public static string familyguid;
        public static string familyname;
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            Family fam = null;
            FamilySymbol famSymb = null;

            List<Family> checkPreloadedFams = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(Family))
                    .Where(i => i.Name.Equals(familyname))
                    .Cast<Family>()
                    .ToList();
            if (checkPreloadedFams.Count > 0)
            {
                fam = checkPreloadedFams[0];
                Debug.WriteLine("Family has already loaded " + familyname);
            }
            else
            {
                Debug.WriteLine("Start download family: " + familyguid);
                WebConnection connect = new WebConnection(App.settings.Email, App.settings.Password, App.settings.Website);
                string fampath = connect.DownloadFamily(familyguid, familyname);
                if (connect.status != 200)
                {
                    TaskDialog.Show("Ошибка", connect.error);
                    return;
                }

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Load family");
                    bool loadSuccess = false;
                    try
                    {
                        loadSuccess = doc.LoadFamily(fampath, new FamilyLoadOptions(), out fam);
                    }
                    catch
                    {
                        Debug.WriteLine("Unable to load family: " + fampath);
                        return;
                    }
                    if (!loadSuccess)
                    {
                        TaskDialog.Show("Ошибка", "Не удалось загрузить " + fampath);
                        return;
                    }
                    //string familyname = Path.GetFileNameWithoutExtension(fampath);

                    List<Family> fams = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfClass(typeof(Family))
                        .Where(i => i.Name.Equals(familyname))
                        .Cast<Family>()
                        .ToList();
                    if (fams.Count == 0)
                    {
                        TaskDialog.Show("Ошибка", "Не удалось найти загруженное семейство " + familyname);
                        Debug.WriteLine("Loaded family isnt found: " + familyname);
                        return;
                    }
                    fam = fams[0];
                    doc.Regenerate();
                    t.Commit();
                }
            }
            famSymb = doc.GetElement(fam.GetFamilySymbolIds().First()) as FamilySymbol;
            if (!famSymb.IsActive)
            {
                using (Transaction t2 = new Transaction(doc))
                {
                    t2.Start("Activate symbol");
                    famSymb.Activate();
                    Debug.WriteLine("Family symbol has activated");
                    t2.Commit();
                }
            }

            app.ActiveUIDocument.PostRequestForElementTypePlacement(famSymb);
            Debug.WriteLine("Family is loaded succesfully");
        }

        public string GetName()
        {
            return "Bim-Starter_LoadFamily_Event";
        }
    }
}
