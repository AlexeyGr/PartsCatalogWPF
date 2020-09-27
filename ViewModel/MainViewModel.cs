using MvvmDialogs;
using PartsCatalog.Model;
using PartsCatalog.Model.Db;
using PartsCatalog.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PartsCatalog.ViewModel
{
	public class MainViewModel : NotifyPropertyChangedBase
	{
		private readonly PartsContext partsContext;
		private IHierarchicalEntityAdapter<Part, PartsMapping> selectedPart;
		private ICommand addCmd;
		private ICommand addChildCmd;
		private ICommand removeCmd;
		private ICommand editCmd;
		private ICommand createReportCmd;
		private ICollection<IHierarchicalEntityAdapter<Part, PartsMapping>> parts;
		private readonly IDialogService dialogService;

		public ICollection<IHierarchicalEntityAdapter<Part, PartsMapping>> Parts 
		{
			get => parts;
			set
			{
				parts = value;
				RaisePropertyChanged();
			}
		}

		public IHierarchicalEntityAdapter<Part, PartsMapping> SelectedPart
		{
			get => selectedPart;
			set
			{
				selectedPart = value;
				RaisePropertyChanged();
			}
		}

		//TODO: may to be generic
		public MainViewModel(IDialogService dialogService, PartsContext partsContext)
		{
			this.dialogService = dialogService ?? throw new NullReferenceException(nameof(dialogService));
			this.partsContext = partsContext ?? throw new NullReferenceException(nameof(partsContext));
			Parts = new ObservableCollection<IHierarchicalEntityAdapter<Part, PartsMapping>>(partsContext.Parts
				.Where(p => p.ParentsMappings.Count == 0)
				.AsEnumerable()
				.Select(p => new EntityAdapter<Part, PartsMapping>(p)));
		}

		public ICommand AddCmd =>
			addCmd ?? (addCmd = new RelayCommand(param => Add()));

		public ICommand AddChildCmd =>
			addChildCmd ?? (addChildCmd = new RelayCommand(param => AddChild(), param => SelectedPart != null));

		public ICommand RemoveCmd =>
			removeCmd ?? (removeCmd = new RelayCommand(param => Remove(), param => SelectedPart != null));

		public ICommand EditCmd =>
			editCmd ?? (editCmd = new RelayCommand(param => Edit(), param => SelectedPart != null));

		public ICommand CreateReportCmd =>
			createReportCmd ?? (createReportCmd = new RelayCommand(param => CreateReport(), param => CreateReportCanExecute()));

		public bool CreateReportCanExecute()
		{
			return SelectedPart != null
				&& SelectedPart.Children != null
				&& SelectedPart.Children.Count > 0;
		}
		private void Add()
		{
			var vm = new AddEntityViewModel<Part, PartsMapping>(partsContext);
			if (dialogService.ShowDialog(this, vm) ?? false)
				Parts = new ObservableCollection<IHierarchicalEntityAdapter<Part, PartsMapping>>(partsContext.Parts
					.Where(p => p.ParentsMappings.Count == 0)
					.AsEnumerable()
					.Select(p => new EntityAdapter<Part, PartsMapping>(p)));
		}

		private void AddChild()
		{
			var vm = new AddEntityViewModel<Part, PartsMapping>(partsContext, SelectedPart);
			if (dialogService.ShowDialog(this, vm) ?? false)
				UpdateTree(Parts, SelectedPart);
		}

		private void Remove()
		{
			var dlgRes = MessageBox.Show($"Удалить компонент \"{SelectedPart.Entity.Name}\"?", 
				"Удаление компонента", 
				MessageBoxButton.YesNo, 
				MessageBoxImage.Question);

			if (dlgRes == MessageBoxResult.Yes)
			{
				var parent = SelectedPart.Parent;
				RemovePart(SelectedPart.Parent?.Entity, SelectedPart.Entity);
				if (!partsContext.Save(out string err))
					MessageBox.Show(err,
						"Ошибка", 
						MessageBoxButton.OK,
						icon: MessageBoxImage.Error);
				else
					UpdateTree(Parts, parent);
			}
		}

		private void Edit()
		{
			var vm = new EditEntityViewModel<Part, PartsMapping>(partsContext, SelectedPart.Entity);
			dialogService.ShowDialog(this, vm);
		}

		private void CreateReport()
		{
			var data = partsContext.Database.SqlQuery<ReportEntity>
				($"EXEC PartCompositionSummary {SelectedPart.Entity.Id}")
				.Select(e => new string[] { e.Name, $"{e.Count.ToString()} шт" })
				.ToArray();
			IReport report = new OpenXmlWordReport();

			try
			{
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
					$"{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.docx");
				report.CreateAndOpen(
					$"Отчет о сводном составе: {SelectedPart.Entity.Name}",
					path, "winword.exe", data);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message,
					"Ошибка",
					MessageBoxButton.OK,
					icon: MessageBoxImage.Error);
			}
		}
			   
		//TODO: should to be moved to repo?
		private void RemovePart(Part parent, Part part)
		{
			if (parent != null)
			{
				// find and remove relation if found
				var targetMapping = partsContext.PartsRelations.Find(parent.Id, part.Id);
				if (targetMapping != null)
				{
					try
					{
						parent.ChildrenMappings.Remove(targetMapping);
						part.ParentsMappings.Remove(targetMapping);
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message,
							"Ошибка",
							MessageBoxButton.OK,
							icon: MessageBoxImage.Error);
					}
				}
			}

			//find and remove mappings in children
			if (part.ChildrenMappings.Count > 0)
			{
				PartsMapping[] childrenMappings = new PartsMapping[part.ChildrenMappings.Count];
				part.ChildrenMappings.CopyTo(childrenMappings, 0);
				foreach (var child in childrenMappings)
				{
					RemovePart(part, child.Child);
				}
			}

			//if there is no relations in part remove part
			if (part.ParentsMappings.Count == 0)
			{
				try
				{
					partsContext.RemoveEntity(part);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message,
						"Ошибка",
						MessageBoxButton.OK,
						icon: MessageBoxImage.Error);
				}
			}
		}

		private void UpdateTree(ICollection<IHierarchicalEntityAdapter<Part, PartsMapping>> partProxies,
			IHierarchicalEntityAdapter<Part, PartsMapping> parentPart)
		{
			// if entity added to root collection, then refresh root
			if (parentPart == null)
			{
				Parts = new ObservableCollection<IHierarchicalEntityAdapter<Part, PartsMapping>>(partsContext.Parts
					.Where(p => p.ParentsMappings.Count == 0)
					.AsEnumerable()
					.Select(p => new EntityAdapter<Part, PartsMapping>(p)));
				return;
			}
			else //else looking tree for adapters containing added entity and then update it
			{
				parentPart.UpdateChildren();
				foreach (var partProxy in partProxies)
				{
					if (partProxy.Children?.Count != 0)
					{
						foreach (var part in partProxy.Children.Where(p => p.Entity == parentPart.Entity))
							part.UpdateChildren();
						UpdateTree(partProxy.Children, parentPart);
					}
					else
					{
						partProxy.UpdateChildren();
					}
				}
			}
		}
	}
}
