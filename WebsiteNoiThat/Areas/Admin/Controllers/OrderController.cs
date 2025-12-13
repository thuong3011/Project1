using Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Models;
using Rotativa;
using WebsiteNoiThat.Common;
using PagedList;
using System.Net;
using WebsiteNoiThat.Areas.Admin.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
	public class OrderController : HomeController
	{
		DBNoiThat db = new DBNoiThat();

		//──────────────────────────────
		// SHOW
		//──────────────────────────────
		[HttpGet]
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Show()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			ViewBag.StatusList = new SelectList(db.Status.ToList(), "StatusId", "Name");

			var model = (from a in db.Orders
						 join b in db.OrderDetails on a.OrderId equals b.OrderId
						 join c in db.ProductOrders on b.OrderDetailId equals c.OrderDetailId
						 join d in db.Status on a.StatusId equals d.StatusId
						 select new OrderViewModel
						 {
							 OrderDetailId1 = b.OrderDetailId,
							 OrderId = a.OrderId,
							 ProductId = c.ProductId,
							 ShipAddress = a.ShipAddress,
							 ShipName = a.ShipName,
							 ShipPhone = a.ShipPhone,
							 Price = c.Price,
							 Quantity = b.Quantity,
							 Discount = c.Discount,
							 UpdateDate = a.UpdateDate,
							 StatusId = a.StatusId,
							 StatusName = d.Name,
							 UserId = a.UserId
						 }).ToList();

			return View(model);
		}

		[HttpPost]
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Show(OrderViewModel model)
		{
			try
			{
				if (model.OrderDetailId1 > 0)
				{
					OrderDetail emp = db.OrderDetails.SingleOrDefault(x => x.OrderDetailId == model.OrderDetailId1);
					db.SaveChanges();
				}
				return Redirect("~/Admin/Order/Show");
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		//──────────────────────────────
		// EDIT ORDERDETAIL
		//──────────────────────────────
		[HasCredential(RoleId = "EDIT_ORDER")]
		public ActionResult AddEditOrder(int OrderDetailId)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			ViewBag.StatusList = new SelectList(db.Status.ToList(), "StatusId", "Name");

			OrderViewModel model = new OrderViewModel();

			if (OrderDetailId > 0)
			{
				OrderDetail emp = db.OrderDetails.SingleOrDefault(x => x.OrderDetailId == OrderDetailId);
				model.OrderId = emp.OrderId;
				model.ProductId = emp.ProductId;
				model.Price = emp.Price;
				model.Quantity = emp.Quantity;
				model.OrderDetailId1 = emp.OrderDetailId;
			}

			return PartialView("~/Areas/Admin/Views/Order/Partial2.cshtml", model);
		}

		//──────────────────────────────
		// DELETE MULTI ORDERDETAIL
		//──────────────────────────────
		[HttpPost]
		[HasCredential(RoleId = "DELETE_ORDER")]
		public ActionResult Delete(FormCollection formCollection)
		{
			string[] ids = formCollection["OrderDetailId1"].Split(new char[] { ',' });
			foreach (string id in ids)
			{
				var model = db.OrderDetails.Find(Convert.ToInt32(id));
				db.OrderDetails.Remove(model);
				db.SaveChanges();
			}
			return RedirectToAction("Show");
		}

		//──────────────────────────────
		// INDEX
		//──────────────────────────────
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Index()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			var models = (from a in db.OrderDetails
						  join b in db.Orders on a.OrderId equals b.OrderId
						  join c in db.ProductOrders on a.OrderDetailId equals c.OrderDetailId
						  join d in db.Status on b.StatusId equals d.StatusId
						  select new OrderViewModel
						  {
							  OrderDetailId1 = a.OrderDetailId,
							  OrderId = a.OrderId,
							  ProductId = c.ProductId,
							  Price = c.Price,
							  Quantity = a.Quantity,
							  Discount = c.Discount,
							  UpdateDate = b.UpdateDate,
						  }).ToList();

			ViewBag.total = models.Sum(x => x.Price * x.Quantity);

			return View(models);
		}

		//──────────────────────────────
		// VIEWMODEL (GET)
		//──────────────────────────────
		[HttpGet]
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Viewmodel()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			var model = (from a in db.Orders
						 join b in db.OrderDetails on a.OrderId equals b.OrderId
						 join c in db.ProductOrders on b.OrderDetailId equals c.OrderDetailId
						 join d in db.Status on a.StatusId equals d.StatusId
						 where a.StatusId == 5
						 select new OrderViewModel
						 {
							 OrderDetailId1 = b.OrderDetailId,
							 OrderId = a.OrderId,
							 ProductId = c.ProductId,
							 ShipAddress = a.ShipAddress,
							 ShipName = a.ShipName,
							 ShipPhone = a.ShipPhone,
							 Price = c.Price,
							 Quantity = b.Quantity,
							 Discount = c.Discount,
							 UpdateDate = a.UpdateDate,
							 StatusId = a.StatusId,
							 StatusName = d.Name,
							 UserId = a.UserId
						 }).ToList();

			ViewBag.total = model.Sum(x => x.Price * x.Quantity);
			return View(model);
		}

		//──────────────────────────────
		// VIEWMODEL POST (FILTER DATE)
		//──────────────────────────────
		[HttpPost]
		public ActionResult Viewmodel(DateTime dfr, DateTime dto)
		{
			var models = (from a in db.Orders
						  join b in db.OrderDetails on a.OrderId equals b.OrderId
						  join c in db.ProductOrders on b.OrderDetailId equals c.OrderDetailId
						  join d in db.Status on a.StatusId equals d.StatusId
						  where a.StatusId == 5
						  select new OrderViewModel
						  {
							  OrderDetailId1 = b.OrderDetailId,
							  OrderId = a.OrderId,
							  ProductId = c.ProductId,
							  ShipAddress = a.ShipAddress,
							  ShipName = a.ShipName,
							  ShipPhone = a.ShipPhone,
							  Price = c.Price,
							  Quantity = b.Quantity,
							  Discount = c.Discount,
							  UpdateDate = a.UpdateDate,
							  StatusId = a.StatusId,
							  StatusName = d.Name,
							  UserId = a.UserId
						  }).ToList();

			var model = models.Where(n => n.UpdateDate >= dfr && n.UpdateDate <= dto).ToList();
			ViewBag.total = model.Sum(x => x.Price * x.Quantity);

			return View(model);
		}

		//──────────────────────────────
		// DETAILS
		//──────────────────────────────
		[HasCredential(RoleId = "VIEW_ORDER")]
		public ActionResult Details(int? id)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Order order = db.Orders.Find(id);
			if (order == null) return HttpNotFound();

			ViewBag.aaaa = db.Status.SingleOrDefault(x => x.StatusId == order.StatusId).Name;

			var orderproducts = (from a in db.OrderDetails
								 join c in db.ProductOrders on a.OrderDetailId equals c.OrderDetailId
								 select new OrderProduct
								 {
									 OrderId = a.OrderId,
									 ProductName = c.Name,
									 Quantity = a.Quantity,
									 Price = c.Price,
									 ProductId = c.ProductId
								 }).Where(o => o.OrderId == order.OrderId).ToList();

			ViewBag.orderproducts = orderproducts;
			ViewBag.total = orderproducts.Sum(x => x.Price);

			return View(order);
		}

		//──────────────────────────────
		// DELETE ONE ORDERDETAIL
		//──────────────────────────────
		[HttpDelete]
		[HasCredential(RoleId = "DELETE_ORDER")]
		public ActionResult Delete(int id)
		{
			var model = db.OrderDetails.SingleOrDefault(n => n.OrderDetailId == id);
			db.OrderDetails.Remove(model);
			db.SaveChanges();
			return RedirectToAction("Show");
		}

		//──────────────────────────────
		// EDIT ORDER STATUS
		//──────────────────────────────
		[HttpGet]
		[HasCredential(RoleId = "EDIT_ORDER")]
		public ActionResult Edit(int OrderDetailId)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			OrderViewModel model = new OrderViewModel();

			if (OrderDetailId > 0)
			{
				OrderDetail emp = db.OrderDetails.SingleOrDefault(x => x.OrderDetailId == OrderDetailId);
				model.OrderId = emp.OrderId;
				model.ProductId = emp.ProductId;
				model.Price = emp.Price;
				model.Quantity = emp.Quantity;
				model.OrderDetailId1 = emp.OrderDetailId;
			}

			return View(model);
		}

		[HttpPost]
		[HasCredential(RoleId = "EDIT_ORDER")]
		public ActionResult Edit(OrderViewModel model)
		{
			if (model.OrderDetailId1 > 0)
			{
				Order order = db.Orders.SingleOrDefault(x => x.OrderId == model.OrderId);
				order.StatusId = model.StatusId.GetValueOrDefault();
				db.SaveChanges();
			}
			return Redirect("~/Admin/Order/Show");
		}

		//──────────────────────────────
		// IN HÓA ĐƠN PDF
		//──────────────────────────────
		public ActionResult IndexById(int id)
		{
			Order order = db.Orders.Find(id);
			ViewBag.aaaa = db.Status.SingleOrDefault(x => x.StatusId == order.StatusId).Name;

			var orderproducts = (
				from a in db.OrderDetails
				join c in db.ProductOrders on a.OrderDetailId equals c.OrderDetailId
				select new OrderProduct
				{
					OrderId = a.OrderId,
					ProductName = c.Name,
					Quantity = a.Quantity,
					Price = c.Price,
					ProductId = c.ProductId
				}
			).Where(o => o.OrderId == order.OrderId).ToList();

			ViewBag.orderproducts = orderproducts;
			ViewBag.total = orderproducts.Sum(x => x.Price);

			return View(order);
		}

		public ActionResult PrintSalarySlip(int id)
		{
			var report = new ActionAsPdf("IndexById", new { id = id });
			return report;
		}
	}
}
