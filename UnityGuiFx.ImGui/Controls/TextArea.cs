using UnityEngine;

namespace ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.TextArea.html
	public class TextArea : TextBase {
		protected override void OnLoaded() {
			if(Style == null)
				Style = GUI.skin.textArea;
			base.OnLoaded();
		}

		protected override string RenderText(){
#if UNITY_EDITOR
			var text = Text;
			text = UnityEditor.EditorGUILayout.TextArea(text ?? "", Style, WidthAndHeight);
			if (MaxLength > -1 && text.Length > MaxLength)
				text = text.Substring(0, MaxLength);
			return text;
#else
			return GUILayout.TextArea(Text ?? "", MaxLength, Style, WidthAndHeight);
#endif
		}
	}
}