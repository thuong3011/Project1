using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
	[Table("AnhChiTiet")]
	public class AnhChiTiet
	{
		[Key]
		public int Id { get; set; }

		public int ProductId { get; set; }

		public string Photo { get; set; }
	}
}

