using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Threading;
using MyThread;
using System.Collections.Generic;


public class test : MonoBehaviour {
	const int maxnum = 4;
 	
	HttpUtils h;
	Hashtable cur = new Hashtable ();
	List<string> resl = new List<string> ();
	List<string> del = new List<string>();
	string res_list = string.Empty;
	float t = 0;
	bool isStart = false;
	bool isGetRes = false;
	bool isGetReslist = false;
	int curnum = 0;
	int num = 0;
	int completnum = 0;
	static string rootPath = Application.streamingAssetsPath;
	// Use this for initialization
	void Awake()
	{
		res_list = Application.streamingAssetsPath+"/res/resList.txt";


	}
	void Start () {


		DownLoadThreadPool.instance.MAXNUM = maxnum;
		ThreadPool.SetMaxThreads(maxnum*5,maxnum*5);
		ThreadPool.SetMinThreads(maxnum,maxnum);
		System.Net.ServicePointManager.DefaultConnectionLimit = 512;
	}
	
	void getRelist()
	{
		{
			isGetReslist = true;
			h=null;
			h = new HttpUtils(new HttpUtils.SucDelegate(getReslistComplete),new HttpUtils.FailDelegate(getReslistFail));
			h.DownLoadBundle("/res/resList.txt");
			return;
		}
		/*
		if (!System.IO.File.Exists (reslist)) {
			isGetReslist = true;
			h=null;
			h = new HttpUtils(new HttpUtils.SucDelegate(getReslistComplete),new HttpUtils.FailDelegate(getReslistFail));
			h.DownLoadBundle("/res/resList.txt");
			return;
		}
		isGetReslist = false;
		StreamReader sr = new StreamReader (reslist, Encoding.Default);
		string line;
		string data;
		while ((line = sr.ReadLine())!=null) {
			data = line;
			resl.Add(data);
		}
		isStart =true;
		num = resl.Count;
		isGetRes = true;
	*/
	}
	void getReslistComplete(string s)
	{
		FileStream f = FileUtils.Instance.CreateFile(res_list);
		f.Write(h.help.Bytes,0,(int)h.help.ToTleSize);
		FileUtils.Instance.close(f);

		string res = System.Text.Encoding.Default.GetString(h.help.Bytes);
		string[] reslist = res.Split('\n');
		for(int i=0;i<reslist.Length;i++)
		{
			string s1 = reslist[i];
			resl.Add(s1);
		}
		Debug.LogError ("reslist num is " + reslist.Length);
		isStart =true;
		num = resl.Count;
		isGetRes = true;
		isGetReslist = false;
		h.destory();
		h = null;
	}

	void getReslistFail(string s)
	{

	}

	void suc(string s)
	{
		if (cur.Contains (s) && !del.Contains(s)) {
			HttpUtils h = (HttpUtils)cur[s];
			FileStream f = FileUtils.Instance.CreateFile(rootPath+h.help.FilePath);
			f.Write(h.help.Bytes,0,(int)h.help.ToTleSize);
			FileUtils.Instance.close(f);
			del.Add(s);
			completnum++;
		}
	}

	void fail(string s)
	{
		HttpUtils h = (HttpUtils)cur[s];
		h.reDownLoadBundle ();
	}
	// Update is called once per frame
	void Update () {


		if (isStart) {
			if (del.Count > 0) {
				while (del.Count>0) {
					HttpUtils h = (HttpUtils)cur[del[0]];
					h.destory();
					h=null;
					cur.Remove (del [0]);
					del.RemoveAt (0);
				}
			}

			if (cur.Count < maxnum) {
				if (curnum > resl.Count - 1) {

				} else {

					string s = resl [curnum].Split(',')[0];
					HttpUtils h = new HttpUtils (new HttpUtils.SucDelegate (suc), new HttpUtils.FailDelegate (fail));

					cur.Add (s, h);
					h.DownLoadBundle (s);
					curnum++;
				}
			}

			foreach (DictionaryEntry h in cur) {
				((HttpUtils)h.Value).update ();
			}
			Debug.Log ("complete num is " + completnum + " totle num is " + num + " del count is " + del.Count);
			if (completnum == num && del.Count == 0) {
				isStart = false;
				Debug.LogError ("success time is " + (Time.realtimeSinceStartup - t));


			}


		} else {
			if(h != null )
			{
				h.update();
			}
		} 
	}
	void OnGUI()
	{
		if (!isStart && completnum == num && del.Count == 0 && !isGetReslist) {
			GUI.TextArea(new Rect (300, 100 + 50, 200, 30), "download is sussesc");
		}
		string conttext = "getres";
		if (isGetReslist) {
			conttext = "downloadres";
		}

		if (GUI.Button (new Rect (0, 0, 100, 30), conttext)) {
			
			/*	for (int i =0; i<str.Length; i++) { //str是string型数组，存放部分assetbundle名字
				Thread thread = new Thread (new ParameterizedThreadStart (DownAsset)); //ParameterizedThreadStart 多线程传参 
				thread.Start (rootPath + "|" + str [i]); //只能带一个object参数 所以使用字符串拼接
			}
			*/
			t = Time.realtimeSinceStartup;
			getRelist ();

			
		}

		if (isGetReslist) {
			GUI.TextArea (new Rect (0, 100+50, 300, 30), h.help.FilePath + "");
			GUI.TextArea (new Rect (300, 100+50, 200, 30), "cur size :" + h.help.CurrentSize + "");
			GUI.TextArea (new Rect (500, 100+50, 200, 30), "totle size :" + h.help.ToTleSize + "");
		}


		int i = 0;
		foreach (DictionaryEntry hh in cur) {
			HttpUtils h = (HttpUtils)hh.Value;
			GUI.TextArea (new Rect (0, 100+i*50, 300, 30), h.help.FilePath + "");
			GUI.TextArea (new Rect (300, 100+i*50, 200, 30), "cur size :" + h.help.CurrentSize+ "");
			GUI.TextArea (new Rect (500, 100+i*50, 200, 30), "totle size :" + h.help.ToTleSize + "");
			i++;
		}


		
	}

	void OnApplicationQuit()
	{
		MyThread.DownLoadThreadPool.instance.isStop = true;
	}
}
