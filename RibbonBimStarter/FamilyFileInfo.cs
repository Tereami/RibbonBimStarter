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


namespace RibbonBimStarter
{
    public class FamilyFileInfo
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string FilePath { get; set; }

        public string FolderTitle { get; set; }

        public string ImagePath { get; set; }

        public string Description { get; set; }

        public string ShortDescription { get; set; }

    }
}
