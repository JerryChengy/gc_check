using System;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Tests {
	public class MethodCall {
		public string m_Name;
		public List<string> m_ListCall = new List<string> ();
		public void Add(string callname)
		{
			m_ListCall.Add (callname);
		}
	}
	public class MethodCallSet {
		private static MethodCallSet _instance;
		public static MethodCallSet Instance {
			get {
				if (_instance == null) {
					_instance = new MethodCallSet ();
				}
				return _instance;
			}
		}
		private static int m_FuncRetTimes = 0;
		//存储函数名称与调用函数的对应信息
		Dictionary<string, MethodCall> m_DictMethodCall = new Dictionary<string, MethodCall> ();
		//存储update型的函数名
		public List<string> m_ListCallerName = new List<string>();
		//所需检索的有GC的函数名
		public string m_FuncName;
		//输出结果
		public List<string> m_ListFindName = new List<string> ();
		public void Init()
		{
			//m_ListCallerName.Add ("Controller::Update");
			//m_ListCallerName.Add ("OnViewUpdate");
			m_ListCallerName.Add ("ClientModule::Tick");
			//m_ListCallerName.Add ("FixedUpdate");
			//m_ListCallerName.Add ("LateUpdate");
			//m_ListCallerName.Add ("Update");
		}
		public void Add(MethodCall mc)
		{
			if (mc == null) {
				return;
			}
			if (m_DictMethodCall.ContainsKey(mc.m_Name)) {
				return;
			}
			
			m_DictMethodCall.Add (mc.m_Name, mc);
		}
		public string Exec()
		{
			foreach (var callername in m_ListCallerName) {
				MethodCall mc = null;
				foreach (var mciter in m_DictMethodCall) {
					if (!mciter.Key.Contains (callername)) {
						continue;
					}
					if (mciter.Key.EndsWith (callername)) {
						mc = mciter.Value;
						if (mc.m_Name == m_FuncName) {
							string strFindName = "[find:" + callername + ", " + m_FuncName + "]";
							return strFindName;
						}
						m_FuncRetTimes = 0;
						//找到起始mc
						if (Find (mc)) {
							string strFindName = "[find:" + callername + ", " + m_FuncName + "]";
							return strFindName;
						}
					}
				}
			}
			return string.Empty;
		}

		private bool Find(MethodCall mc)
		{
			m_FuncRetTimes++;
			if (m_FuncRetTimes >= 10) {
				return false;
			}
			if (mc == null) {
				return false;
			}
			if (mc.m_Name == m_FuncName) {
				return true;
			}
			foreach (var item in mc.m_ListCall) {
				
				if (item == m_FuncName) {
					return true;
				}
				//防止循环递归
				if (item == mc.m_Name) {
					break;
				}
				if (m_DictMethodCall.ContainsKey(item)) {
					bool bRet = Find (m_DictMethodCall [item]);
					if (bRet) {
						return true;
					}
				}				
			}
			return false;
		}
	}

}