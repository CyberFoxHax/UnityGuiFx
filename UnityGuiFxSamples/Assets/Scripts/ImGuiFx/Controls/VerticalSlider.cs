using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.VerticalSlider.html
	public class VerticalSlider : BaseSlider {
		public VerticalSlider(){
			SliderStyle = new GUIStyle(GUI.skin.verticalSlider);
			ThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb);
		}

		protected override float RenderSlider(float value, float min, float max, GUIStyle sliderStyle, GUIStyle thumbStyle, GUILayoutOption[] options) {
			return GUILayout.VerticalSlider(Value, Min, Max, SliderStyle, ThumbStyle, options);
		}
	}
}