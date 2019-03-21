using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImGuiFx {
	public class DispatcherOnGUI : DispatcherOnGUI.IInvoker {
		public interface IInvoker {
			void InvokeOnStart(Action func);
			void InvokeOnEnd(Action func);
		}

		// todo add editor events?

		private readonly Queue<Action> _startMethods = new Queue<Action>(100);
		private readonly Queue<Action> _endMethods = new Queue<Action>(100);

		void IInvoker.InvokeOnStart(Action func) {
			_startMethods.Enqueue(func);
		}

		void IInvoker.InvokeOnEnd(Action func) {
			_endMethods.Enqueue(func);
		}

		public void DispatchAllStart() {
			DispatchAll(_startMethods);
		}

		public void DispatchAllEnd() {
			DispatchAll(_endMethods);
		}

		private static void DispatchAll(Queue<Action> actions) {
			while (actions.Count > 0) {
				try {
					actions.Dequeue()();
				}
				catch (Exception e) {
                    if (System.Diagnostics.Debugger.IsAttached)
                        throw;
					Debug.LogError(e);
				}
			}
		}
	}
}