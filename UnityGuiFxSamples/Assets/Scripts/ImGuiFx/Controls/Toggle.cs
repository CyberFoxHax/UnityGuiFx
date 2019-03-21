using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.Toggle.html
	public class Toggle : Control, IControl {
		public Toggle(){
			Style = new GUIStyle(GUI.skin.toggle);
		}

		public bool IsChecked { get; set; }
		public object Content { get; set; }

		public GUIStyle Style { get; set; }

		public event Action<Toggle> Checked;
		public event Action<Toggle> Unchecked;
		public event Action<Toggle> Changed;

		public void Render(){
			var enumator = TryRender();
			while (enumator.MoveNext() && enumator.Current == false) { }
		}

		private IEnumerator<bool> TryRender() {
			if (Content == null){
				yield return TryRender<string>(null, GUILayout.Toggle);
			}

			yield return TryRender<string>		(Content, GUILayout.Toggle);
			yield return TryRender<Texture>		(Content, GUILayout.Toggle);
			yield return TryRender<GUIContent>	(Content, GUILayout.Toggle);
			yield return TryRender<string>		(Content+"", GUILayout.Toggle);
		}

		private delegate bool UnityButton<in T>(bool value, T content, GUIStyle style, params GUILayoutOption[] options) where T : class;

		private bool TryRender<T>(object content, UnityButton<T> func) where T : class {
			var contentAsT = content as T;
			if (contentAsT == null) return false;

			var isChecked = func(
				IsChecked,
				contentAsT,
				Style,
				GuiLayoutOptions
			);

			if (isChecked == IsChecked) return true;
			IsChecked = isChecked;

			if (Checked != null && isChecked)
				Checked(this);
			else if (Unchecked != null && isChecked == false)
				Unchecked(this);

			if (Changed != null)
				Changed(this);

			return true;
		}
	}
}