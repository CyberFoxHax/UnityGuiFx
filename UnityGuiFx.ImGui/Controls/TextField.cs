using UnityEngine;

namespace ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.TextField.html
	public class TextField : TextBase {
		protected override void OnLoaded() {
			if(Style == null)
				Style = GUI.skin.textField;
			base.OnLoaded();
		}

		protected override string RenderText(){
#if UNITY_EDITOR
			var text = Text;
			text = UnityEditor.EditorGUILayout.TextField(text ?? "", Style, WidthAndHeight);
			if (MaxLength > -1 && text.Length > MaxLength)
				text = text.Substring(0, MaxLength);
			return text;
#else
			return GUILayout.TextField(Text ?? "", MaxLength, Style, WidthAndHeight);
#endif
		}
	}
}