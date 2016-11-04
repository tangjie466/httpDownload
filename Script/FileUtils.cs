using UnityEngine;
using System.Collections;
using System.IO;
public class FileUtils  {

	private static FileUtils _instance;
	public static FileUtils Instance
	{
		get{
			if(_instance == null)
			{
				_instance = new FileUtils();
			}
			return _instance;
		}
	}


	public FileStream CreateFile(string filePath)
	{
		string floderPath = filePath.Substring(0,filePath.LastIndexOf("/"));
		if (Directory.Exists(floderPath) == false)//如果不存在就创建file文件夹
		{
			Directory.CreateDirectory(floderPath);
		}
		if (File.Exists (filePath)) {
			File.Delete (filePath);
		}
		FileStream f = new FileStream (filePath, FileMode.Create);
		return f;
	}

	public void close(FileStream f)
	{
		if (f != null) {
			f.Close();
			f = null;
		}
	}


}
