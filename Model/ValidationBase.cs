using PartsCatalog.Model.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model
{
	public abstract class ValidationBase : NotifyPropertyChangedBase, INotifyDataErrorInfo, IModelValidator
	{
		#region Validation

		private readonly object _lock = new object();
		private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

		public bool HasErrors =>
			errors.Any(propErrors => propErrors.Value != null && propErrors.Value.Count > 0);

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public IEnumerable GetErrors(string propertyName)
		{
			if (!string.IsNullOrEmpty(propertyName))
			{
				if (errors.ContainsKey(propertyName) && (errors[propertyName] != null) && errors[propertyName].Count > 0)
					return errors[propertyName].ToList();
				else
					return null;
			}
			else
				return errors.SelectMany(err => err.Value.ToList());
		}

		public void OnErrorsChanged(string prop) =>
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));

		public void ValidateProperty(object value, [CallerMemberName] string prop = "")
		{
			lock (_lock)
			{
				var validationContext = new ValidationContext(this, null, null)
				{
					MemberName = prop
				};
				var validationResults = new List<ValidationResult>();
				Validator.TryValidateProperty(value, validationContext, validationResults);

				if (errors.ContainsKey(prop))
					errors.Remove(prop);
				OnErrorsChanged(prop);
				HandleValidationResults(validationResults);
			}
		}

		public void Validate()
		{
			lock (_lock)
			{
				var validationContext = new ValidationContext(this, null, null);
				var validationResults = new List<ValidationResult>();
				Validator.TryValidateObject(this, validationContext, validationResults, true);

				var propNames = errors.Keys.ToList();
				errors.Clear();
				propNames.ForEach(pn => OnErrorsChanged(pn));
				HandleValidationResults(validationResults);
			}
		}

		private void HandleValidationResults(List<ValidationResult> validationResults)
		{
			var resultsByPropNames = from res in validationResults
									 from mname in res.MemberNames
									 group res by mname into g
									 select g;

			foreach (var prop in resultsByPropNames)
			{
				var messages = prop.Select(r => r.ErrorMessage).ToList();
				errors.Add(prop.Key, messages);
				OnErrorsChanged(prop.Key);
			}
		}
		#endregion
	}
}
