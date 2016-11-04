using UnityEngine;
using System.Collections;
using System.Threading;
using System;
namespace MyThread
{
	public class DownLoadThreadPool{
		int maxNum = 10;
		public bool isStop = false;
		public int MAXNUM
		{
			get{return maxNum;}
			set{ 
				maxNum = value;
			}
		}
		ArrayList threadPool = new ArrayList();
		static DownLoadThreadPool _instance = null;
		public static  DownLoadThreadPool instance
		{
			get{
				if(_instance == null)
				{
					_instance = new DownLoadThreadPool();
				}
				return _instance;
			}
		}


		public void start(WaitCallback c,object p)
		{
			ThreadPool.QueueUserWorkItem (c,p);
		}


	}
}