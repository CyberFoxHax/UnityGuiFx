using System;
using UnityEngine;

namespace ImGuiFx.Controls{
	public abstract class SliderBase : Control, IControl {
		protected SliderBase(){
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

		public event Action<SliderBase> Changed;

		public void Render() {
			var newValue = RenderSlider(Value, Min, Max, SliderStyle, ThumbStyle, WidthAndHeight);
			if(Math.Abs(Snap) > 0.001f)
				newValue = Mathf.Round(newValue / Snap) * Snap;
			if (Changed != null && Math.Abs(newValue - Value) > 0.001f)
				Changed(this);
			Value = newValue;
		}

		protected abstract float RenderSlider(float value, float min, float max, GUIStyle sliderStyle, GUIStyle thumbStyle, GUILayoutOption[] options);
	}
}