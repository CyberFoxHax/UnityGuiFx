using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImGuiFx{
	public abstract class Control : IInternalInitializers{
		protected Control() {
			Size = new Vector2(float.NaN, float.NaN);
			Children = new List<Control>();
			Alive = true;
		}

		public bool Alive { get; private set; }

		~Control() {
			Alive = false;
		}

		public virtual List<Control> Children { get; protected set; }
		public bool Disabled { get; set; }
		public bool Hidden { get; set; }
		public bool IsInitialized { get; set; }
		public bool IsLoaded { get; set; }
		public DispatcherOnGUI.IInvoker Dispatcher { get; set; }

		public event Action<Control> Loaded; 

		protected GUILayoutOption[] WidthAndHeight { get; private set; }

		private Vector2 _size;
		public Vector2 Size{
			get { return _size; }
			set{
				if (Vector2IsEqual(_size, value) == false)
					OnSizeChanged(value);
				_size = value;
			}
		}

		public float Width {
			get { return _size.x; }
			set{
				if (_size.x != value)
					OnSizeChanged(new Vector2(value, _size.y));
				_size.x = value;
			}
		}

		public float Height {
			get { return _size.y; }
			set{
				if (_size.y != value)
					OnSizeChanged(new Vector2(_size.x, value));
				_size.y = value;
			}
		}

		protected virtual void OnSizeChanged(Vector2 newSize){
			var size = 0;
			var xNan = float.IsNaN(newSize.x);
			var yNan = float.IsNaN(newSize.y);
			if (xNan == false) size++;
			if (yNan == false) size++;
			var arr = WidthAndHeight != null && WidthAndHeight.Length == size
				? WidthAndHeight
				: new GUILayoutOption[size];
			if (xNan == false) arr[--size] = GUILayout.Width(newSize.x);
			if (yNan == false) arr[--size] = GUILayout.Height(newSize.y);
			WidthAndHeight = arr;
		}

		private static bool Vector2IsEqual(Vector2 a, Vector2 b){
			var xEqual = float.IsNaN(a.x) && float.IsNaN(b.x) || Mathf.Abs(a.x - b.x) > 0.01f;
			var yEqual = float.IsNaN(a.y) && float.IsNaN(b.y) || Mathf.Abs(a.y - b.y) > 0.01f;
			return xEqual && yEqual;
		}

		protected void Initialize(){
			Children.AddRange(OnInitialize());
			IsInitialized = true;
		}

		void IInternalInitializers.Initialize() {
			Initialize();
		}

		void IInternalInitializers.OnLoaded() {
			OnLoaded();
		}

		protected virtual void OnLoaded() {
			IsLoaded = true;
			if (Loaded != null)
				Loaded(this);
		}

		public virtual IEnumerable<Control> OnInitialize(){
			yield break;
		}
	}
}