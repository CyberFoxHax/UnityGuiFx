using UnityEngine;

namespace ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.HorizontalSlider.html
	public class HorizontalSlider : SliderBase {
		protected override void OnLoaded() {
			SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
			ThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
			base.OnLoaded();
		}

		protected override float RenderSlider(float value, float min, float max, GUIStyle sliderStyle, GUIStyle thumbStyle, GUILayoutOption[] options) {
			return GUILayout.HorizontalSlider(Value, Min, Max, SliderStyle, ThumbStyle, options);
		}
	}
}