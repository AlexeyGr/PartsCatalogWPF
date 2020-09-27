using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Interfaces
{
	public interface IModelValidator
	{
		void ValidateProperty(object value, string prop);

		void Validate();
	}
}
