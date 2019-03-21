using System;
using UnityEngine;

namespace Assets.Scripts.Editors.DesignerUi{
	[ExecuteInEditMode]
	public class Delay : MonoBehaviour {
		public static Delay DelayForSeconds(float sec, Action func) {
			var obj = new GameObject("Delay");
			var delayer = obj.AddComponent<Delay>();
			delayer._seconds = sec;
			delayer._function = func;
			return delayer;
		}

		private DateTime _startTime;
		private Action _function;
		private float _seconds;

		private void Awake(){
			_startTime = DateTime.Now;
		}

		public event Action OnDead;

		public void UpdatePriv(){
			if ((DateTime.Now - _startTime).Seconds > _seconds) {
				_function();
				if (OnDead != null)
					OnDead();
				try{
					DestroyImmediate(gameObject);
				}
				finally{
				}
			}
		}
	}
}