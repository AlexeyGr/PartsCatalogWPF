using PartsCatalog.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Db
{
	public class PartsMapping : ValidationBase,  IEntityMapping<Part, PartsMapping>, ICountable
	{
		private int childCount;

		public int ChildId { get; set; }

		public int ParentId { get; set; }

		public virtual Part Child { get; set; }

		public virtual Part Parent { get; set; }

		[Range(1, int.MaxValue)]
		[RegularExpression("([0-9]+)")]
		public int Count
		{
			get => childCount;
			set
			{
				childCount = value;
				ValidateProperty(value);
				RaisePropertyChanged();
			}
		}
	}
}
