using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EF
{
	[Table("ProductOrder")]
	public class ProductOrder
	{
		[Key]
		public int ProductOrderId { get; set; }   // Khóa chính tự tăng

		public int ProductId { get; set; }        // Id sản phẩm

		[StringLength(50)]
		public string Name { get; set; }          // Tên sản phẩm

		public int Price { get; set; }            // Giá tại thời điểm mua

		public int? Discount { get; set; }        // Giảm giá (có thể null)

		public int OrderDetailId { get; set; }    // Id chi tiết hóa đơn


		
	}
}
