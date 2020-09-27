using PartsCatalog.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model
{
	public class EntityAdapter<TEntity, TMapping> : NotifyPropertyChangedBase, IHierarchicalEntityAdapter<TEntity, TMapping>
		where TEntity: class, IHierachicalEntity<TEntity, TMapping>
		where TMapping: class, IEntityMapping<TEntity, TMapping>, ICountable
	{
		private ICollection<IHierarchicalEntityAdapter<TEntity, TMapping>> parts;
		public IHierarchicalEntityAdapter<TEntity, TMapping> Parent { get; set; }

		public TEntity Entity { get; set; }

		public int Count { get; set; }
		
		public EntityAdapter(TEntity part, EntityAdapter<TEntity, TMapping> parent = null, int count = 0)
		{
			Entity = part ?? throw new NullReferenceException(nameof(part));
			Parent = parent;
			Count = count;
		}

		public ICollection<IHierarchicalEntityAdapter<TEntity, TMapping>> Children
		{
			get => parts ?? (parts = new ObservableCollection<IHierarchicalEntityAdapter<TEntity, TMapping>>
				(Entity.ChildrenMappings
				.Select(p => new EntityAdapter<TEntity, TMapping>(p.Child, this, p.Count))));
			set
			{
				parts = value;
				RaisePropertyChanged();
			}
		}

		public void UpdateChildren()
		{
			Children = new ObservableCollection<IHierarchicalEntityAdapter<TEntity, TMapping>>(Entity.ChildrenMappings
				.Select(p => new EntityAdapter<TEntity, TMapping>(p.Child, this, p.Count)));
		}
	}
}
