using Models.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Areas.Admin.Models;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
	public class OrdersController : HomeController
	{
		private DBNoiThat db = new DBNoiThat();

		// ===================== SHOW =====================
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Show(int? searchId)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			ViewBag.CurrentFilter = searchId;

			var baseQuery = from a in db.Orders
							join b in db.Status on a.StatusId equals b.StatusId
							where a.StatusId != 5
							select new OrderView
							{
								OrderId = a.OrderId,
								ShipAddress = a.ShipAddress,
								ShipEmail = a.ShipEmail,
								ShipName = a.ShipName,
								ShipPhone = a.ShipPhone,
								StatusName = b.Name,
								UpdateDate = a.UpdateDate,
								UserId = a.UserId,
							};

			if (searchId.HasValue && searchId.Value > 0)
			{
				var order = baseQuery
					.Where(x => x.OrderId == searchId.Value)
					.FirstOrDefault();

				if (order == null)
				{
					TempData["NotFound"] = "Không tìm thấy hóa đơn có mã: " + searchId.Value;
					return RedirectToAction("Show");
				}

				var oneItemList = new List<OrderView> { order };
				return View(oneItemList);
			}

			return View(baseQuery.ToList());
		}

		// ===================== DETAILS =====================
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Details(int? id)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Order order = db.Orders.Find(id);
			if (order == null)
				return HttpNotFound();

			ViewBag.aaaa = db.Status.SingleOrDefault(x => x.StatusId == order.StatusId).Name;

			// LẤY SẢN PHẨM TỪ ProductOrder
			var orderproducts = (
				 from detail in db.OrderDetails
				 join po in db.ProductOrders
					 on detail.OrderDetailId equals po.OrderDetailId
				 select new OrderProduct
				 {
					 OrderId = detail.OrderId,
					 ProductName = po.Name,
					 Quantity = detail.Quantity,
					 Price = po.Price,
					 ProductId = po.ProductId
				 }
			 ).Where(o => o.OrderId == order.OrderId).ToList();

			ViewBag.orderproducts = orderproducts;

			double? total = 0;
			foreach (OrderProduct item in orderproducts)
				total += item.Price * item.Quantity;

			ViewBag.total = total;

			return View(order);
		}

		// ===================== CREATE =====================
		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "OrderId,ShipName,UserId,ShipPhone,ShipEmail,UpdateDate,ShipAddress")] Order order)
		{
			if (ModelState.IsValid)
			{
				db.Orders.Add(order);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(order);
		}

		// ===================== EDIT =====================
		[HasCredential(RoleId = "EDIT_ORDER")]
		public ActionResult Edit(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Order order = db.Orders.Find(id);
			if (order == null)
				return HttpNotFound();

			ViewBag.ListStatus = new SelectList(db.Status.ToList(), "StatusId", "Name");
			return View(order);
		}


		[HasCredential(RoleId = "EDIT_ORDER")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(Order order)
		{
			if (ModelState.IsValid)
			{
				order.UpdateDate = DateTime.Now;

				db.Entry(order).State = EntityState.Modified;
				db.SaveChanges();

				return RedirectToAction("Show");
			}

			ViewBag.ListStatus = new SelectList(db.Status.ToList(), "StatusId", "Name");
			return View(order);
		}

		// ===================== DELETE =====================
		[HasCredential(RoleId = "DELETE_ORDER")]
		[HttpGet]
		public ActionResult Delete(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Order order = db.Orders.Find(id);
			if (order == null)
				return HttpNotFound();

			db.Orders.Remove(order);
			db.SaveChanges();

			TempData["Message"] = "Đơn hàng đã được xóa thành công!";
			return RedirectToAction("Show");
		}


		[HasCredential(RoleId = "DELETE_ORDER")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			Order order = db.Orders.Find(id);
			if (order == null)
				return HttpNotFound();

			db.Orders.Remove(order);
			db.SaveChanges();

			TempData["Message"] = "Đơn hàng đã được xóa thành công!";
			return RedirectToAction("Show");
		}

		// ===================== DISPOSE =====================
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}

	}
}
