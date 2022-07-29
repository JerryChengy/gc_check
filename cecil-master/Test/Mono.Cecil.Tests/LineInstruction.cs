using System;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Tests {
	public class LineInstruction {
		public int m_Line;
		public bool m_Begin = false;
		public string m_FullName;
		public int m_Times = 0;
		public string m_KeyWord;
		public string m_Param;
		public bool m_bIgnore;
		private List<LineInstructionKeyWord> m_ListLineInstructionKeyWord = new List<LineInstructionKeyWord> ();
		public void Begin ()
		{
			m_Begin = true;
			m_bIgnore = false;
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_LogSystem());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_DebugLog ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_QSLogger ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_FGUIData ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_UtilFormat ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_SBAppendFormat ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_ShowTip());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_ShowTipByPos ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_GfxSendmsg ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_StrConcat ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_StoryValueResult ());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_StrFormat());
			m_ListLineInstructionKeyWord.Add (new LineInstructionKeyWord_DictFormat());
			m_ListLineInstructionKeyWord.Add (new Lambda_DictFormat ());
			foreach (var item in m_ListLineInstructionKeyWord) {
				item.Init ();
			}
		}
		public string GetKeyWord ()
		{
			string strKeyWord = string.Empty;
			if (m_KeyWord!=null && m_KeyWord.Length > 0) {
				strKeyWord += "[" + m_KeyWord + "]";
				if (m_Param!=null && m_Param.Length > 0) {
					strKeyWord += "[" + m_Param + "]";
				}
			}
			if (strKeyWord.Length == 0) {
				return "[none]";
			}
			return strKeyWord;
		}
		public void CheckKeyWord(string strInstruction)
		{
			bool bFind = false;
			foreach (var item in m_ListLineInstructionKeyWord) {
				if (strInstruction.Contains(item.m_KeyWord)) {
					if (item.m_bIgnore) {
						m_bIgnore = true;
						break;
					}
					m_KeyWord = item.m_KeyWord;
					bFind = true;
				}
				if (item.m_Param != null && strInstruction.Contains(item.m_Param)) {
					m_Param = item.m_Param;
				}
				if (bFind) {
					//break;
				}
			}
		}
	}
	public class LineInstructionKeyWord {
		public string m_KeyWord;
		public string m_Param;
		public bool m_bIgnore;
		public virtual void Init()
		{
			m_bIgnore = false;
		}
	}
	public class LineInstructionKeyWord_LogSystem: LineInstructionKeyWord {
		public override void Init()
		{
			m_KeyWord = "LogSystem";
			m_bIgnore = true;
		}
	}
	public class LineInstructionKeyWord_DebugLog : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "Debug::Log";
			m_bIgnore = true;
		}
	}
	public class LineInstructionKeyWord_QSLogger : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "QSLogger";
			m_bIgnore = true;
		}
	}
	public class LineInstructionKeyWord_FGUIData : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "FairyGUI.GObject::data";
			m_Param = "System.Int32";
		}
	}
	public class LineInstructionKeyWord_UtilFormat : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "Utility::Format";
		}
	}
	public class LineInstructionKeyWord_SBAppendFormat : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "StringBuilder::AppendFormat";
		}
	}
	public class LineInstructionKeyWord_ShowTip: LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "UIMasterUtils::showTip";
		}
	}
	public class LineInstructionKeyWord_ShowTipByPos : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "UIMasterUtils::ShowTipByPos";
		}
	}
	public class LineInstructionKeyWord_GfxSendmsg: LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "GfxStorySystem::SendMessage";
		}
	}
	public class LineInstructionKeyWord_StrConcat: LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "String::Concat";
		}
	}
	public class LineInstructionKeyWord_StoryValueResult: LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "StoryValueResult::set_Value";
		}
	}
	public class LineInstructionKeyWord_StrFormat: LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "String::Format";
		}
	}
	public class LineInstructionKeyWord_DictFormat: LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "Dict::Format";
		}
	}
	public class Lambda_DictFormat : LineInstructionKeyWord {
		public override void Init ()
		{
			m_KeyWord = "System.Action";
		}
	}
}
