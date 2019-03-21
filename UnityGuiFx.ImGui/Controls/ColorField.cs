using System;
using UnityEditor;
using UnityEngine;

namespace ImGuiFx.Controls {
	public class ColorField : Control, IControl {
		public Color Color { get; set; }

		public event Action<ColorField> Changed;

		public void Render() {
			var color = EditorGUILayout.ColorField(Color, WidthAndHeight);
			if (color == Color)
				return;
			Color = color;
			if (Changed != null)
				Changed(this);
		}
	}
}