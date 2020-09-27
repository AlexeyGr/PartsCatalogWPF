using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Interfaces
{
	public interface IEntityMapping<TEntity, TMapping>
		where TEntity : class, IHierachicalEntity<TEntity, TMapping>
		where TMapping : class, IEntityMapping<TEntity, TMapping>
	{
		int ChildId { get; set; }

		int ParentId { get; set; }

		TEntity Child { get; set; }

		TEntity Parent { get; set; }
	}
}
