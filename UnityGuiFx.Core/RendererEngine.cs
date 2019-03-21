using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImGuiFx {
	public class RendererEngine {
		private List<Control> _controls;

		public List<Control> Controls {
			get { return _controls; }
			set {
				if (_controls != null && _controls != value) {
					DisposeControls(_controls);
				}
				_controls = value;
			}
		}

		private void DisposeControls(IList<Control> controls) {
			for (var i = 0; i < controls.Count; i++) {
				var baseControl = controls[i];

				if (baseControl.Children.Any())
					DisposeControls(baseControl.Children);

				var control = baseControl as IDisposable;
				if (control != null) {
					try {
						control.Dispose();
					}
					catch (Exception e) {
						Debug.LogException(e);
					}
				}
			}
		}

		private readonly HashSet<Control> _allControls = new HashSet<Control>();
		private readonly HashSet<Control> _allOrphans = new HashSet<Control>();
		private readonly HashSet<Control> _allControlsLastRender = new HashSet<Control>();

		public DispatcherOnGUI Dispatcher { get; set; }

		public void RenderControls() {
			_allControlsLastRender.Clear();

			RenderControls(Controls);

			_allOrphans.Clear();

			foreach (var control in _allControls) {
				if (control.Alive == false)
					_allOrphans.Add(control);
			}
			foreach (var orphan in _allOrphans) {
				_allControls.Remove(orphan);

				var disposable = orphan as IDisposable;
				if(disposable != null)
					disposable.Dispose();
			}
		}

		private void RenderControls(IList<Control> controls){
			for (var i = 0; i < controls.Count; i++){
				var baseControl = controls[i];
				_allControls.Add(baseControl);
				_allControlsLastRender.Add(baseControl);
				baseControl.Dispatcher = Dispatcher;

				if (baseControl.IsInitialized == false) {
					((IInternalInitializers)baseControl).Initialize();
					baseControl.IsInitialized = true;
				}

				if (baseControl.IsLoaded == false) {
					((IInternalInitializers)baseControl).OnLoaded();
					baseControl.IsLoaded = true;
				}

				if (baseControl.Hidden) continue;
				if(baseControl.Disabled)
					GUI.enabled = false;

				var layout = baseControl as ILayout;
				if (layout != null)
					layout.LayoutBegin();

				if (baseControl.Children.Any())
					RenderControls(baseControl.Children);

				var control = baseControl as IControl;
				if (control != null) {
					try {
						control.Render();
					}
					catch (Exception e) {
						Debug.LogException(e);
					}
				}

				if (layout != null)
					layout.LayoutEnd();

				if (baseControl.Disabled)
					GUI.enabled = true;
			}
		}
	}
}
