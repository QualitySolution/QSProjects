﻿using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using Gamma.Utilities;
using System.Linq.Expressions;

namespace QS.Commands
{
	public abstract class PropertySubscribedCommandBase : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public abstract bool CanExecute(object parameter);

		public abstract void Execute(object parameter);

		private List<object[]> updatedSets = new List<object[]>();

		public void CanExecuteChangedWith<TUpdatedSubject>(TUpdatedSubject updatedSubject, params Expression<Func<TUpdatedSubject, object>>[] propertySelectors)
			where TUpdatedSubject : INotifyPropertyChanged
		{
			var updatedPropertiesNames = propertySelectors.Select(PropertyUtil.GetName).ToArray();

			var foundSet = updatedSets.FirstOrDefault(x => object.ReferenceEquals(x[0], updatedSubject));
			if(foundSet != null) {
				foundSet[0] = updatedSubject;
				foundSet[1] = updatedPropertiesNames;
			} else {
				updatedSets.Add(new object[2] { updatedSubject, updatedPropertiesNames });
			}
			updatedSubject.PropertyChanged -= ViewModel_PropertyChanged;
			updatedSubject.PropertyChanged += ViewModel_PropertyChanged;
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var foundSet = updatedSets.FirstOrDefault(x => object.ReferenceEquals(x[0], sender));
			if(((string[])foundSet[1]).Contains(e.PropertyName)) {
				RaiseCanExecuteChanged();
			}
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
