using System;
using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls{
	public abstract class BaseSlider : Control, IControl {
		protected BaseSlider(){
			Min = 0;
			Min = 100;
			Value = 0;
			Snap = 0;
		}

		public float Min { get; set; }
		public float Max { get; set; }
		public float Value { get; set; }
		public float Snap { get; set; }

		public GUIStyle SliderStyle { get; set; }
		public GUIStyle ThumbStyle { get; set; }

		public event Action<BaseSlider> Change;

		public void Render() {
			var newValue = RenderSlider(Value, Min, Max, SliderStyle, ThumbStyle, GuiLayoutOptions);
			if(Math.Abs(Snap) > 0.001f)
				newValue = Mathf.Round(newValue / Snap) * Snap;
			Value = newValue;
			if (Change != null && Math.Abs(newValue - Value) > 0.001f)
				Change(this);
		}

		protected abstract float RenderSlider(float value, float min, float max, GUIStyle sliderStyle, GUIStyle thumbStyle, GUILayoutOption[] options);
	}
}