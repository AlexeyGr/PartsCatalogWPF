using MvvmDialogs.DialogTypeLocators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PartsCatalog.View
{
    public class ViewTypeLocator : IDialogTypeLocator
    {
        private readonly Dictionary<Type, Type> mappings;

        public ViewTypeLocator()
        {
            mappings = new Dictionary<Type, Type>();
        }

        public Type Locate(INotifyPropertyChanged viewModel)
        {
            Type viewType = mappings[viewModel.GetType()];
            return viewType;
        }

        public void Register<TViewModel, TView>() where TViewModel : INotifyPropertyChanged
                                                  where TView : Window
        {
            if (!mappings.ContainsKey(typeof(TViewModel)))
                mappings.Add(typeof(TViewModel), typeof(TView));
        }
    }
}
