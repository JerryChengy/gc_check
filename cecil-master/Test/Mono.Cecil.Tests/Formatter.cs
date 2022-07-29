using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Mono.Cecil.Tests {

	public static class Formatter {
		public static string m_FileName;
		public static string FormatInstruction (Instruction instruction)
		{
			var writer = new StringWriter ();
			WriteInstruction (writer, instruction);
			return writer.ToString ();
		}

		public static string FormatMethodBody (MethodDefinition method)
		{
			var writer = new StringWriter ();
			WriteMethodBody (writer, method);
			return writer.ToString ();
		}

		public static void WriteMethodBody (TextWriter writer, MethodDefinition method)
		{
			var body = method.Body;
			if (body == null) {
				return;
			}
			int startLine = 0;
			foreach (Instruction instruction in body.Instructions) {
				var sequence_point = body.Method.DebugInformation.GetSequencePoint (instruction);
				if (sequence_point != null) {
					startLine = sequence_point.StartLine;
				}
				if (instruction.opcode.Name.CompareTo ("newobj") == 0) {
					writer.Write (method.FullName+":"+startLine+"\n");
				}
			}

			WriteExceptionHandlers (writer, body);
		}

		public static string FormatMethodBody1 (MethodDefinition method, string filter)
		{
			var writer = new StringWriter ();
			WriteMethodBody1 (writer, method, filter);
			return writer.ToString ();
		}
		public static void FormatCallMethodInfo(MethodDefinition method)
		{
			var body = method.Body;
			if (body == null) {
				return;
			}
			string strFullName = method.DeclaringType + "::" + method.Name;
			MethodCall mc = new MethodCall ();
			mc.m_Name = strFullName;
			foreach (Instruction instruction in body.Instructions) {
				var sequence_point = body.Method.DebugInformation.GetSequencePoint (instruction);
				
				if (instruction.opcode.Name.CompareTo ("call") == 0 || instruction.opcode.Name.CompareTo ("callvirt") == 0) {
					string callfuncname = ((MethodReference)instruction.operand).DeclaringType + "::" + ((MethodReference)instruction.operand).Name;
					mc.Add (callfuncname);
				}
			}
			MethodCallSet.Instance.Add (mc);
		}
		public static void WriteMethodBody1 (TextWriter writer, MethodDefinition method, string filter)
		{
			var body = method.Body;
			if (body == null) {
				return;
			}
			string strFullName = method.DeclaringType + "::" + method.Name;
			
			List<LineInstruction> m_LIList = new List<LineInstruction> ();
			int startLine = 0;
			LineInstruction li = null;
			string strLogSystem = string.Empty;
			foreach (Instruction instruction in body.Instructions) {
				var sequence_point = body.Method.DebugInformation.GetSequencePoint (instruction);
				if (sequence_point != null) {
					if (startLine != sequence_point.StartLine) {
						if (li != null) {
							m_LIList.Add (li);
						}						
						li = new LineInstruction ();
						li.Begin ();
						li.m_Line = sequence_point.StartLine;
						li.m_FullName = method.FullName;
					}
					startLine = sequence_point.StartLine;
				}
				if (li != null) {
					li.CheckKeyWord (instruction.ToString ());
				}
						
				if (instruction.opcode.Name.CompareTo (filter) == 0) {
					if (li != null) {
						li.m_Times++;
					}
				}
			}
			if (li != null) {
				m_LIList.Add (li);
			}
			bool bIgnore = false;
			foreach (var item in m_LIList) {
				if (item.m_bIgnore) {
					bIgnore = true;
					break;
				}
			}
			if (!bIgnore) {
				foreach (var item in m_LIList) {
					if (item.m_Times > 0) {
						MethodCallSet.Instance.m_FuncName = strFullName;
						string strResult = MethodCallSet.Instance.Exec ();
						writer.Write (m_FileName + "|" + strResult + item.GetKeyWord () + item.m_FullName + "|" + item.m_Line + "\n");

					}
				}
			}
			
		}

		static void WriteVariables (TextWriter writer, MethodBody body)
		{
			if (body == null) {
				return;
			}
			var variables = body.Variables;

			writer.Write ('\t');
			writer.Write (".locals {0}(", body.InitLocals ? "init " : string.Empty);

			for (int i = 0; i < variables.Count; i++) {
				if (i > 0)
					writer.Write (", ");

				var variable = variables [i];

				writer.Write ("{0} {1}", variable.VariableType, GetVariableName (variable, body));
			}
			writer.WriteLine (")");
		}

		static string GetVariableName (VariableDefinition variable, MethodBody body)
		{
			string name;
			if (body.Method.DebugInformation.TryGetName (variable, out name))
				return name;

			return variable.ToString ();
		}

		static void WriteInstruction (TextWriter writer, Instruction instruction)
		{
			writer.Write (FormatLabel (instruction.Offset));
			writer.Write (": ");
			writer.Write (instruction.OpCode.Name);
			if (null != instruction.Operand) {
				writer.Write (' ');
				WriteOperand (writer, instruction.Operand);
			}
		}

		static void WriteSequencePoint (TextWriter writer, SequencePoint sequence_point)
		{
			if (sequence_point.IsHidden) {
				writer.Write (".line hidden '{0}'", sequence_point.Document.Url);
				return;
			}

			writer.Write (".line {0},{1}:{2},{3} '{4}'",
				sequence_point.StartLine,
				sequence_point.EndLine,
				sequence_point.StartColumn,
				sequence_point.EndColumn,
				sequence_point.Document.Url);
		}

		static string FormatLabel (int offset)
		{
			string label = "000" + offset.ToString ("x");
			return "IL_" + label.Substring (label.Length - 4);
		}

		static string FormatLabel (Instruction instruction)
		{
			return FormatLabel (instruction.Offset);
		}

		static void WriteOperand (TextWriter writer, object operand)
		{
			if (null == operand) throw new ArgumentNullException ("operand");

			var target = operand as Instruction;
			if (null != target) {
				writer.Write (FormatLabel (target.Offset));
				return;
			}

			var targets = operand as Instruction [];
			if (null != targets) {
				WriteLabelList (writer, targets);
				return;
			}

			string s = operand as string;
			if (null != s) {
				writer.Write ("\"" + s + "\"");
				return;
			}

			var parameter = operand as ParameterDefinition;
			if (parameter != null) {
				writer.Write (ToInvariantCultureString (parameter.Sequence));
				return;
			}

			s = ToInvariantCultureString (operand);
			writer.Write (s);
		}

		static void WriteLabelList (TextWriter writer, Instruction [] instructions)
		{
			writer.Write ("(");

			for (int i = 0; i < instructions.Length; i++) {
				if (i != 0) writer.Write (", ");
				writer.Write (FormatLabel (instructions [i].Offset));
			}

			writer.Write (")");
		}

		static void WriteExceptionHandlers (TextWriter writer, MethodBody body)
		{
			//if (!body.HasExceptionHandlers)
			//	return;

			//foreach (var handler in body.ExceptionHandlers) {
			//	writer.Write ("\t");
			//	writer.WriteLine (".try {0} to {1} {2} handler {3} to {4}",
			//		FormatLabel (handler.TryStart),
			//		FormatLabel (handler.TryEnd),
			//		FormatHandlerType (handler),
			//		FormatLabel (handler.HandlerStart),
			//		FormatLabel (handler.HandlerEnd));
			//}
		}

		static string FormatHandlerType (ExceptionHandler handler)
		{
			var handler_type = handler.HandlerType;
			var type = handler_type.ToString ().ToLowerInvariant ();

			switch (handler_type) {
			case ExceptionHandlerType.Catch:
				return string.Format ("{0} {1}", type, handler.CatchType.FullName);
			case ExceptionHandlerType.Filter:
				throw new NotImplementedException ();
			default:
				return type;
			}
		}

		public static string ToInvariantCultureString (object value)
		{
			var convertible = value as IConvertible;
			return (null != convertible)
				? convertible.ToString (System.Globalization.CultureInfo.InvariantCulture)
				: value.ToString ();
		}
	}
}
