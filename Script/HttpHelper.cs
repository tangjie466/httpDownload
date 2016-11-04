using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.IO;
using MyThread;
internal class WebReqData
{
	private FileStream fileStream;
	public static  int requestTimeout = 10000;
	public static  int readTimeout = 5000;
	public static  int responseTimeout = 5000;
	public const int BufferSize = 1024 *2;
	public static  int sleepTime = 100;
	public Stream OrginalStream;
	public HttpWebResponse WebResponse;
	private string filePath = string.Empty;
	public string FilePath
	{
		get{return filePath;}
	}

	public WebReqData(String filePath)
	{
		Buffer = new byte[BufferSize];
		this.filePath = filePath;


	}
	public FileStream getFileStream(FileMode mode)
	{
		string saveFile = filePath.Substring(0,filePath.LastIndexOf('/'));
		if (Directory.Exists(saveFile) == false)//如果不存在就创建file文件夹
		{
			Directory.CreateDirectory(saveFile);
		}
		if (System.IO.File.Exists(filePath)) 
		{ 
			if(mode == FileMode.Create)
			{
				System.IO.File.Delete(filePath);
				fileStream = new FileStream(filePath,mode);
				return fileStream;
			}
			fileStream= System.IO.File.OpenWrite(filePath); 


		} 
		else 
		{ 
			fileStream = new FileStream (filePath,mode);
		}

		return fileStream;
	}

}

public class HttpHelper{
	byte[] bytes;
	const int MAX_SIZE = 1024 * 1024 * 8;
	long contlength = 0;
	long curContLength = 0;
	public bool isDone = false;
	public string url = string.Empty;
	public byte[] Bytes
	{
		get{return bytes};
	}
	public long getProgress
	{
		get{return curContLength;}
	}

	public long getContLength
	{
		get{return contlength;}
	}


	string savePath = null;
	public string assetName;
	public HttpHelper(int size)
	{
		bytes = new byte[size];
	}
	public void init()
	{
		contlength = 0;
		curContLength = 0;
		isDone = false;
	}
	public string SavePath
	{
		get{return savePath;}
		set{savePath = value;}
	}

	public void continueDownload(string url,string filename)
	{
		System.GC.Collect();
		if (DownLoadThreadPool.instance.isStop) {
			return;
		}
		assetName = filename;
		HttpWebRequest req = null;
		HttpWebResponse response = null;
		WebReqData reqData = null;
		FileStream fileStream = null;
		try{
			reqData = new WebReqData (savePath + assetName);
			fileStream = reqData.getFileStream (FileMode.Append);
			curContLength = fileStream.Length;
			fileStream.Seek(curContLength,SeekOrigin.Current);
			req = WebRequest.Create (url+filename) as HttpWebRequest;
			req.AddRange((int)curContLength);
			req.Timeout = WebReqData.requestTimeout;
			req.ReadWriteTimeout = WebReqData.responseTimeout;
			response = (HttpWebResponse)req.GetResponse ();
			contlength = response.ContentLength + curContLength;
			if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.PartialContent) {
				response.Close();
				req.Abort();
				Debug.LogError("responce code is "+response.StatusCode);
				isDone = true;
				return;
			}
			reqData.WebResponse = response;
			reqData.OrginalStream = response.GetResponseStream ();
			
			reqData.OrginalStream.ReadTimeout = WebReqData.readTimeout;
			int len = reqData.OrginalStream.Read(reqData.Buffer,0,WebReqData.BufferSize);

			curContLength += len;
			while(len > 0)
			{
				if(DownLoadThreadPool.instance.isStop)
				{
					
					break;
				}
				
				fileStream.Write(reqData.Buffer,0,len);
				fileStream.Flush();
				len = reqData.OrginalStream.Read(reqData.Buffer,0,WebReqData.BufferSize);
				curContLength += len;
			}



			fileStream.Close ();
			fileStream = null;
			reqData.OrginalStream.Close ();
			reqData.OrginalStream = null;
			reqData.WebResponse.Close ();
			reqData.WebResponse = null;
			req.Abort ();
			req = null;
			if(curContLength < contlength)
			{
				continueDownload(url,filename);
			}else{
				isDone = true;
				Debug.LogError ("sucess");
			}


		}catch(Exception e)
		{
			Debug.LogError("url is "+url+filename+" exception is "+e.Message);
			if(fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
			if(reqData != null)
			{
				reqData.OrginalStream.Close();
				reqData.OrginalStream = null;
				reqData.WebResponse.Close();
				reqData.WebResponse = null;
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

		assetName = filename;
		HttpWebRequest req = null;
		HttpWebResponse response = null;
		WebReqData reqData = null;
		Stream responseStream = null;
		FileStream fileStream = null;

		try{
			req = WebRequest.Create (url+filename) as HttpWebRequest;
			req.Timeout = WebReqData.requestTimeout;
			req.ReadWriteTimeout = WebReqData.responseTimeout;
			
			response = (HttpWebResponse)req.GetResponse ();
			contlength = response.ContentLength;
			if (response.StatusCode != HttpStatusCode.OK) {
				response.Close();
				req.Abort();
				isDone = true;
				return;
			}
			reqData = new WebReqData (savePath + assetName);
			reqData.WebResponse = response;
			responseStream = response.GetResponseStream ();
			reqData.OrginalStream = responseStream;

			reqData.OrginalStream.ReadTimeout = WebReqData.readTimeout;
			int len = reqData.OrginalStream.Read(reqData.Buffer,0,WebReqData.BufferSize);
			fileStream = reqData.getFileStream (FileMode.Create);
			curContLength += len;
			while(len > 0)
			{
				if(DownLoadThreadPool.instance.isStop)
				{

					break;
				}
					
				fileStream.Write(reqData.Buffer,0,len);
				fileStream.Flush();
				len = reqData.OrginalStream.Read(reqData.Buffer,0,WebReqData.BufferSize);
				curContLength += len;
			}

			fileStream.Close ();
			fileStream = null;
			reqData.OrginalStream.Close ();
			reqData.OrginalStream = null;
			reqData.WebResponse.Close ();
			reqData.WebResponse = null;
			req.Abort ();
			req = null;
			isDone = true;
		}
		catch(Exception e)
		{
			Debug.LogError("url is "+url+filename+" exception is "+e.Message);
			if(fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
			if(reqData != null)
			{
				reqData.OrginalStream.Close();
				reqData.OrginalStream = null;
				reqData.WebResponse.Close();
				reqData.WebResponse = null;
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
