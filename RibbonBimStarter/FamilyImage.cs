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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.DB;

namespace RibbonBimStarter
{
    public static class FamilyImage
    {
        public static string ImageCreate(Document FamilyDoc)
        {
            using (Transaction t = new Transaction(FamilyDoc))
            {
                t.Start("Создание вида для экспорта");

                View view = null;
                try
                {
                    view = Create3DView(FamilyDoc, "EXPORT");
                }
                catch
                {
                    view = GetAnyView(FamilyDoc);
                }

                if (view == null)
                    return null;

                try
                {
                    view.DisplayStyle = DisplayStyle.ShadingWithEdges;
                    view.Scale = 1;
                    view.DetailLevel = ViewDetailLevel.Fine;
                    view.AreAnnotationCategoriesHidden = true;
                }
                catch { }

                try
                {
                    List<ElementId> ids = new FilteredElementCollector(FamilyDoc)
                        .OfClass(typeof(ConnectorElement))
                        .ToElementIds()
                        .ToList();
                    FamilyDoc.Delete(ids);
                }
                catch { }

                try
                {
                    List<ElementId> ids = new FilteredElementCollector(FamilyDoc)
                        .OfCategory(BuiltInCategory.OST_Lines)
                        .WhereElementIsNotElementType()
                        .ToElementIds()
                        .ToList();
                    FamilyDoc.Delete(ids);
                }
                catch { }


                t.Commit();


                IList<ElementId> viewsId = new List<ElementId>();
                viewsId.Add(view.Id);

                string name = FamilyDoc.Title.Replace(".rfa", "");
                string folder = Path.GetDirectoryName(FamilyDoc.PathName);
                

                ImageExportOptions opt = new ImageExportOptions
                {
                    ZoomType = ZoomFitType.FitToPage,
                    PixelSize = 300,
                    FilePath = Path.Combine(folder, name),
                    FitDirection = FitDirectionType.Vertical,
                    HLRandWFViewsFileType = ImageFileType.JPEGMedium,
                    ImageResolution = ImageResolution.DPI_300,
                    ExportRange = ExportRange.SetOfViews
                };

                opt.SetViewsAndSheets(viewsId);
                

                FamilyDoc.ExportImage(opt);

                string viewTypename = FamilyDoc.GetElement(view.GetTypeId()).Name;
                string imagepathjpg = Path.Combine(folder, name + " - " + viewTypename + " - " + view.Name + ".jpg");

                return imagepathjpg;
            }
        }


        private static View3D Create3DView(Document doc, string name)
        {
            List<View3D> views = FindViews(doc, name);

            if (views.Count > 0)
            {
                View3D existView = views.First();
                return existView;
            }

            FilteredElementCollector col2 = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));
            List<ViewFamilyType> viewTypes = col2.Cast<ViewFamilyType>().ToList();

            viewTypes = viewTypes.Where(i => i.ViewFamily == ViewFamily.ThreeDimensional).ToList();

            ViewFamilyType viewType = viewTypes.First();

            ElementId viewTypeId = viewType.Id;

            View3D view = View3D.CreateIsometric(doc, viewTypeId);

            view.Name = name;

            return view;
        }

        private static List<View3D> FindViews(Document doc, string name)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc);
            IList<Element> elems = col.OfClass(typeof(View3D)).ToElements();
            List<View3D> views = col.Cast<View3D>().ToList();
            views = views.Where(i => i.Name.Equals(name)).ToList();
            views = views.Where(i => !i.IsTemplate).ToList();

            return views;
        }


        private static View GetAnyView(Document FamilyDoc)
        {
            List<ViewPlan> views = new FilteredElementCollector(FamilyDoc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>().ToList();

            if (views.Count != 0)
                return views.First() as View;

            List<ViewSheet> sheets = new FilteredElementCollector(FamilyDoc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>().ToList();

            if (sheets.Count != 0)
                return sheets.First() as View;

            return null;
        }
    }
}
