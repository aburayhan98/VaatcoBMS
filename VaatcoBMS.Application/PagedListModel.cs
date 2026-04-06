using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaatcoBMS.Application;

public class PagedListModel<T> where T : IBasePaginationModel, new()
{
	public PagedListModel(IEnumerable<T> rows)
	{
		Rows = rows;
		Total = rows.FirstOrDefault()?.Total ?? 0;
	}

	public int Total { get; set; }
	public IEnumerable<T> Rows { get; set; }
}
