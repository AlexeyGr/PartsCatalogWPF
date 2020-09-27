using PartsCatalog.Model;
using PartsCatalog.Model.Db;
using PartsCatalog.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PartsCatalog.ViewModel
{
	public class EditEntityViewModel<TEntity, TMapping> : ModalDialogViewModelBase
		where TEntity : class, IHierachicalEntity<TEntity, TMapping>, INotifyDataErrorInfo
		where TMapping : class, IEntityMapping<TEntity, TMapping>, ICountable

	{
		private ICommand okCmd;
		private ICommand cancelCmd;

		private readonly PartsContext context;

		public TEntity Part { get; private set; }

		public EditEntityViewModel(PartsContext context, TEntity part)
		{
			Part = part ?? throw new NullReferenceException(nameof(part));
			this.context = context ?? throw new NullReferenceException(nameof(context));
		}

		public ICommand OkCmd =>
			okCmd ?? (okCmd = new RelayCommand(p => Ok(), p => OkCmdCanExecute()));

		public ICommand CancelCmd =>
			cancelCmd ?? (cancelCmd = new RelayCommand(p => Cancel()));

		private void Ok()
		{
			var matchedParts = from parts in context.Parts
							   where parts.Name.Equals(Part.Name, StringComparison.OrdinalIgnoreCase)
							   select parts;
			var existingPart = matchedParts.FirstOrDefault();

			if (existingPart != null)
			{
				MessageBox.Show("Запись с таким именем уже существует",
					"Ошибка",
					MessageBoxButton.OK,
					icon: MessageBoxImage.Error);
				return;
			}

			try
			{
				context.SaveChanges();
				DialogResult = true;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message,
					"Ошибка", 
					MessageBoxButton.OK,
					icon: MessageBoxImage.Error);
			}
		}

		private void Cancel()
		{
			context.RejectChanges();
			DialogResult = false;
		}
			
		private bool OkCmdCanExecute() =>
			Part != null && !Part.HasErrors;
	}
}
