using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataModels;
using Assets.Scripts.DataModels.Database;
using Assets.Scripts.ImGuiFx.Controls;
using Assets.Scripts.ImGuiFx.Layouts;
using Assets.Scripts.Lib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Editors.UploadUi {
	class TagsUi : VerticalLayout {
		private static readonly string TagsUrl = ServerUrl.Url + "Tags";

		private List<MaterialTag> _materialTagsDb;
		private VerticalLayout _tagsWrapper;

		public IEnumerable<MaterialTag> CheckedTags{
			get{
				return _tagsWrapper.Children.OfType<TagField>().Where(p => p.Tag != null && p.IsChecked).Select(p => p.Tag);
			}
		}

		public IEnumerable<MaterialTag> AllTags{
			get{
				return _tagsWrapper.Children.OfType<TagField>().Where(p => p.Tag != null).Select(p => p.Tag);
			}
		}

		public override void OnLoaded(){
			Children.Add(_tagsWrapper = new VerticalLayout());

			var req = UnityWebRequest.Get(TagsUrl);
			req.downloadHandler = new DownloadHandlerBuffer();
			req.Send();
			while (req.error == null && req.downloadHandler.isDone == false) {

			}
			var responseText = req.downloadHandler.text;

			if (string.IsNullOrEmpty(responseText) == false) {
				var tags = Json.Deserialize<List<MaterialTag>>(responseText);

				_tagsWrapper.Children.Clear();
				foreach (var materialTag in tags) {
					_tagsWrapper.Children.Add(new TagField {
						Tag = materialTag
					});
				}

				_materialTagsDb = tags.Select(p => new MaterialTag { Id = p.Id, Name = p.Name, ChineseName = p.ChineseName }).ToList();

				var newTagField = new TagField();
				newTagField.Changed += NewTagFieldOnChanged;
				_tagsWrapper.Children.Add(newTagField);
			}

			var submitBtn = new Button{
				Content = "Submit"
			};
			submitBtn.Click += SaveTagsButtonOnClick;
			Children.Add(submitBtn);
		}

		private void SaveTagsButtonOnClick(Button obj){
			SubmitTags();
		}

		private class TagsResponse : JesusWebRequest.ResponseData {
			public int[] NewIds { get; set; }
		}

		private void NewTagFieldOnChanged(TagField tagField) {
			tagField.SetTagNoEvent(new MaterialTag());
			tagField.Changed -= NewTagFieldOnChanged;
			var newTagField = new TagField();
			newTagField.Changed += NewTagFieldOnChanged;
			_tagsWrapper.Children.Add(newTagField);
		}

		private void SubmitTags() {
			var tags = _tagsWrapper.Children.OfType<TagField>().Where(p => p.Tag != null).ToArray();

			var newTags = tags.Where(p => p.Tag != null && p.Tag.Id == 0).Select(p => new { p.Tag.Name, p.Tag.ChineseName });
			var changedTags = (
				from tagElm in tags
				let tag = tagElm.Tag
				let dbTags = (
					from dbTag in _materialTagsDb
					where dbTag.Id == tag.Id
					&& (
						dbTag.Name != tag.Name || dbTag.ChineseName != tag.ChineseName
					)
					select dbTag
				)
				where tagElm.Tag != null
				&& tag.Id > 0
				&& tagElm.Delete == false
				&& dbTags.Any()
				select tag
			);
			var deletedTags = tags.Where(p => p.Tag != null && p.Delete).Select(p => p.Tag.Id);

			if (newTags.Any() == false && changedTags.Any() == false && deletedTags.Any() == false)
				return;

			var user = Login.GetGlobal();

			var req = JesusWebRequest.Post(TagsUrl, new {
				NewTags = newTags,
				ChangedTags = changedTags,
				DeletedTags = deletedTags,
				UserName = user.Username,
				Password = Crc32.Checksum(user.Password)
			});
			req.SendWait();
			var resp = req.GetResponse<TagsResponse>();

			if (resp.Error)
				Debug.LogError("Error in webserver response");


			var ids = resp.NewIds.GetEnumerator();
			foreach (var tag in tags.Where(p => p.Tag != null && p.Tag.Id == 0)){
				ids.MoveNext();
				if (ids.Current != null)
					tag.Tag.Id = (int)ids.Current;
			}

			_tagsWrapper.Children.RemoveAll(p => ((TagField)p).Delete);
			_materialTagsDb = tags.Select(p => p.Tag).Select(p => new MaterialTag { Id = p.Id, Name = p.Name, ChineseName = p.ChineseName }).ToList();
		}

	}
}
