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
using System.IO;
#endregion

namespace RibbonBimStarter
{
    public static class FilesScaner
    {
		public static List<string> DirSearch(string Directory, string FileType)
		{
			List<string> list = new List<string>();
			List<string> result;
			try
			{
				string[] files = System.IO.Directory.GetFiles(Directory);
				for (int i = 0; i < files.Length; i++)
				{
					string text = files[i];
					bool flag = text.EndsWith(FileType);
					if (flag)
					{
						list.Add(text);
					}
				}
				string[] directories = System.IO.Directory.GetDirectories(Directory);
				for (int j = 0; j < directories.Length; j++)
				{
					string directory = directories[j];
					list.AddRange(FilesScaner.DirSearch(directory, FileType));
				}
			}
			catch
			{
				result = null;
				return result;
			}
			result = list;
			return result;
		}

		public static Dictionary<string, ObservableCollection<FamilyFileInfo>> GetInfo(string folderPath)
		{
			Dictionary<string, ObservableCollection<FamilyFileInfo>> dictionary = new Dictionary<string, ObservableCollection<FamilyFileInfo>>();
			List<string> list = FilesScaner.DirSearch(folderPath, ".rfa");
			foreach (string current in list)
			{
				FamilyFileInfo familyFileInfo = new FamilyFileInfo();
				familyFileInfo.FilePath = current;
				familyFileInfo.Title = Path.GetFileName(current);
				string imageFilePath = current.Substring(0, current.Length - 3) + "jpg";
				bool imageExists = File.Exists(imageFilePath);
				if (!imageExists)
				{
					imageFilePath = current.Substring(0, current.Length - 3) + "png";
					imageExists = File.Exists(imageFilePath);
				}
				if (imageExists)
				{
					familyFileInfo.ImagePath = imageFilePath;
				}
				else
                {
					continue;
                }
				string folderTitle = Path.GetDirectoryName(current).Split('\\').Last();
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
