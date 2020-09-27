using MvvmDialogs;
using PartsCatalog.Model;
using PartsCatalog.Model.Db;
using PartsCatalog.View;
using PartsCatalog.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PartsCatalog
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		private PartsContext partsContext;
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			//TODO: Understand how to check if DB schema not matching context
			//calling on all sets FirstOrDefault()?
			partsContext = new PartsContext();

			ViewTypeLocator viewTypeLocator = new ViewTypeLocator();
			viewTypeLocator.Register<MainViewModel, MainView>();
			viewTypeLocator.Register<AddEntityViewModel<Part, PartsMapping>, AddEntityView>();
			viewTypeLocator.Register<EditEntityViewModel<Part, PartsMapping>, EditEntityView>();

			IDialogService dialogService = new DialogService(dialogTypeLocator: viewTypeLocator);
			MainViewModel mainViewModel = new MainViewModel(dialogService, partsContext);
			MainView mainView = new MainView
			{
				DataContext = mainViewModel
			};
			mainView.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			partsContext?.Dispose();
			base.OnExit(e);
		}
	}
}
