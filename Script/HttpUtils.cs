using UnityEngine;
using System.Collections;
using System.Threading;
using MyThread;
public class HttpUtils{
	//android上保存到 /storage/sdcard0/Android/data/包名(例如：com.example.test)/files
	//string urlpath = "http://47.90.62.133:8081/android_20161020/";
	string urlpath = "http://res.weiye-global.haoxingame.com/android_20161020";
	string name = string.Empty;
	public HttpHelper help;

	public delegate void SucDelegate(string s);
	public delegate void FailDelegate(string s);

	SucDelegate suc;
	FailDelegate fail;

	public HttpUtils(SucDelegate s,FailDelegate f)
	{
		string rootPath = Application.persistentDataPath;;
		help = new HttpHelper();
		help.SavePath = rootPath;
		suc = s;
		fail = f;
	}

	public void reDownLoadBundle()
	{
		help.init ();
		DownLoadThreadPool.instance.start(new WaitCallback(DownAsset),help);
	}

	public void DownLoadBundle(string url)
	{
		name = url;
		help.url = url;
		DownLoadThreadPool.instance.start(new WaitCallback(DownAsset),help);
	}

	
	void DownAsset(System.Object h)
	{
		HttpHelper help = (HttpHelper)h;
		if(help != null)
		help.AsyDownLoad(urlpath,help.url);//注意在手机上测试 该对Ip地址
	}

	public void update()
	{
		if (help.isDone) {
			if(help.getContLength != help.getProgress)
			{
				onFail();
			}else{
				onSuc();
			}
		}
	}

	void onSuc()
	{
		Debug.Log("download succes!");
		if (suc != null) {
			suc.Invoke (name);
		}
	}

	void onFail()
	{
		Debug.LogError ("download fail! totle size is "+help.getContLength+"  cur size is "+help.getProgress);

		if (fail != null) {
			fail.Invoke (name);
		}
	}

}