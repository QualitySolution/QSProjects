using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using QS.Utilities.Enums;

namespace QS.Controls;

/// <summary>
/// Отображает значения перечисления в виде списка флажков.
/// </summary>
public partial class EnumCheckList : UserControl {
	public static readonly StyledProperty<Type?> EnumTypeProperty =
		AvaloniaProperty.Register<EnumCheckList, Type?>(nameof(EnumType));

	public static readonly StyledProperty<IList?> SelectedValuesProperty =
		AvaloniaProperty.Register<EnumCheckList, IList?>(
			nameof(SelectedValues),
			defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
		);

	public static readonly StyledProperty<bool> ExcludeZeroValueProperty =
		AvaloniaProperty.Register<EnumCheckList, bool>(nameof(ExcludeZeroValue));

	private bool isUpdatingSelection;

	public EnumCheckList() {
		InitializeComponent();
	}

	public Type? EnumType {
		get => GetValue(EnumTypeProperty);
		set => SetValue(EnumTypeProperty, value);
	}

	/// <summary>
	/// Выбранные значения. Для двусторонней привязки контрол создаёт типизированный
	/// <see cref="List{T}"/>, где T — тип из <see cref="EnumType"/>.
	/// </summary>
	public IList? SelectedValues {
		get => GetValue(SelectedValuesProperty);
		set => SetValue(SelectedValuesProperty, value);
	}

	/// <summary>Не показывать нулевое значение перечисления (обычно None).</summary>
	public bool ExcludeZeroValue {
		get => GetValue(ExcludeZeroValueProperty);
		set => SetValue(ExcludeZeroValueProperty, value);
	}

	public ObservableCollection<EnumCheckListItem> Items { get; } = new ObservableCollection<EnumCheckListItem>();

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
		base.OnPropertyChanged(change);

		if(change.Property == EnumTypeProperty || change.Property == ExcludeZeroValueProperty)
			RebuildItems();
		else if(change.Property == SelectedValuesProperty)
			UpdateSelectionFromValue();
	}

	private void RebuildItems() {
		foreach(var item in Items)
			item.PropertyChanged -= OnItemPropertyChanged;

		Items.Clear();
		if(EnumType?.IsEnum != true)
			return;

		foreach(Enum value in Enum.GetValues(EnumType)) {
			if(ExcludeZeroValue && Convert.ToDecimal(value) == 0)
				continue;

			var item = new EnumCheckListItem(value, value.GetEnumTitle());
			item.PropertyChanged += OnItemPropertyChanged;
			Items.Add(item);
		}

		UpdateSelectionFromValue();
	}

	private void UpdateSelectionFromValue() {
		if(isUpdatingSelection)
			return;

		isUpdatingSelection = true;
		try {
			foreach(var item in Items)
				item.IsSelected = SelectedValues?.Cast<object>().Any(value => Equals(value, item.Value)) == true;
		}
		finally {
			isUpdatingSelection = false;
		}
	}

	private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if(isUpdatingSelection || e.PropertyName != nameof(EnumCheckListItem.IsSelected) || EnumType == null)
			return;

		var selectedValues = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(EnumType))!;
		foreach(var item in Items.Where(item => item.IsSelected))
			selectedValues.Add(item.Value);

		SetCurrentValue(SelectedValuesProperty, selectedValues);
	}
}

public sealed class EnumCheckListItem : INotifyPropertyChanged {
	private bool isSelected;

	public EnumCheckListItem(Enum value, string title) {
		Value = value;
		Title = title;
	}

	public Enum Value { get; }
	public string Title { get; }

	public bool IsSelected {
		get => isSelected;
		set {
			if(isSelected == value)
				return;

			isSelected = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}
