using PartsCatalog.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Db
{
	
	public class Part : ValidationBase, IHierachicalEntity<Part, PartsMapping>
	{
		private string name;

		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Необходимо задать название")]
		[Index(IsUnique = true)]
		[StringLength(100, MinimumLength = 4, ErrorMessage = "Количество символов от {2} до {1}")]
		public string Name
		{
			get => name;
			set
			{
				name = value;
				ValidateProperty(value);
				RaisePropertyChanged();
			}
		}

		public virtual ICollection<PartsMapping> ParentsMappings { get; set; } = new ObservableCollection<PartsMapping>();

		public virtual ICollection<PartsMapping> ChildrenMappings { get; set; } = new ObservableCollection<PartsMapping>();
	}
}
