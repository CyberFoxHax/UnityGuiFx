using System.Collections.Generic;
using Assets.Scripts.ImGuiFx;
using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.ImGuiFx.Layouts;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editors.DesignerUi {
	
	public partial class DesignerUiMain : EditorWindowHelper{

		[MenuItem("IdMei/DesignerUI")]
		public static void ShowWindow() {
			StaticStart<DesignerUiMain>();
		}

		private Label _sunRotationLabel;
		private HorizontalSlider _sunRotationSlider;
		private Label _sunHeightLabel;
		private HorizontalSlider _sunHeightSlider;
		private HorizontalLayout _skyboxesContainer;
		private HorizontalLayout _interiorsContainer;
		private Button _unselectEverythingButton;
		private ObjectField<Transform> _monitoringObjectField;
		private Button _centerButton;

		protected override IEnumerable<Control> OnInitialize(){
			Title = "DesignerUI";
			yield return new VerticalLayout {
				Children = {
					new VerticalLayout{
						Size = new Vector2(500, float.NaN),
						Children = {
							new HorizontalLayout{
								Children = {
									new Label{
										Content = "Sun Rotation (",
									},
									(_sunRotationLabel = new Label{
										Content = "123",
									}),
									new Label{
										Content = ")",
									},
									(_sunRotationSlider = new HorizontalSlider{
										Min = 0,
										Max = 540,
										Size = new Vector2(500, float.NaN)
									})
								}
							},
							new HorizontalLayout{
								Children = {
									new Label{
										Content = "SunHeight (",
									},
									(_sunHeightLabel = new Label{
										Content = "123",
									}),
									new Label{
										Content = ")",
									},
									(_sunHeightSlider = new HorizontalSlider{
										Min = 0,
										Max = 90,
										Size = new Vector2(500, float.NaN)
									}),
								}
							},
							new HorizontalLayout{
								Children = {
									(_monitoringObjectField = new ObjectField<Transform>{
										AllowSceneObjects = true,
										Label = "Monitoring Object",
									}),
									(_centerButton = new Button{
										Content = "CenterObject",
										Hidden = true
									})
								},
							}
						},
					},
					(_skyboxesContainer = new HorizontalLayout()),
					(_interiorsContainer = new HorizontalLayout()),
					(_unselectEverythingButton = new Button{
						Content = "Unselect everything",
						Size = new Vector2(200, float.NaN)
					}),
				}
			};
		}
	}
}
