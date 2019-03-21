using System;
using Assets.Scripts.DataModels.Database;
using Assets.Scripts.ImGuiFx;
using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.ImGuiFx.Layouts;

namespace Assets.Scripts.Editors.UploadUi{
	public class TagField : HorizontalLayout {
		public TagField() {
			Children.AddRange(new Control[]{
				_toggle,
				_name,
				_chineseName,
				_deleteBtn
			});
			_deleteBtn.Click += DeleteBtnOnClick;
			_name.Changed += NameOnChanged;
			_chineseName.Changed += NameOnChanged;
			OnModelChange(null);
		}

		private void NameOnChanged(TextBase sender, string s){
			if (string.IsNullOrEmpty(_name.Text) || string.IsNullOrEmpty(_chineseName.Text))
				return;
			if (Changed != null)
				Changed(this);
			if (_tag == null)
				return;
			_tag.Name = _name.Text;
			_tag.ChineseName = _chineseName.Text;
		}

		private void DeleteBtnOnClick(Button button) {
			Delete = !Delete;
		}

		private readonly Toggle _toggle = new Toggle { Width = 10 };
		private readonly TextField _name = new TextField { Width = 100 };
		private readonly TextField _chineseName = new TextField { Width = 100 };
		private readonly Button _deleteBtn = new Button { Content = "X", Width = 20, Color = UnityEngine.Color.yellow };

		public event Action<TagField> Changed;

		public bool Delete {
			get { return _delete; }
			private set {
				if (value)
					Color = UnityEngine.Color.red;
				else
					Color = null;

				_delete = value;
			}
		}

		private void OnModelChange(MaterialTag tag){
			if (tag == null){
				_toggle.IsChecked = false;
				_deleteBtn.Hidden = true;
				return;
			}
			_deleteBtn.Hidden = false;

			_toggle.IsChecked = false;
			_name.Text = tag.Name;
			_chineseName.Text = tag.ChineseName;
		}

		private MaterialTag _tag;
		private bool _delete;

		public MaterialTag Tag {
			get { return _tag; }
			set {
				if (_tag != value)
					OnModelChange(value);
				_tag = value;
			}
		}

		public bool IsChecked{
			get { return _toggle.IsChecked; }
			set { _toggle.IsChecked = value; }
		}

		public void SetTagNoEvent(MaterialTag value) {
			_deleteBtn.Hidden = false;
			_tag = value;
		}
	}
}