#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImGuiFx.Controls {
	//https://docs.unity3d.com/ScriptReference/EditorGUILayout.ObjectField.html
	public class ObjectField : Control, IControl {
		public ObjectField() {
			Type = typeof(UnityEngine.Object);
		}
		public delegate void ObjectFieldChange(ObjectField sender, UnityEngine.Object oldObject);

		public event ObjectFieldChange Changed;

		public Type Type { get; set; }
		public UnityEngine.Object Object { get; set; }
		public string Label { get; set; }
		public bool AllowSceneObjects { get; set; }

		public void Render() {
			var oldObject = Object;
			Object newObj;
			try {
				newObj = EditorGUILayout.ObjectField(Label ?? "", Object, Type, AllowSceneObjects, WidthAndHeight);
			}
			// https://answers.unity.com/questions/385235/editorguilayoutcolorfield-inside-guilayoutwindow-c.html
			catch (ExitGUIException) {
				return;
			}
			Object = newObj;
			if (Changed != null && newObj != oldObject)
				Changed(this, oldObject);
		}
	}

	//https://docs.unity3d.com/ScriptReference/EditorGUILayout.ObjectField.html
	public class ObjectField<T> : ObjectField where T : UnityEngine.Object{
		public ObjectField() {
			Type = typeof(T);
			base.Changed += OnChanged;
		}

		private void OnChanged(ObjectField sender, object o) {
			if (Changed != null)
				Changed(this, (T) o);
		}

		public new T Object {
			get { return (T) base.Object; }
			set { base.Object = value; }
		}

		public new delegate void ObjectFieldChange(ObjectField<T> sender, T oldObject);

		public new event ObjectFieldChange Changed;
	}
}
#endif