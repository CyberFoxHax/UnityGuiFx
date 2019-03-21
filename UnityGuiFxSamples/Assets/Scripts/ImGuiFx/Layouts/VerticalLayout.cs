using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Layouts{
	//https://docs.unity3d.com/ScriptReference/GUILayout.BeginVertical.html
	//https://docs.unity3d.com/ScriptReference/GUILayout.EndVertical.html
	public class VerticalLayout : Control, ILayout {
		public Color Color { get; set; }
		private Color _oldColor;

		public void LayoutBegin() {
			if(Color != default(Color)){
				_oldColor = GUI.color;
				GUI.color = Color;
			}
			GUILayout.BeginVertical(GuiLayoutOptions);
		}

		public void LayoutEnd() {
			if(Color != default(Color))
				GUI.color = _oldColor;
			GUILayout.EndVertical();
		}
	}
}