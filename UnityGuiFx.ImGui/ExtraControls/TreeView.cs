using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ImGuiFx.Controls;
using ImGuiFx.Layouts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImGuiFx.ExtraControls {
	public class TreeView : Control {

		public interface ITreeColumn {
			int Depth { get; }
			bool IsOpen { get; set; }
		}

		public interface INotifyPropertyChanged {
			void PropertyChanged(PropertyInfo property);
		}

		public class TreeViewColumn : Control {
			public string Binding { get; set; }
			public string Title { get; set; }
			public bool Foldout { get; set; }
		}

		public class TreeViewTextFieldColumn : TreeViewColumn { }
		public class TreeViewToggleColumn : TreeViewColumn { }
		public class TreeViewObjectFieldColumn : TreeViewColumn { }
		public class TreeViewParseNumberColumn : TreeViewColumn { }

		public class TreeViewSliderColumn : TreeViewColumn {
			public float Min { get; set; }
			public float Max { get; set; }
		}

		public override IEnumerable<Control> OnInitialize() {
			_columns = Children.OfType<TreeViewColumn>().ToArray(); 
			Children.Clear();
			ReflectProperties();
			Render();
			return base.OnInitialize();
		}

		private TreeViewColumn[] _columns;
		private object[] _data;
		private Type _dataType;
		private PropertyInfo[] _properties;
		private readonly List<List<Control>> _rows = new List<List<Control>>();

		public void SetData<T>(IEnumerable<T> data) {
			_dataType = typeof(T);
			_data = data.Cast<object>().ToArray();

			// note: validating tree integrity
			if(typeof(ITreeColumn).IsAssignableFrom(_dataType)) {
				var currentDepth = 0;
				var index = 0;
				var treeDataList = _data.OfType<ITreeColumn>().ToArray();

				if (treeDataList.Any(p => p.Depth < 0))
					Debug.LogException(new Exception("Depth can't be negative"));

				foreach (var treeData in treeDataList) {
					index++;
					if (treeData.Depth - currentDepth > 1) {
						Debug.LogException(new Exception(
							"Depth tree is not property formatted at index: " + index + " for type \"" + _dataType + "\"" +
						    "\ngoing from Depth " + currentDepth + " to Depth " + treeData.Depth + " is not supported"));
					}
					currentDepth = treeData.Depth;
				}
			}

			if (_columns == null || _columns.Any() == false)
				return;

			ReflectProperties();
			Render();
		}

		private void ReflectProperties() {
			if (_dataType == null)
				return;
			var type = _dataType;
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			var selectedPropertyNames = _columns
				.Select(p => p.Binding)
				.ToArray();

			var selectedProperties = selectedPropertyNames
				.Select(p => new {
					Property = properties.FirstOrDefault(pp => pp.Name == p),
					Key = p
				})
				.ToArray();

			foreach (var nullProp in selectedProperties.Where(p => p.Property == null))
				Debug.LogError("Property: \"" + nullProp.Key + "\" was not found on type: \"" + type.Name + "\"");

			_properties = selectedProperties
				.Where(p => p.Property != null)
				.Select(p => p.Property)
				.ToArray();
		}

		public void Render() {
			if (_dataType == null)
				return;
			var root = new HorizontalLayout();
			_rows.Clear();

			var columnEnumerator = _columns.ToList().GetEnumerator();
			foreach (var propertyInfo in _properties) {
				columnEnumerator.MoveNext();
				var col = columnEnumerator.Current;
				if (columnEnumerator.Current == null)
					return;
				var column = new VerticalLayout();
				if(float.IsNaN(col.Width) == false)
				if (_data == null || _data.Any() == false)
					return;

				column.Width = col.Width;
				
				var header = new HorizontalLayout {
					Children = {
						new Label { Content = columnEnumerator.Current.Title ?? columnEnumerator.Current.Binding },
						new HorizontalLayout{ Style = VerticalBorderStyle ,Children = {
							new Label {
								Content = ""
							}
						}}
					},
					Style = HeaderStyle
				};
				column.Children.Add(header);
				var borderBottom = new HorizontalLayout {
					Children = {
						new Label { Content = " " }
					},
					Style = HorizontalBorderStyle,
				};
				column.Children.Add(borderBottom);
				root.Children.Add(column);

				var rows = new List<Control>();
				_rows.Add(rows);

				var rowIndex = 0;
				foreach (var row in _data) {
					var data = propertyInfo.GetValue(row, new object[0]);

					var cell = new HorizontalLayout {
						Style = rowIndex++ % 2 == 0 ? OddStyle : EvenStyle
					};

					rows.Add(cell);

					var fold = CreateTreeExtraColumn(propertyInfo, col, row, data);
					if (fold != null)
						cell.Children.Add(fold);

					var control = CreateBoundControl(propertyInfo, col, row, data);
					if (control != null)
						cell.Children.Add(control);

					column.Children.Add(cell);
				}
			}

			columnEnumerator.Dispose();

			Children.Clear();
			Children.Add(root);
		}

		private class PropertyBinding {
			public PropertyInfo PropertyInfo { get; set; }
			public object Context { get; set; }
			public Control Control { get; set; }
			public TreeViewColumn ColumnInformation { get; set; }
			public object Value {
				get { return PropertyInfo.GetValue(Context, new object[0]); }
			}

			public void OnChangeTyped(object value) {
				PropertyInfo.SetValue(Context, value, new object[0]);
				OnChanged();
			}

			public void OnChangeString(string value) {
				try {
					var parsedValue = TypeDescriptor.GetConverter(PropertyInfo.PropertyType).ConvertFromString(value);
					PropertyInfo.SetValue(Context, parsedValue, new object[0]);
					OnChanged();
				}
				catch (Exception e) {
					// note: supressing FormatException
					if (e.InnerException == null)
						throw;
					if (e.InnerException is FormatException == false)
						throw;
				}
			}

			private void OnChanged() {
				if (Changed != null)
					Changed(this);

				var notifier = Context as INotifyPropertyChanged;
				if(notifier != null)
					notifier.PropertyChanged(PropertyInfo);
			}

			public event Action<PropertyBinding> Changed;
		}

		private readonly Dictionary<object, List<PropertyBinding>> _bindings = new Dictionary<object, List<PropertyBinding>>();

		private void BoundControlOnChanged(PropertyBinding propertyBinding) {
			var relatedBindings = _bindings[propertyBinding.Context]
				.Where(p => p.PropertyInfo == propertyBinding.PropertyInfo && p != propertyBinding)
				.ToArray();

			foreach (var binding in relatedBindings) {
				var label = binding.Control as Label;
				if (label != null) {
					label.Content = binding.Value;
					continue;
				}

				var textField = binding.Control as TextField;
				if (textField != null) {
					textField.Text = binding.Value + "";
					continue;
				}

				var sliderBase = binding.Control as SliderBase;
				if (sliderBase != null) {
					sliderBase.Value = (float) binding.Value;
					continue;
				}

				var toggle = binding.Control as Toggle;
				if (toggle != null) {
					toggle.IsChecked = (bool) binding.Value;
					continue;
				}
			}
		}

		private Control CreateTreeExtraColumn(PropertyInfo propertyInfo, TreeViewColumn col, object context, object value) {
			var treeColumn = context as ITreeColumn; 
			if (treeColumn == null || col.Foldout == false)
				return null;

			var foldout = new Foldout {
				IsOpen = treeColumn.IsOpen,
				ToggleLabelClick = false
			};

			var horizontalLayout = new HorizontalLayout {
				Children = { foldout },
				Style = new GUIStyle {
					fixedWidth = 20,
					margin = new RectOffset(25 * treeColumn.Depth, 0, 0, 0)
				}
			};

			Action<int> hideRow = p => {
				var columns = _rows;
				foreach (var control in columns) {
					control.ElementAt(p).Hidden = foldout.IsOpen == false;
				}
			};

			Action fold = () => {
				var startRowIndex = _data.Select((p, i) => p == context ? i : -1).First(p => p > -1);
				var lowDepth = treeColumn.Depth;
				for (var i = startRowIndex + 1; i < _data.Length; i++) {
					var currentRow = (ITreeColumn)_data[i];
					if (currentRow.Depth <= lowDepth) {
						break;
					}
					hideRow(i);
				}
			};

			foldout.Changed += sender => fold();
			foldout.Loaded  += sender => fold();

			return horizontalLayout;
		}

		public class FoldoutBinding {
			public PropertyInfo PropertyInfo { get; set; }
			public object Context { get; set; }
			public Control Control { get; set; }
			public TreeViewColumn ColumnInformation { get; set; }
		}


		private Control CreateBoundControl(PropertyInfo propertyInfo, TreeViewColumn col, object context, object value) {
			var binder = new PropertyBinding {
				PropertyInfo = propertyInfo,
				Context = context,
				ColumnInformation = col,
			};

			binder.Changed += BoundControlOnChanged;
			binder.Control = null;

			if (_bindings.ContainsKey(context) == false)
				_bindings[context] = new List<PropertyBinding>();
			_bindings[context].Add(binder);

			Control control;
			if (col is TreeViewTextFieldColumn) {
				var textBox = new TextField {
					Text = value + ""
				};
				textBox.Changed += (sender, oldValue) =>
					binder.OnChangeString(sender.Text);

				control = textBox;
			}
			else if (col is TreeViewSliderColumn) {
				var sliderCol = (TreeViewSliderColumn)col;
				var slider = new HorizontalSlider {
					Value = (float)value,
					Min = sliderCol.Min,
					Max = sliderCol.Max
				};
				slider.Changed += p =>
					binder.OnChangeTyped(p.Value);

				control = slider;
			}
			else if (col is TreeViewToggleColumn) {
				var toggle = new Toggle {
					IsChecked = (bool)value
				};
				toggle.Changed += p =>
					binder.OnChangeTyped(p.IsChecked);

				control = toggle;
			}
			else if (col is TreeViewObjectFieldColumn) {
				var objectField = new ObjectField {
					Object = (Object) value
				};
				objectField.Changed += (sender, oldObject) =>
					sender.Object = oldObject;

				control = objectField;
			}
			else if (col is TreeViewParseNumberColumn) {
				throw new NotImplementedException();
			}
			else {
				if (value is Texture) {
					var image = new Image {
						Texture2D = (Texture2D) value,
						Width  = 20,
						Height = 20
					};
					control = image;
				}
				else {
					var text = new Label {
						Content = value
					};
					control = text;
				}
			}

			binder.Control = control;
			return control;
		}

		//////////// <Styles> //////////////
		private static readonly GUIStyle EvenStyle = new GUIStyle("OL EntryBackEven") {
			margin = new RectOffset(),
			stretchHeight = true,
			fixedHeight = 30,
			padding = new RectOffset(0, 0, 7, 0)
		};

		private static readonly GUIStyle OddStyle = new GUIStyle("OL EntryBackOdd") {
			margin = new RectOffset(),
			stretchHeight = true,
			fixedHeight = 30,
			padding = new RectOffset(0, 0, 7, 0)
		};

		private static readonly GUIStyle HeaderStyle = new GUIStyle("OL EntryBackOdd") {
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 5, 10),
			fixedHeight = 30
		};

		private static readonly GUIStyle HorizontalBorderStyle = new GUIStyle {
			fixedHeight = 1,
			stretchHeight = true,
			stretchWidth = true,
			normal = {
				background = CreateSolidColor(new Color32(120, 120, 120, 255))
			}
		};

		private static readonly GUIStyle VerticalBorderStyle = new GUIStyle {
			fixedWidth = 1,
			stretchHeight = true,
			stretchWidth = true,
			normal = {
				background = CreateSolidColor(new Color32(190, 190, 190, 255))
			}
		};

		private static Texture2D CreateSolidColor(Color32 color) {
			var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			tex.SetPixels32(new[] { color });
			tex.Apply();
			return tex;
		}
	}
}
