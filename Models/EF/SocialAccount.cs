using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Models.EF
{
	public class SocialAccount
	{
		[Key]
		public int Id { get; set; }

		public int UserId { get; set; }

		[Required]
		[StringLength(50)]
		public string Provider { get; set; }

		[Required]
		[StringLength(255)]
		public string ProviderUserId { get; set; }

		[ForeignKey("UserId")]
		public virtual User User { get; set; }
	}
}
