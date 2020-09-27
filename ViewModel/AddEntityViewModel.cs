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
	public class AddEntityViewModel<TEntity, TMapping>: ModalDialogViewModelBase
		where TEntity : class, IHierachicalEntity<TEntity, TMapping>, IModelValidator, INotifyDataErrorInfo, new()
		where TMapping : class, IEntityMapping<TEntity, TMapping>, IModelValidator, INotifyDataErrorInfo, ICountable, new()
	{
		private ICommand okCmd;
		private ICommand cancelCmd;

		private readonly IHierarchicalEntityAdapter<TEntity, TMapping> parent;
		private readonly PartsContext context;

		public TEntity NewEntity { get; private set; }

		public TMapping EntitiesMapping { get; set; }

		public bool IsRoot =>
			parent == null;
		
		public AddEntityViewModel(PartsContext context, IHierarchicalEntityAdapter<TEntity, TMapping> parent = null)
		{
			this.parent = parent;
			this.context = context ?? throw new NullReferenceException(nameof(context));

			NewEntity = new TEntity();

			if (this.parent != null)
			{
				EntitiesMapping = new TMapping()
				{
					Child = NewEntity,
					Parent = this.parent.Entity
				};
				EntitiesMapping.Validate();
			}
			NewEntity.Validate();
		}

		public ICommand OkCmd =>
			okCmd ?? (okCmd = new RelayCommand(p => Ok(), p => OkCmdCanExecute()));

		public ICommand CancelCmd =>
			cancelCmd ?? (cancelCmd = new RelayCommand(p => Cancel()));

		private void Ok()
		{
			var matchedParts = from parts in context.Set<TEntity>()
							   where parts.Name.Equals(NewEntity.Name, StringComparison.OrdinalIgnoreCase)
							   select parts;

			var existingPart = matchedParts.FirstOrDefault();

			if (existingPart != null)
			{
				if (EntitiesMapping != null)
				{
					var existingMappings = from mappings in context.Set<TMapping>()
										   where mappings.Child.Name
										   .Equals(NewEntity.Name, StringComparison.OrdinalIgnoreCase)
										   && mappings.Parent.Name
										   .Equals(parent.Entity.Name, StringComparison.OrdinalIgnoreCase)
										   select mappings;

					if (existingMappings.Count() != 0)
					{
						MessageBox.Show("Запись с таким именем уже существует",
							"Ошибка",
							MessageBoxButton.OK,
							icon: MessageBoxImage.Error);
						return;
					}

					EntitiesMapping.Child = existingPart;
					parent.Entity.ChildrenMappings.Add(EntitiesMapping);
				}
				else
				{
					MessageBox.Show("Запись с таким именем уже существует",
						"Ошибка",
						MessageBoxButton.OK,
						icon: MessageBoxImage.Error);
					return;
				}
			}
			else
			{
				if (EntitiesMapping != null)
				{
					EntitiesMapping.Child = NewEntity;
					parent.Entity.ChildrenMappings.Add(EntitiesMapping);
				}
				context.AddEntity(NewEntity);
			}

			if (!context.Save(out string err))
				MessageBox.Show(err, 
					"Ошибка",
					MessageBoxButton.OK,
					icon: MessageBoxImage.Error);
			else
				DialogResult = true;
		}

		private void Cancel()
		{
			context.RejectChanges();
			DialogResult = false;
		}
			
		private bool OkCmdCanExecute()
		{
			return (NewEntity != null && !NewEntity.HasErrors)
				&& (parent == null || (EntitiesMapping != null && !EntitiesMapping.HasErrors));
		}
	}
}
