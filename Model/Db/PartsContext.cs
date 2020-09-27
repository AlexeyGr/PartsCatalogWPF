using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Db
{
	public class PartsContext : DbContext
	{
		public DbSet<Part> Parts { get; set; }
		public DbSet<PartsMapping> PartsRelations { get; set; }

		public PartsContext():base("PartsCatalogConnection")
		{
			Database.SetInitializer(new StoredProceduresInitializer());
		}

		public bool Save(out string error)
		{
			StringBuilder errors = new StringBuilder();
			error = string.Empty;
			try
			{
				SaveChanges();
				return true;
			}
			catch (DbEntityValidationException dbeve)
			{
				foreach (var valErr in dbeve.EntityValidationErrors)
					foreach (var err in valErr.ValidationErrors)
						errors.AppendLine(err.ErrorMessage);
				RejectChanges();
			}
			catch (Exception ex)
			{
				foreach (var e in Helper.GetInnerExceptions(ex))
					errors.AppendLine(e.Message);
				RejectChanges();
			}
			error = errors.ToString();
			return false;
		}

		protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
		{
			if (entityEntry.Entity is PartsMapping mapping)
			{
				if (CheckLoop(mapping.Child, mapping.Child))
				{
					var list = new List<DbValidationError>
					{
						new DbValidationError(mapping.Child.Name,
							$"Компонент \"{mapping.Child.Name}\" не может быть в составе \"{mapping.Parent.Name}\"")
					};
					return new DbEntityValidationResult(entityEntry, list);
				}
				else
					return base.ValidateEntity(entityEntry, items);
			}
			return base.ValidateEntity(entityEntry, items);
		}

		// if there is a loop when item added we will return to it (startingPart)
		private bool CheckLoop(Part part, Part startingPart)
		{
			if (part.ChildrenMappings.Count == 0)
				return false;

			var parts = part.ChildrenMappings.Select(c => c.Child);
			if (parts.Contains(startingPart))
				return true;
			else
			{
				foreach (var p in parts)
				{
					if (CheckLoop(p, startingPart))
						return true;
				}
			}
			return false;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PartsMapping>()
				.HasKey(r => new { r.ParentId, r.ChildId });

			modelBuilder.Entity<Part>()
				.HasMany(p => p.ChildrenMappings)
				.WithRequired(p => p.Parent)
				.HasForeignKey(p => p.ParentId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Part>()
				.HasMany(p => p.ParentsMappings)
				.WithRequired(p => p.Child)
				.HasForeignKey(p => p.ChildId)
				.WillCascadeOnDelete(false);
		}

		public void RejectChanges()
		{
			foreach (var entry in ChangeTracker.Entries())
			{
				switch (entry.State)
				{
					case EntityState.Added:
						entry.State = EntityState.Detached;
						break;
					case EntityState.Modified:
						entry.CurrentValues.SetValues(entry.OriginalValues);
						entry.State = EntityState.Unchanged;
						break;
					case EntityState.Deleted:
						entry.State = EntityState.Unchanged;
						break;
				}
			}
		}

		public void RemoveEntity<T>(T entity) where T : class
		{
			Set<T>().Remove(entity);
		}

		public void AddEntity<T>(T entity) where T : class
		{
			Set<T>().Add(entity);
		}
	}
}
