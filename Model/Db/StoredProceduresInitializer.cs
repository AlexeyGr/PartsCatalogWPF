using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Db
{
	public class StoredProceduresInitializer : CreateDatabaseIfNotExists<PartsContext>
	{
		protected override void Seed(PartsContext context)
		{
			context.Database.ExecuteSqlCommand(@"
				CREATE PROCEDURE PartCompositionSummary 
				@id INT
				AS
				BEGIN
					WITH query AS (
						SELECT PartsMappings.ChildId, PartsMappings.ParentId, Parts.Name, PartsMappings.[Count]
						FROM PartsMappings
						JOIN Parts
						ON ChildId = Id
						WHERE PartsMappings.ParentId = @id
	
						UNION ALL
						SELECT pm.ChildId, pm.ParentId, Parts.Name, pm.[Count] * res.[Count]
						FROM PartsMappings pm
						JOIN Parts
						ON ChildId = Id
						JOIN query res
						ON res.ChildId = pm.ParentId
					)
					SELECT query.Name, sum(query.[Count]) AS [Count] FROM query

					LEFT JOIN PartsMappings pm
					ON pm.ParentId = query.ChildId
					WHERE pm.ChildId IS NULL
					GROUP BY Name
				END");
		}
	}
}
