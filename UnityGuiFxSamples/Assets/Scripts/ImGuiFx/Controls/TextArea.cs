using System;
using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.TextArea.html
	public class TextArea : TextBase {
		public TextArea(){
			Style = GUI.skin.textArea;
		}

		protected override string RenderText(){
			return GUILayout.TextArea(Text ?? "", MaxLength, Style, GuiLayoutOptions);
		}
	}
}