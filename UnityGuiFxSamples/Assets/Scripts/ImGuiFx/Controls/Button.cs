using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ImGuiFx.Controls{
	//https://docs.unity3d.com/ScriptReference/GUILayout.Button.html
	public class Button : Control, IControl{

		public Button(){
			GuiStyle = new GUIStyle(GUI.skin.button);
			Color = Color.white;
		}

		public Color Color { get; set; }
		public GUIStyle GuiStyle { get; set; }
		public object Content { get; set; }
		public object Data { get; set; }

		public event Action<Button> Click;

		private void OnClick(){
			if (Click != null)
				Click(this);
		}

		public void Render(){
			var enumator = TryRender();
			while (enumator.MoveNext() && enumator.Current == false) { }
        }

        private IEnumerator<bool> TryRender(){
            if (Content == null)
                yield return TryRender<string>("", GUILayout.Button);

            yield return TryRender<string>(Content, GUILayout.Button);
            yield return TryRender<Texture2D>(Content, GUILayout.Button);
            yield return TryRender<Texture>(Content, GUILayout.Button);
			yield return TryRender<string>(Content + "", GUILayout.Button);
		}

        private delegate bool UnityButton<in T>(T content, GUIStyle style, params GUILayoutOption[] options) where T : class;

		private bool TryRender<T>(object content, UnityButton<T> func) where T:class{
			var contentAsT = content as T;
			if (contentAsT == null) return false;

			var oldColor = GUI.color;
			GUI.color = Color;
			if (func(
				contentAsT,
				GuiStyle,
				GuiLayoutOptions
			) == false){
				GUI.color = oldColor;
				return true;
			}
			GUI.color = oldColor;
			OnClick();
            return true;
		}
	}
}