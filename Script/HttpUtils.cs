using UnityEngine;
using System.Collections;
using System.Threading;
using MyThread;
public class HttpUtils{
	//android上保存到 /storage/sdcard0/Android/data/包名(例如：com.example.test)/files
	//string urlpath = "http://47.90.62.133:8081/android_20161020/";
	string urlpath = "http://res.weiye-global.haoxingame.com/android_20161020";
	public HttpHelper help;

	public delegate void SucDelegate(string s);
	public delegate void FailDelegate(string s);

	SucDelegate suc;
	FailDelegate fail;

	public HttpUtils(SucDelegate s,FailDelegate f,int size)
	{
		help = new HttpHelper(size);
		suc = s;
		fail = f;
	}

	public void reDownLoadBundle()
	{
		help.init ();
		DownLoadThreadPool.instance.start(new WaitCallback(DownAsset),help);
	}

	public void DownLoadBundle(string filePath)
	{
		help.FilePath = filePath;
		DownLoadThreadPool.instance.start(new WaitCallback(DownAsset),help);
	}

	
	void DownAsset(System.Object h)
	{
		HttpHelper help = (HttpHelper)h;
		if(help != null)
		help.AsyDownLoad(urlpath,help.FilePath);//注意在手机上测试 该对Ip地址
	}

	public void update()
	{
		if (help.isDone) {
			if(help.ToTleSize != help.CurrentSize)
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
			suc.Invoke (help.FilePath);
		}
	}

	public void destory()
	{
		help.destory ();
		help = null;
		if (suc != null) {
			suc = null;
		}
		if (fail != null) {
			fail = null;
		}
	}

	void onFail()
	{
		Debug.LogError ("download fail! totle size is "+help.ToTleSize+"  cur size is "+help.CurrentSize);

		if (fail != null) {
			fail.Invoke (help.FilePath);

		}
	}

}