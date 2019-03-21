using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Layouts {
	//https://docs.unity3d.com/ScriptReference/GUILayout.BeginHorizontal.html
	//https://docs.unity3d.com/ScriptReference/GUILayout.EndHorizontal.html
	public class HorizontalLayout : Control, ILayout {
		public Color? Color { get; set; }
		private Color _oldColor;

		public void LayoutBegin() {
			if (Color != null) {
				_oldColor = GUI.color;
				GUI.color = (Color) Color;
			}
			GUILayout.BeginHorizontal(GuiLayoutOptions);
		}

		public void LayoutEnd() {
			if (Color != null)
				GUI.color = _oldColor;
			GUILayout.EndHorizontal();
		}
	}
}