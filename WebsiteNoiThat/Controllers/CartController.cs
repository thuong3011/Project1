using Models.DAO;
using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Controllers
{
	public class CartController : Controller
	{
		DBNoiThat db = new DBNoiThat();
		private const string CartSession = "CartSession";
		private const string CartCookieName = "MyCartCookie";

		private void SaveCartCookie(List<CartItem> cart)
		{
			var cartStr = "";
			if (cart != null && cart.Count > 0)
			{

				var items = cart.Select(x => x.Product.ProductId + ":" + x.Quantity).ToArray();
				cartStr = string.Join("|", items);
			}
			var cookie = new HttpCookie(CartCookieName, cartStr);
			cookie.Expires = DateTime.Now.AddDays(30);
			Response.Cookies.Add(cookie);
		}

		private List<CartItem> LoadCartCookie()
		{
			var cart = new List<CartItem>();
			var cookie = Request.Cookies[CartCookieName];
			if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
			{
				var items = cookie.Value.Split('|');
				foreach (var itemStr in items)
				{
					var parts = itemStr.Split(':');
					if (parts.Length == 2)
					{
						if (int.TryParse(parts[0], out int id) && int.TryParse(parts[1], out int qty))
						{
							var product = db.Products.Find(id);
							if (product != null)
							{
								cart.Add(new CartItem { Product = product, Quantity = qty });
							}
						}
					}
				}
			}
			return cart;
		}

		private void ClearCartCookie()
		{
			var cookie = new HttpCookie(CartCookieName, "");
			cookie.Expires = DateTime.Now.AddDays(-1);
			Response.Cookies.Add(cookie);
		}



		public ActionResult Index()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
			if (session != null)
			{
				var cart = (List<CartItem>)Session[CartSession];


				if (cart == null || cart.Count == 0)
				{
					cart = LoadCartCookie();
					if (cart.Count > 0) Session[CartSession] = cart;
				}

				if (cart != null && cart.Count > 0)
				{
					ViewBag.Status = "Đang chờ xác nhận";
				}
				return View(cart);
			}
			else
			{
				return Redirect("/dang-nhap");
			}
		}

		public ActionResult AddCart(int productId, int quantity)
		{
			var product = db.Products.Find(productId);
			var cart = Session[CartSession];
			var list = new List<CartItem>();


			if (cart != null) list = (List<CartItem>)cart;
			else list = LoadCartCookie();

			if (list.Exists(x => x.Product.ProductId == productId))
			{
				foreach (var item in list)
				{
					if (item.Product.ProductId == productId) item.Quantity += quantity;
				}
			}
			else
			{
				var item = new CartItem();
				item.Product = product;
				item.Quantity = quantity;
				list.Add(item);
			}

			Session[CartSession] = list;
			SaveCartCookie(list);
			return RedirectToAction("Index");
		}

		public JsonResult Update(string cartModel)
		{
			var jsonCart = new JavaScriptSerializer().Deserialize<List<CartItem>>(cartModel);
			var sessionCart = (List<CartItem>)Session[CartSession] ?? LoadCartCookie();

			foreach (var item in sessionCart)
			{
				var jsonItem = jsonCart.SingleOrDefault(x => x.Product.ProductId == item.Product.ProductId);
				if (jsonItem != null)
				{
					item.Quantity = jsonItem.Quantity;
				}
			}
			Session[CartSession] = sessionCart;
			SaveCartCookie(sessionCart);
			return Json(new { status = true });
		}

		[HttpPost]
		public JsonResult Delete(long? id)
		{
			if (id == null) return Json(new { status = false, message = "ID lỗi" });

			var sessionCart = (List<CartItem>)Session[CartSession] ?? LoadCartCookie();
			sessionCart.RemoveAll(x => x.Product.ProductId == id);

			Session[CartSession] = sessionCart;
			SaveCartCookie(sessionCart);
			return Json(new { status = true });
		}

		public JsonResult DeleteAll()
		{
			Session[CartSession] = null;
			ClearCartCookie();
			return Json(new { status = true });
		}



		public void SendOrderEmail(Order order, List<CartItem> cart)
		{
			if (cart == null || !cart.Any()) return;

			double total = 0;
			string htmldata = "";
			int count = 1;

			foreach (var item in cart)
			{
				if (item.Product == null) continue;

				double price = item.Product.Price.GetValueOrDefault(0);
				double discount = item.Product.Discount.GetValueOrDefault(0);
				double discountPrice = price - (price * discount / 100.0);
				total += discountPrice * item.Quantity;

				htmldata += $@"
        <tr>
            <td>{count}</td>
            <td>{item.Product.Name}</td>
            <td>{item.Quantity}</td>
            <td>{discountPrice.ToString("N0")}</td>
            <td>{discount} %</td>
        </tr>";
				count++;
			}

			string paymentStatus = "";

			if (order.StatusId == 1)        // Chưa thanh toán
			{
				paymentStatus = "Chưa thanh toán";
			}
			else if (order.StatusId == 5)   // Đã thanh toán
			{
				paymentStatus = "Đã thanh toán";
			}
			else
			{
				paymentStatus = "Đang xác định";
			}

			string path = Server.MapPath("~/Common/neworder.html");
			if (!System.IO.File.Exists(path)) return;

			string content = System.IO.File.ReadAllText(path);
			content = content.Replace("{{PaymentStatus}}", paymentStatus);

			content = content.Replace("{{id}}", order.OrderId.ToString());
			content = content.Replace("{{CustomerName}}", order.ShipName ?? "");
			content = content.Replace("{{Phone}}", order.ShipPhone?.ToString() ?? "");

			content = content.Replace("{{Email}}", order.ShipEmail ?? "");
			content = content.Replace("{{Address}}", order.ShipAddress ?? "");
			content = content.Replace("{{Total}}", total.ToString("N0"));
			content = content.Replace("{{data}}", htmldata);

			// OrderTime = thời gian đặt hàng
			content = content.Replace("{{OrderTime}}",
				order.OrderTime?.ToString("dd/MM/yyyy HH:mm"));

			// PaymentTime theo StatusId
			if (order.StatusId == 1)
			{
				content = content.Replace("{{PaymentTime}}", "Chưa thanh toán");
			}
			else if (order.StatusId == 5)
			{
				content = content.Replace("{{PaymentTime}}",
					order.UpdateDate?.ToString("dd/MM/yyyy HH:mm"));
			}

			content = content.Replace("{{PaymentMethod}}", order.PaymentMethod ?? "");

			try
			{
				if (!string.IsNullOrEmpty(order.ShipEmail))
					new MailHelper().SendMail(order.ShipEmail, $"Đơn hàng #{order.OrderId}", content);

				var admin = ConfigurationManager.AppSettings["ToEmailAddress"];
				if (!string.IsNullOrEmpty(admin))
					new MailHelper().SendMail(admin, $"Đơn hàng mới #{order.OrderId}", content);
			}
			catch { }
		}

		[HttpGet]
		public ActionResult PayBy()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
			if (session == null) return Redirect("/dang-nhap");

			var model = db.Users.SingleOrDefault(n => n.UserId == session.UserId);
			var cart = (List<CartItem>)Session[CartSession] ?? LoadCartCookie();
			double total = 0;

			if (cart != null)
			{
				ViewBag.Status = "Đang chờ xác nhận";
				foreach (CartItem item in cart)
				{
					double price = item.Product.Price.GetValueOrDefault(0);
					double discount = item.Product.Discount.GetValueOrDefault(0);
					double discountPrice = price - (price * discount / 100.0);
					total += discountPrice * item.Quantity;
				}
			}
			ViewBag.ListItem = cart;
			ViewBag.total = total;

			return View(model);
		}

		[HttpPost]
		public async Task<ActionResult> PayBy(User n, string PaymentMethod)
		{
			var session = (UserLogin)Session[Commoncontent.user_sesion];
			if (session == null) return Redirect("/dang-nhap");

			var cart = (List<CartItem>)Session[CartSession] ?? LoadCartCookie();
			if (cart == null || cart.Count == 0) return Redirect("/");

			var user = db.Users.SingleOrDefault(a => a.UserId == session.UserId);
			if (user != null)
			{
				user.Name = n.Name;
				user.Phone = n.Phone;
				user.Address = n.Address;
				user.Email = n.Email;
				user.Status = true;
				db.SaveChanges();
			}

			double total = cart.Sum(i =>
			{
				double price = i.Product.Price ?? 0;
				double discount = i.Product.Discount ?? 0;
				return (price - (price * discount / 100.0)) * i.Quantity;
			});

			// --------------------------------
			// 🟦 CASE: THANH TOÁN MOMO
			// --------------------------------
			if (PaymentMethod == "MOMO")
			{
				if (total < 1000)
					return Content("Số tiền thanh toán MoMo phải từ 1.000đ");

				int amount = Convert.ToInt32(total);
				string requestId = "MOMO_" + session.UserId + "_" + DateTime.Now.Ticks;

				var momoService = new MomoService();
				string payUrl = await momoService.CreatePaymentRequest(amount, requestId);

				if (!string.IsNullOrEmpty(payUrl))
				{
					TempData["PendingUser"] = n;      // Lưu tạm thông tin user
					TempData["PendingCart"] = cart;   // Lưu tạm giỏ hàng
					TempData["PendingAmount"] = total;
					TempData["RequestId"] = requestId;
					return Redirect(payUrl);
				}

				return Redirect("/Cart/Error");
			}

			// --------------------------------
			// 🟩 CASE: THANH TOÁN COD → TẠO ĐƠN NGAY
			// --------------------------------
			var order = new Order()
			{
				OrderTime = DateTime.Now,
				UpdateDate = DateTime.Now,
				ShipAddress = n.Address,
				ShipPhone = n.Phone,
				ShipName = n.Name,
				ShipEmail = n.Email,
				UserId = session.UserId,
				StatusId = 1,                 // Đã tiếp nhận
				PaymentMethod = "COD"
			};

			int orderId = new OrderDao().Insert(order);
			var detailDao = new OrderDetailDao();

			foreach (var item in cart)
			{
				double price = item.Product.Price ?? 0;
				double discount = item.Product.Discount ?? 0;
				int finalPrice = (int)(price - (price * discount / 100.0));

				detailDao.Insert(new OrderDetail()
				{
					OrderId = orderId,
					ProductId = item.Product.ProductId,
					Quantity = item.Quantity,
					Price = finalPrice
				});

				var product = db.Products.Find(item.Product.ProductId);
				if (product != null) product.Quantity -= item.Quantity;
			}

			db.SaveChanges();

			SendOrderEmail(order, cart);
			Session[CartSession] = null;
			ClearCartCookie();

			return Redirect("/hoan-thanh");
		}


		[HttpGet]
		public ActionResult MomoReturn(string orderId, string resultCode)
		{
			// Nếu thanh toán FAILED → trở về trang chủ
			if (resultCode != "0")
			{
				TempData.Remove("PendingUser");
				TempData.Remove("PendingCart");
				TempData.Remove("PendingAmount");

				return Redirect("/");
			}

			// --- Lấy dữ tạm ---
			var n = TempData["PendingUser"] as User;
			var cart = TempData["PendingCart"] as List<CartItem>;
			double total = Convert.ToDouble(TempData["PendingAmount"]);

			if (n == null || cart == null)
				return Redirect("/"); // Không có dữ liệu → quay về trang chủ

			var session = (UserLogin)Session[Commoncontent.user_sesion];
			if (session == null)
				return Redirect("/dang-nhap");

			
			var order = new Order()
			{
				OrderTime = DateTime.Now,      // thời gian đặt
				UpdateDate = DateTime.Now,     // thời gian thanh toán
				ShipAddress = n.Address,
				ShipPhone = n.Phone,
				ShipName = n.Name,
				ShipEmail = n.Email,
				UserId = session.UserId,
				StatusId = 5,                  // Đã thanh toán
				PaymentMethod = "MOMO"
			};

			int newOrderId = new OrderDao().Insert(order);

			var detailDao = new OrderDetailDao();
			foreach (var item in cart)
			{
				double price = item.Product.Price ?? 0;
				double discount = item.Product.Discount ?? 0;
				int finalPrice = (int)(price - (price * discount / 100.0));

				detailDao.Insert(new OrderDetail()
				{
					OrderId = newOrderId,
					ProductId = item.Product.ProductId,
					Quantity = item.Quantity,
					Price = finalPrice
				});

				var product = db.Products.Find(item.Product.ProductId);
				if (product != null) product.Quantity -= item.Quantity;
			}

			db.SaveChanges();

			// Gửi mail
			SendOrderEmail(order, cart);

			// Clear cart
			Session[CartSession] = null;
			ClearCartCookie();

			// Chuyển đến trang hoàn thành
			return Redirect("/hoan-thanh");
		}


		public ActionResult HistoryCart()
		{
			var session = (UserLogin)Session[Commoncontent.user_sesion];
			if (session == null) return Redirect("/dang-nhap");

			ViewBag.StatusMenu = db.Status
				.Select(s => new StatusMenuVM
				{
					StatusId = s.StatusId,
					Name = s.Name,
					Count = db.Orders.Count(o => o.UserId == session.UserId && o.StatusId == s.StatusId)
				}).ToList();

			return View();
		}

		public ActionResult HistoryCartPartial(int? statusId, int page = 1)
		{
			int pageSize = 6;
			var session = (UserLogin)Session[Commoncontent.user_sesion];
			if (session == null) return PartialView("_HistoryCartTable", null);

			var query = from po in db.ProductOrders
						join od in db.OrderDetails on po.OrderDetailId equals od.OrderDetailId
						join o in db.Orders on od.OrderId equals o.OrderId
						join p in db.Products on po.ProductId equals p.ProductId
						join s in db.Status on o.StatusId equals s.StatusId
						where o.UserId == session.UserId
						select new HistoryCart
						{
							OrderId = o.OrderId,
							Name = po.Name,
							Photo = p.Photo,
							Quantity = od.Quantity ?? 0,
							Price = po.Price,
							Discount = po.Discount,
							StatusId = o.StatusId,
							NameStatus = s.Name
						};

			if (statusId.HasValue)
				query = query.Where(x => x.StatusId == statusId);

			var model = query
				.OrderByDescending(x => x.OrderId)
				.ToPagedList(page, pageSize);

			return PartialView("_HistoryCartTable", model);
		}

		[HttpPost]
		public JsonResult CancelOrder(long orderId)
		{
			var session = (UserLogin)Session[Commoncontent.user_sesion];
			if (session == null)
				return Json(new { success = false, message = "Chưa đăng nhập" });

			var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == session.UserId);
			
			var details = db.OrderDetails.Where(x => x.OrderId == orderId).ToList();
			foreach (var d in details)
			{
				var product = db.Products.Find(d.ProductId);
				if (product != null)
					product.Quantity += d.Quantity ?? 0;
			}

			order.StatusId = 6;
			order.UpdateDate = DateTime.Now;
			db.SaveChanges();

			var statusCounts = db.Orders
				.Where(o => o.UserId == session.UserId)
				.GroupBy(o => o.StatusId)
				.Select(g => new { StatusId = g.Key, Count = g.Count() })
				.ToList();

			return Json(new { success = true, statusCounts });
		}
		public ActionResult Success()
		{
			var cart = Session[CartSession] ?? LoadCartCookie();
			if (cart != null)
			{
				ViewBag.Status = "Đã tiếp nhận";
				ViewBag.ListItem = (List<CartItem>)cart;
				Session[CartSession] = null;
				ClearCartCookie();
			}
			return View(cart);
		}

	}
}
