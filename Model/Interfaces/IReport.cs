using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model.Interfaces
{
	interface IReport
	{
		void CreateAndOpen(string header, string filePath, string exePath, string[][] data);

		void Create(string header, string filePath, string[][] data);
	}
}
