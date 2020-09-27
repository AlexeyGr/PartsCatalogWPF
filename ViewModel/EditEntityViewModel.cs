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

		public TEntity Entity { get; private set; }

		public EditEntityViewModel(PartsContext context, TEntity entity)
		{
			Entity = entity ?? throw new NullReferenceException(nameof(entity));
			this.context = context ?? throw new NullReferenceException(nameof(context));
		}

		public ICommand OkCmd =>
			okCmd ?? (okCmd = new RelayCommand(p => Ok(), p => OkCmdCanExecute()));

		public ICommand CancelCmd =>
			cancelCmd ?? (cancelCmd = new RelayCommand(p => Cancel()));

		private void Ok()
		{
			//TODO: should to be moved to repo?
			var matchedParts = from parts in context.Set<TEntity>()
							   where parts.Name.Equals(Entity.Name, StringComparison.OrdinalIgnoreCase)
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
			Entity != null && !Entity.HasErrors;
	}
}
