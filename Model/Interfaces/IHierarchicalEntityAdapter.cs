using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Interfaces
{
	public interface IHierarchicalEntityAdapter<TEntity, TMapping>
		where TMapping : class, IEntityMapping<TEntity, TMapping>
		where TEntity : class, IHierachicalEntity<TEntity, TMapping>

	{
		IHierarchicalEntityAdapter<TEntity, TMapping> Parent { get; set; }

		TEntity Entity { get; set; }

		ICollection<IHierarchicalEntityAdapter<TEntity, TMapping>> Children { get; set; }

		void UpdateChildren();
	}
}
