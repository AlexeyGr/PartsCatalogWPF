using MvvmDialogs;
using PartsCatalog.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.ViewModel
{
	public abstract class ModalDialogViewModelBase : NotifyPropertyChangedBase, IModalDialogViewModel
	{
		private bool? dialogResult;

		public bool? DialogResult
		{
			get => dialogResult;
			set
			{
				dialogResult = value;
				RaisePropertyChanged();
			}
		}
	}
}
