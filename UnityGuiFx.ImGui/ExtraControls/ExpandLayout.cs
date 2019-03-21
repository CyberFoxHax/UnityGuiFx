using System.Collections.Generic;
using ImGuiFx.Controls;
using ImGuiFx.Layouts;

namespace ImGuiFx.ExtraControls{
	public class ExpandLayout : Control{
		public string Label {
			get { return _label; }
			set {
				if (_label != null)
					_labelControl.Content = value;
				_label = value;
			}
		}

		public bool IsOpen { get; set; }

		private VerticalLayout _innerLayout;
		private Label _labelControl;
		private Button _buttonExpand;
		private string _label;

		private void SetState(bool isOpen) {
			_innerLayout.Hidden = !isOpen;
			_buttonExpand.Content = isOpen ? "<" : ">";
		}

		public new List<Control> Children {
			get { return (_innerLayout ?? (_innerLayout = new VerticalLayout())).Children; }
		}

		public override IEnumerable<Control> OnInitialize(){
			yield return new HorizontalLayout{ Children = {
				(_buttonExpand = new Button {
					Width = 17,
					Height = 15,
					Content = ">"
				}),
				(_labelControl = new Label {
					Height = 15,
					Content = Label
				})
			}};
			_buttonExpand.Click += p => {
				SetState(IsOpen = !IsOpen);
			};
			_innerLayout = _innerLayout  ?? new VerticalLayout();
			_innerLayout.Hidden = true;
			if (IsOpen) {
				SetState(IsOpen);
			}
			yield return _innerLayout;
		}
	}
}