using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
				string text = current.Substring(0, current.Length - 3) + "jpg";
				bool flag = File.Exists(text);
				bool flag2 = !flag;
				if (flag2)
				{
					text = current.Substring(0, current.Length - 3) + "png";
					flag = File.Exists(text);
				}
				bool flag3 = flag;
				if (flag3)
				{
					familyFileInfo.ImagePath = text;
				}
				string text2 = Path.GetDirectoryName(current).Split(new char[]
				{
					'\\'
				}).Last<string>();
				familyFileInfo.FolderTitle = text2;
				bool flag4 = dictionary.ContainsKey(text2);
				bool flag5 = flag4;
				if (flag5)
				{
					ObservableCollection<FamilyFileInfo> observableCollection = dictionary[text2];
					observableCollection.Add(familyFileInfo);
				}
				else
				{
					dictionary.Add(text2, new ObservableCollection<FamilyFileInfo>
					{
						familyFileInfo
					});
				}
			}
			return dictionary;
		}

	}
}
