using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.Label.html
	public class Label : Control, IControl {
		public Label() {
			LabelStyle = new GUIStyle(GUI.skin.label);
		}

		public object Content { get; set; }

		public GUIStyle LabelStyle { get; set; }

		public bool RichText {
			get { return LabelStyle.richText; }
			set { LabelStyle.richText = value; }
		}

		public void Render(){
			var run = Run();
			while (run.MoveNext() && run.Current == false) { }
		}

		private IEnumerator<bool> Run(){
			yield return CastRun<string>(Content, GUILayout.Label);
			yield return CastRun<Texture>(Content, GUILayout.Label);
			yield return CastRun<GUIContent>(Content, GUILayout.Label);
			yield return CastRun<string>(Content + "", GUILayout.Label);
		}


		private bool CastRun<T>(object p, Action<T, GUIStyle, GUILayoutOption[]> f) where T : class{
			var tryCast = p as T;
			if (tryCast == null) return false;
			f(tryCast, LabelStyle, WidthAndHeight);
			return true;
		}
	}
}