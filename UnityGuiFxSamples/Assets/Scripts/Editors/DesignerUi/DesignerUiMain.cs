using System;
using System.Linq;
using Assets.Scripts.DataModels;
using Assets.Scripts.ImGuiFx.Controls;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Editors.DesignerUi {
	public partial class DesignerUiMain{
		public static Skyboxes Skyboxes;

		private Skyboxes _skies;
		private float _offsetSun;

		private static Light GetSun(){
			return FindObjectsOfType<Light>().FirstOrDefault(p => p.type == LightType.Directional);
		}

		protected override void OnLoaded(){
			foreach (var junk in FindObjectsOfType<Delay>())
				DestroyImmediate(junk.gameObject);
			var globals = GlobalBindings.GetGlobals();
			_skies = globals.Skyboxes;
			foreach (var skybox in globals.Skyboxes.Skies){
				var newButton = new Button{
					Content = AssetPreview.GetAssetPreview(skybox.SkyboxMaterial),
					Size = new Vector2(50, 50),
					Data = skybox,
				};
				newButton.Click += SkyboxOnClick;
				_skyboxesContainer.Children.Add(newButton);
			}

			foreach (var interior in globals.Interiors) {
				var newButton = new Button {
					Content = interior.Thumbnail,
					Size = new Vector2(100, 100),
					Data = interior,
				};
				newButton.Click += InteriorOnClick;
				_interiorsContainer.Children.Add(newButton);
			}

			_sunRotationSlider.Change += SunRotationSliderOnChange;
			_sunHeightSlider.Change += SunHeightSliderOnChange;
			var light = GetSun();
			_sunRotationSlider.Value = light.transform.eulerAngles.y;
			_sunRotationLabel.Content = light.transform.eulerAngles.y;
			_sunHeightLabel.Content = light.transform.eulerAngles.x;
			_sunHeightSlider.Value = light.transform.eulerAngles.x;

			_unselectEverythingButton.Click += p => Selection.objects = new Object[0];

			_monitoringObjectField.Change += MonitoringObjectFieldOnChange;
			_centerButton.Click += CenterButtonOnClick;
		}

		private static void CenterObject(Transform obj){
			var roomController = FindObjectOfType<RoomController>();
			if (roomController == null || obj == null) return;
			obj.position = roomController.RoomCenter.position;
		}

		private void CenterButtonOnClick(Button button){
			CenterObject(_monitoringObjectField.Object);
		}

		private void MonitoringObjectFieldOnChange(ObjectField<Transform> objectField, Transform transform){
			if (objectField.Object == null){
				_centerButton.Hidden = true;
				return;
			}
			CenterObject(_monitoringObjectField.Object);
			FrameObject(_monitoringObjectField.Object);
			_centerButton.Hidden = false;
		}

		private void Update(){
			if (UpdateEvt != null)
				UpdateEvt();
		}

		private event Action UpdateEvt;

		private void OnHierarchyChange(){
			if (Controls == null) return;
			GameObject mesh;
			GameObject reflectionProbe;
			try{
				mesh = FindObjectsOfType<MeshFilter>().First(p => p.transform.parent == null).gameObject;
				reflectionProbe = FindObjectsOfType<ReflectionProbe>().First(p => p.transform.parent == null).gameObject;
			}
			catch{
				mesh = null;
				reflectionProbe = null;
			}
			var excludedItems = new[]{
				GetSun().gameObject,
				Camera.main.gameObject,
				reflectionProbe,
				mesh
			}.Where(p=>p!=null).Select(p=>p.transform);

			excludedItems = excludedItems.Except(FindObjectsOfType<Delay>().Select(p => p.transform));

			if (_monitoringObjectField.Object != null) return;
			var newItems = FindObjectsOfType<Transform>().Except(excludedItems);
			var roomController = FindObjectOfType<RoomController>();
			if (roomController != null)
				newItems = newItems.Except(new[]{roomController.transform});

			_monitoringObjectField.Object = newItems.FirstOrDefault(p => p.transform.parent == null);

			_monitoringObjectField.Object = _monitoringObjectField.Object;
			_centerButton.Hidden = _monitoringObjectField.Object == null;

			if (_monitoringObjectField.Object == null) return;

			Repaint();

			for (var i = 0; i < 5; i++){
				var delay = Delay.DelayForSeconds(0.5f * i, () =>{
					CenterObject(_monitoringObjectField.Object);
					FrameObject(_monitoringObjectField.Object);
				});
				UpdateEvt += delay.UpdatePriv;
				delay.OnDead += () => UpdateEvt -= delay.UpdatePriv;
			}
		}

		private void InteriorOnClick(Button obj){
			var data = (Interior)obj.Data;

			var res = FindObjectOfType<RoomController>();
			if (res != null)
				DestroyImmediate(res.gameObject);


			var instance = (GameObject)PrefabUtility.InstantiatePrefab(data.Prefab);
			var roomController = instance.GetComponent<RoomController>();

			var pos = -roomController.RoomCenter.position;
			pos.y = 0;
			instance.transform.localPosition = pos;
			instance.transform.rotation = Quaternion.identity;
			instance.transform.localScale = Vector3.one;

			var reflectionProbe = FindObjectOfType<ReflectionProbe>();
			if (reflectionProbe != null)
				reflectionProbe.RenderProbe();

			if (_monitoringObjectField.Object != null) {
				_monitoringObjectField.Object.transform.position = roomController.RoomCenter.position;
				FrameObject(_monitoringObjectField.Object.gameObject);
			}
			else
				FrameObject(roomController.RoomCenter.gameObject);
		}

		private static void FrameObject(Object obj){
			var savedSelection = Selection.objects;
			Selection.objects = new[] { obj };
			SceneView.FrameLastActiveSceneView();
			if (SceneView.currentDrawingSceneView != null)
				SceneView.currentDrawingSceneView.FrameSelected();
			Selection.objects = savedSelection;
		}


		private void SunHeightSliderOnChange(BaseSlider slider){
			var light = GetSun();
			var sunEuler = light.transform.eulerAngles;
			sunEuler.x = slider.Value;
			light.transform.eulerAngles = sunEuler;
			_sunHeightLabel.Content = Math.Round(slider.Value);
		}

		private void SunRotationSliderOnChange(BaseSlider slider){
			_sunRotationLabel.Content = Math.Round(slider.Value);
			var light = GetSun();
			var sunEuler = light.transform.eulerAngles;
			sunEuler.y = slider.Value;
			light.transform.eulerAngles = sunEuler;
			RenderSettings.skybox.SetFloat("_Rotation", -slider.Value + _offsetSun);
		}

		private void SkyboxOnClick(Button button){
			var data = (Skyboxes.SkyboxSetup)button.Data;
			RenderSettings.skybox = Instantiate(data.SkyboxMaterial);
			_offsetSun = data.Rotation.y;
			var light = GetSun();
			if (light == null) return;
			light.transform.eulerAngles = data.Rotation;
			light.color = data.IsNight ? _skies.DuskColor : _skies.DayColor;

			_sunRotationSlider.Value = light.transform.eulerAngles.y;
			_sunRotationLabel.Content = light.transform.eulerAngles.y;

			_sunHeightSlider.Value = light.transform.eulerAngles.x;
			_sunHeightLabel.Content = Math.Round(light.transform.eulerAngles.x);
		}
	}
}
