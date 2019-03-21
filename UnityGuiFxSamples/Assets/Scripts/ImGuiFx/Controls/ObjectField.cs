using System;
using UnityEditor;

namespace Assets.Scripts.ImGuiFx.Controls {
	//https://docs.unity3d.com/ScriptReference/EditorGUILayout.ObjectField.html
	public class ObjectField<T> : Control, IControl where T : UnityEngine.Object{

		public delegate void ObjectFieldChange(ObjectField<T> sender, T oldObject);

		public event ObjectFieldChange Change;

		private readonly Type _type = typeof(T);

		public T Object { get; set; }
		public string Label { get; set; }
		public bool AllowSceneObjects { get; set; }

		public void Render(){
			var oldObject = Object;
			var newObj = (T)EditorGUILayout.ObjectField(Label ?? "", Object, _type, AllowSceneObjects, GuiLayoutOptions);
			Object = newObj;
			if (Change != null && newObj != oldObject)
				Change(this, oldObject);
		}
	}
}
