using Models.DAO;
using Models.EF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
	public class ProductController : HomeController
	{
		DBNoiThat db = new DBNoiThat();

		[HasCredential(RoleId = "VIEW_PRODUCT")]
		public ActionResult Show()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			var productViewModels = (from a in db.Products
									 join b in db.Providers on a.ProviderId equals b.ProviderId
									 join c in db.Categories on a.CateId equals c.CategoryId
									 select new ProductViewModel
									 {
										 ProductId = a.ProductId,
										 Name = a.Name,
										 Description = a.Description,
										 Discount = a.Discount,
										 ProviderName = b.Name,
										 CateName = c.Name,
										 Price = a.Price,
										 Quantity = a.Quantity,
										 StartDate = a.StartDate,
										 EndDate = a.EndDate,
										 Photo = a.Photo,
									 }).ToList();

			return View(productViewModels);
		}

		[HasCredential(RoleId = "VIEW_PRODUCT")]
		public ActionResult SearchById(int? id)
		{
			// Lưu lại giá trị vừa nhập
			ViewBag.SearchId = id;

			if (id == null)
			{
				return RedirectToAction("Show");
			}

			var result = (from a in db.Products
						  join b in db.Providers on a.ProviderId equals b.ProviderId
						  join c in db.Categories on a.CateId equals c.CategoryId
						  where a.ProductId == id
						  select new ProductViewModel
						  {
							  ProductId = a.ProductId,
							  Name = a.Name,
							  Description = a.Description,
							  Discount = a.Discount,
							  ProviderName = b.Name,
							  CateName = c.Name,
							  Price = a.Price,
							  Quantity = a.Quantity,
							  StartDate = a.StartDate,
							  EndDate = a.EndDate,
							  Photo = a.Photo,
						  }).ToList();

			return View("Show", result);
		}

		[HasCredential(RoleId = "VIEW_PRODUCT")]
		public ActionResult SearchByName(string name)
		{
			ViewBag.SearchName = name;

			if (string.IsNullOrEmpty(name))
			{
				return RedirectToAction("Show");
			}

			var result = (from a in db.Products
						  join b in db.Providers on a.ProviderId equals b.ProviderId
						  join c in db.Categories on a.CateId equals c.CategoryId
						  where a.Name.Contains(name)
						  select new ProductViewModel
						  {
							  ProductId = a.ProductId,
							  Name = a.Name,
							  Description = a.Description,
							  Discount = a.Discount,
							  ProviderName = b.Name,
							  CateName = c.Name,
							  Price = a.Price,
							  Quantity = a.Quantity,
							  StartDate = a.StartDate,
							  EndDate = a.EndDate,
							  Photo = a.Photo,
						  }).ToList();

			return View("Show", result);
		}


		[HttpGet]
		[HasCredential(RoleId = "ADD_PRODUCT")]
		public ActionResult Add()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;
			ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
			ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name");
			return View();
		}

		[HttpPost]
		public ActionResult Add(ProductViewModel n,
						HttpPostedFileBase UploadImage,
						HttpPostedFileBase[] UploadImagesDetails)
		{
			// Luôn phải set lại ViewBag khi return View()
			ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
			ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name");

			if (!ModelState.IsValid)
			{
				return View(n);   // QUAN TRỌNG!
			}

			Product model = new Product();

			// Ảnh đại diện
			if (UploadImage != null)
			{
				string fileName = Path.GetFileName(UploadImage.FileName);
				string path = Path.Combine(Server.MapPath("~/image"), fileName);
				UploadImage.SaveAs(path);
				model.Photo = fileName;
			}

			model.Name = n.Name;
			model.Price = n.Price;
			model.Quantity = n.Quantity;
			model.StartDate = n.StartDate;
			model.EndDate = n.EndDate;
			model.CateId = n.CateId;
			model.ProviderId = n.ProviderId;
			model.Description = n.Description;
			model.Discount = n.Discount;

			db.Products.Add(model);
			db.SaveChanges();   // tạo ProductId

			// Ảnh chi tiết
			if (UploadImagesDetails != null)
			{
				foreach (var img in UploadImagesDetails)
				{
					if (img != null)
					{
						string fileName = Path.GetFileName(img.FileName);
						string path = Path.Combine(Server.MapPath("~/image"), fileName);
						img.SaveAs(path);

						AnhChiTiet ct = new AnhChiTiet();
						ct.ProductId = model.ProductId; // đã có ID
						ct.Photo = fileName;

						db.AnhChiTiets.Add(ct);
					}
				}
			}

			db.SaveChanges();
			return RedirectToAction("Show", new { CateId = n.CateId });
		}




		[HttpGet]
		[HasCredential(RoleId = "EDIT_PRODUCT")]
		public ActionResult Edit(int ProductId)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			var model = (from a in db.Products
						 join b in db.Providers on a.ProviderId equals b.ProviderId
						 join c in db.Categories on a.CateId equals c.CategoryId
						 where a.ProductId == ProductId
						 select new ProductViewModel
						 {
							 ProductId = a.ProductId,
							 Name = a.Name,
							 Description = a.Description,
							 Discount = a.Discount,
							 ProviderName = b.Name,
							 CateName = c.Name,
							 Price = a.Price,
							 Quantity = a.Quantity,
							 StartDate = a.StartDate,
							 EndDate = a.EndDate,
							 Photo = a.Photo,
							 CateId = a.CateId,

							 // LẤY DANH SÁCH ẢNH CHI TIẾT
							 ListPhotoDetail = db.AnhChiTiets
												 .Where(x => x.ProductId == ProductId)
												 .ToList()
						 }).FirstOrDefault();

			ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
			ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name");

			return View(model);
		}

		[HttpPost]
		[HasCredential(RoleId = "EDIT_PRODUCT")]
		public ActionResult Edit(ProductViewModel n,
								 HttpPostedFileBase UploadImage,
								 HttpPostedFileBase[] UploadImagesDetails)
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
			ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name");

			if (ModelState.IsValid)
			{
				var model = db.Products.FirstOrDefault(m => m.ProductId == n.ProductId);

				// === 1. UPDATE ẢNH ĐẠI DIỆN ===
				if (UploadImage != null)
				{
					string fileName = Path.GetFileName(UploadImage.FileName);
					string path = Path.Combine(Server.MapPath("~/image"), fileName);
					UploadImage.SaveAs(path);
					model.Photo = fileName;
				}

				// === 2. UPDATE THÔNG TIN SẢN PHẨM ===
				model.Name = n.Name;
				model.Price = n.Price;
				model.Quantity = n.Quantity;
				model.StartDate = n.StartDate;
				model.EndDate = n.EndDate;
				model.CateId = n.CateId;
				model.Description = n.Description;
				model.Discount = n.Discount;
				model.ProviderId = n.ProviderId;

				// === 3. UPDATE ẢNH CHI TIẾT ===
				if (UploadImagesDetails != null)
				{
					foreach (var img in UploadImagesDetails)
					{
						if (img != null)
						{
							string fileName = Path.GetFileName(img.FileName);
							string path = Path.Combine(Server.MapPath("~/image"), fileName);
							img.SaveAs(path);

							AnhChiTiet ct = new AnhChiTiet();
							ct.ProductId = model.ProductId;
							ct.Photo = fileName;

							db.AnhChiTiets.Add(ct);
						}
					}
				}


				db.SaveChanges();
				return RedirectToAction("Show", new { CateId = n.CateId });
			}
			else
			{
				ModelState.AddModelError("", "Ngày kết thúc phải muộn hơn ngày bắt đầu");
				return View();
			}
		}
		[HasCredential(RoleId = "EDIT_PRODUCT")]
		public ActionResult DeleteDetailImage(int id, int productId)
		{
			var img = db.AnhChiTiets.Find(id);
			if (img != null)
			{
				// xóa file vật lý nếu muốn:
				string path = Server.MapPath("~/image/" + img.Photo);
				if (System.IO.File.Exists(path))
					System.IO.File.Delete(path);

				db.AnhChiTiets.Remove(img);
				db.SaveChanges();
			}

			return RedirectToAction("Edit", new { ProductId = productId });
		}

		//[HttpGet]
		//[HasCredential(RoleId = "DELETE_PRODUCT")]
		//public ActionResult Delete()
		//{
		//    var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
		//    ViewBag.username = session.Username;
		//    return View();
		//}
		[HttpGet]
		[HasCredential(RoleId = "DELETE_PRODUCT")]
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return HttpNotFound();
			}

			var model = db.Products.Find(id);
			if (model != null)
			{
				db.Products.Remove(model);
				db.SaveChanges();
			}

			return RedirectToAction("Show");
		}
	
		public ActionResult Menu()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
			ViewBag.username = session.Username;

			var model = new CategoryDao().ListCategory();
			return PartialView(model);
		}
	}
}