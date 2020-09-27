using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PartsCatalog.Model
{
	static class Helper
	{
        public static IEnumerable<Exception> GetInnerExceptions(Exception ex)
        {
            if (ex.InnerException == null)
                yield return ex;
            else
                foreach (var e in GetInnerExceptions(ex.InnerException))
                    yield return e;
        }
    }
}
