using System;
using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.TextField.html
	public class TextField : TextBase {
		public TextField(){
			Style = GUI.skin.textField;
		}

		protected override string RenderText(){
			return GUILayout.TextField(Text ?? "", MaxLength, Style, GuiLayoutOptions);
		}
	}
}