using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Interfaces
{
	public interface IHierachicalEntity<TEntity, TMapping>
		where TMapping : class, IEntityMapping<TEntity, TMapping>
		where TEntity : class, IHierachicalEntity<TEntity, TMapping>
	{
		int Id { get; set; }

		string Name { get; set; }

		ICollection<TMapping> ParentsMappings { get; set; }

		ICollection<TMapping> ChildrenMappings { get; set; }
	}
}
