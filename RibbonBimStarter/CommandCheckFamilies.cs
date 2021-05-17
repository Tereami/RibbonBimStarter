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

            TaskDialog.Show("Test", "Функция в разработке! Здесь будет функционал: найти неверифицированные семейства," +
                "продублированные с разными именами, устаревшей версии и т.д.");
            return Result.Succeeded;
        }
    }
}
