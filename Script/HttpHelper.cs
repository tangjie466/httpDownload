using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.IO;
using MyThread;
internal class WebReqData
{
	public static  int requestTimeout = 10000;
	public static  int readTimeout = 5000;
	public static  int responseTimeout = 5000;
	public static  int BufferSize = 1024 *2;
	public static  int sleepTime = 100;
}

public class HttpHelper{
	private string downLoadUrl = string.Empty;
	private long totleSize = 0;
	private int currentSize = 0;
	byte[] bytes;
	const int MAX_SIZE = 1024 * 1024 * 8;
	public bool isDone = false;
	public string url = string.Empty;
	private string filePath = string.Empty;

	public void destory()
	{
		Array.Clear (bytes, 0, bytes.Length);
		bytes = null;
	}

	public byte[] Bytes
	{
		get{return bytes;}
	}

	public int CurrentSize
	{
		get{return currentSize;}
		set{currentSize = value;}
	}
	
	public long ToTleSize
	{
		get{return totleSize;}
		set{totleSize = value;}
	}
	
	public string UrlPath
	{
		get{return downLoadUrl;}
		set{downLoadUrl = value;}
	}
	
	public string FilePath
	{
		get{return filePath;}
		set{filePath = value;}
	}
	public HttpHelper()
	{
		totleSize = 0;
		bytes = new byte[MAX_SIZE];

	}
	public void init()
	{
		Array.Clear (bytes, 0, bytes.Length);
		currentSize = 0;
		isDone = false;
	}

	public void continueDownload(string url,string filename)
	{
		System.GC.Collect();
		if (DownLoadThreadPool.instance.isStop) {
			return;
		}
		UrlPath = url;
		filePath = filename;
		HttpWebRequest req = null;
		HttpWebResponse response = null;
		Stream responseStream = null;
		try{
			int cur_len = bytes.Length;
			if(cur_len != currentSize)
			{
				Debug.LogError(" curentsize is "+currentSize+" , bytes size is "+cur_len+",file path is"+filePath);
			}
			req = WebRequest.Create (url+filename) as HttpWebRequest;
			req.AddRange((int)currentSize);
			req.Timeout = WebReqData.requestTimeout;
			req.ReadWriteTimeout = WebReqData.responseTimeout;
			response = (HttpWebResponse)req.GetResponse ();
			if(totleSize == 0)
			{
				totleSize = response.ContentLength;
			}
			if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent) {
				response.Close();
				req.Abort();
				Debug.LogError("responce code is "+response.StatusCode);
				isDone = true;
				return;
			}
			responseStream = response.GetResponseStream ();
			
			responseStream.ReadTimeout = WebReqData.readTimeout;
			int len = responseStream.Read(bytes,currentSize,WebReqData.BufferSize);

			currentSize += len;
			while(len > 0)
			{
				if(DownLoadThreadPool.instance.isStop)
				{
					
					break;
				}
				len = responseStream.Read(bytes,currentSize,WebReqData.BufferSize);
				currentSize += len;
			}

			response.Close();
			response = null;
			responseStream.Close();
			responseStream = null;
			req.Abort ();
			req = null;
			if(currentSize < totleSize)
			{
				continueDownload(url,filename);
			}else{
				isDone = true;
				Debug.LogError ("sucess");
			}


		}catch(Exception e)
		{
			Debug.LogError("url is "+url+filename+" exception is "+e.Message);
			if(response != null)
			{
				response.Close();
				response = null;
			}
			if(responseStream != null)
			{
				responseStream.Close();
				responseStream = null;
			}
			if(req != null)
			{
				req.Abort();
				req = null;
			}
			System.Threading.Thread.Sleep(WebReqData.sleepTime);
			continueDownload(url,filename);
			Debug.LogError("fail");
		}
	}

	public void AsyDownLoad(string url,string filename)
	{
		System.GC.Collect();
		if (DownLoadThreadPool.instance.isStop) {
			return;
		}

		UrlPath = url;
		HttpWebRequest req = null;
		HttpWebResponse response = null;
		Stream responseStream = null;
		try{
			req = WebRequest.Create (url+filePath) as HttpWebRequest;
			req.Timeout = WebReqData.requestTimeout;
			req.ReadWriteTimeout = WebReqData.responseTimeout;
			response = (HttpWebResponse)req.GetResponse ();
			long contlength = response.ContentLength;
			if(totleSize == 0)
			{
				totleSize = contlength;
			}
			if(contlength != 0 && contlength != totleSize)
			{
				Debug.LogError("contlength is "+contlength+" , totlesize is "+totleSize+" filepath is "+filePath);
			}

			if (response.StatusCode != HttpStatusCode.OK) {
				response.Close();
				req.Abort();
				isDone = true;
				return;
			}
			responseStream = response.GetResponseStream ();
			responseStream.ReadTimeout = WebReqData.readTimeout;
			int len = responseStream.Read(bytes,0,WebReqData.BufferSize);
			currentSize += len;
			while(len > 0)
			{
				if(DownLoadThreadPool.instance.isStop)
				{

					break;
				}
				len = responseStream.Read(bytes,currentSize,WebReqData.BufferSize);
				currentSize += len;
			}
			responseStream.Close();
			responseStream = null;
			response.Close();
			response = null;
			req.Abort ();
			req = null;
			if(currentSize < totleSize)
			{
				continueDownload(url,filename);
			}else{
				isDone = true;
				Debug.LogError ("sucess");
			}
		}
		catch(Exception e)
		{
			Debug.LogError("url is "+url+filename+" exception is "+e.Message);

			if(response != null)
			{
				response.Close();
				response = null;
			}
			if(responseStream != null)
			{
				responseStream.Close();
				responseStream = null;
			}
			if(req != null)
			{
				req.Abort();
				req = null;
			}
			System.Threading.Thread.Sleep(WebReqData.sleepTime);
			continueDownload(url,filename);
			Debug.LogError("fail");
		}
	}








}
