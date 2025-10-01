﻿using Models;
using Models.DAO;
using Models.EF;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;



namespace WebsiteNoiThat.Controllers
{
	public class ProductController : Controller
	{
		DBNoiThat db = new DBNoiThat();
		// GET: Product
		public ActionResult Index()
		{
			return View();
		}

		//cho home page
		public ActionResult ProductHot()
		{
			var model = new ProductDao().ProductHot();
			return PartialView(model);
		}
		public ActionResult SaleProduct()
		{
			var model = new ProductDao().SaleProduct();
			return PartialView(model);
		}
		
public ActionResult Sales(int? page)
	{
		var pageNumber = page ?? 1;
		var pageSize = 10;

		var list = new ProductDao().SaleProduct();
		var pagedList = list.ToPagedList(pageNumber, pageSize);

		return PartialView(pagedList);
	}

	public ActionResult CateHavana()
		{
			var model = new ProductDao().CateHavana();
			return PartialView(model);
		}
		public ActionResult CateKorea()
		{
			var model = new ProductDao().CateKorea();
			return PartialView(model);
		}
		public ActionResult NewProduct()
		{
			var model = new ProductDao().NewProduct();
			return PartialView(model);
		}

		//public ActionResult DetailProduct(int Id)
		//{
		//  var model = new ProductDao().DetailsProduct(Id);
		//    if (model == null)
		//    {
		//        Response.StatusCode = 404;
		//        return null;
		//    }
		//    return View(model);

		//}

		//in ra theo danh mục sản phẩm
		public ActionResult CategoryShow(int cateId, int page = 1, int pagesize = 16)
		{
			var category = new CategoryDao().ViewDetail(cateId);
			ViewBag.CategoryShow = category;
			int total = 0;
			var model = new ProductDao().ListByCategoryId(cateId, ref total, page, pagesize);
			ViewBag.Total = total;
			ViewBag.Page = page;
			int maxpage = 10;
			int totalpage = 0;
			totalpage = (int)Math.Ceiling((double)total / pagesize);
			ViewBag.Totalpage = totalpage;
			ViewBag.Maxpage = maxpage;
			ViewBag.First = 1;
			ViewBag.Last = maxpage;
			ViewBag.Next = page + 1;
			ViewBag.Prev = page - 1;

			return View(model);
		}

		//tìm kiếm
		public ActionResult Search(string keyword, int page = 1, int pagesize = 16)
		{

			int total = 0;
			var model = new ProductDao().Search(keyword, ref total, page, pagesize);
			ViewBag.Total = total;
			ViewBag.Page = page;
			int maxpage = 10;
			int totalpage = 0;
			totalpage = (int)Math.Ceiling((double)total / pagesize);
			ViewBag.Totalpage = totalpage;
			ViewBag.Maxpage = maxpage;
			ViewBag.First = 1;
			ViewBag.Keyword = keyword;
			ViewBag.Last = maxpage;
			ViewBag.Next = page + 1;
			ViewBag.Prev = page - 1;

			return View(model);
		}

		//tìm theo khoảng giá
		public ActionResult SearchFocus(bool? check0, bool? check1, bool? check2, bool? check3, bool? check4, int? page)
		{
			int pagenumber = (page ?? 1);
			int pagesize = 16;
			if (check0 == true)
			{
				var model = db.Products.Where(n => n.Price > 0 && n.Price <= 5000000).OrderBy(n => n.Price).ToPagedList(pagenumber, pagesize);
				ViewBag.Check0 = check0;
				return View(model);
			}
			if (check1 == true)
			{
				var model = db.Products.Where(n => n.Price > 5000000 && n.Price <= 10000000).OrderBy(n => n.Price).ToPagedList(pagenumber, pagesize);
				ViewBag.Check1 = check1;
				return View(model);
			}
			if (check2 == true)
			{
				var model = db.Products.Where(n => n.Price > 10000000 && n.Price <= 20000000).OrderBy(n => n.Price).ToPagedList(pagenumber, pagesize);
				ViewBag.Check2 = check2;
				return View(model);
			}
			if (check3 == true)
			{
				var model = db.Products.Where(n => n.Price > 20000000 && n.Price <= 50000000).OrderBy(n => n.Price).ToPagedList(pagenumber, pagesize);
				ViewBag.Check3 = check3;
				return View(model);
			}
			if (check4 == true)
			{
				var model = db.Products.Where(n => n.Price > 50000000).OrderBy(n => n.Price).ToPagedList(pagenumber, pagesize);
				ViewBag.Check4 = check4;
				return View(model);
			}
			return View();

		}

		public JsonResult ListName(string q)
		{
			var data = new ProductDao().ListName(q);
			return Json(new
			{
				data = data,
				status = true
			}, JsonRequestBehavior.AllowGet);
		}

		// Xem tất cả sản phẩm
		public ActionResult ShowProduct(int? page)
		{
			int pagenumber = (page ?? 1);
			int pagesize = 16;
			var model = db.Products.Where(n => n.Discount == 0 || n.EndDate < DateTime.Now || n.StartDate > DateTime.Now).OrderBy(n => n.Name).ToPagedList(pagenumber, pagesize);
			return View(model);
		}

		// Xem tất cả sản phẩm hot
		public ActionResult Hots(int? page)
		{
			int pagenumber = (page ?? 1);
			int pagesize = 16;
			var model = (from a in db.Products
						 join b in db.OrderDetails on a.ProductId equals b.ProductId
						 group b by new { a.Description, a.ProductId, a.Photo, a.Price, a.Discount, a.EndDate, a.StartDate }
						 into g
						 select new ProductView
						 {
							 Description = g.Key.Description,
							 ProductId = g.Key.ProductId,
							 Price = g.Key.Price,
							 Discount = g.Key.Discount,
							 StartDate = g.Key.StartDate,
							 EndDate = g.Key.EndDate,
							 Photo = g.Key.Photo,
							 Quantity = g.Sum(s => s.Quantity),

						 }).OrderByDescending(n => n.Quantity).ToPagedList(pagenumber, pagesize);
			return View(model);
		}

		// Chi tiết sản phẩm + gợi ý
		public async Task<ActionResult> DetailProduct(int? Id)
		{
			if (Id == null)
				return RedirectToAction("Index", "Home");

			var model = new ProductDao().DetailsProduct(Id.Value);
			if (model == null)
			{
				Response.StatusCode = 404;
				return null;
			}

			// Gọi API gợi ý
			var recommendProducts = await GetRecommendations(model.Name);
			ViewBag.RecommendProducts = recommendProducts;

			return View(model);
		}

		private async Task<List<Product>> GetRecommendations(string productName)
		{
			if (string.IsNullOrWhiteSpace(productName))
				return new List<Product>();

			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

				using (var client = new HttpClient(new HttpClientHandler
				{
					ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
				}))
				{
					client.BaseAddress = new Uri("https://fastapi-recommender-product.onrender.com/");
					client.Timeout = TimeSpan.FromSeconds(30);

					var encodedName = Uri.EscapeDataString(productName);


					int maxRetry = 5;
					for (int i = 0; i < maxRetry; i++)
					{
						try
						{
							var response = await client.GetAsync($"recommend/{encodedName}");
							if (response.IsSuccessStatusCode)
							{
								var json = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
								

								var result = JsonConvert.DeserializeObject<RecommendationResponse>(json);

								if (result?.Recommendations == null || result.Recommendations.Count == 0)
									return db.Products.OrderByDescending(p => p.Quantity).Take(5).ToList();

								// Chuẩn hóa tên sản phẩm: lowercase + trim + bỏ khoảng trắng
								var normalizedRecs = result.Recommendations
									.Select(r => r.ToLower().Replace(" ", "").Trim())
									.ToList();

								Console.WriteLine("Normalized Recommendations:");
								normalizedRecs.ForEach(r => Console.WriteLine(r));

								// Lọc DB trên bộ nhớ để tránh EF NotSupportedException
								var recommendProducts = db.Products
									.AsEnumerable()
									.Where(p => normalizedRecs.Any(r => p.Name.ToLower().Replace(" ", "").Contains(r)))
									.ToList();

								// fallback nếu không tìm thấy
								if (!recommendProducts.Any())
									recommendProducts = db.Products.OrderByDescending(p => p.Quantity).Take(5).ToList();

								return recommendProducts;
							}
						}
						catch (HttpRequestException)
						{
							if (i == maxRetry - 1)
								return db.Products.OrderByDescending(p => p.Quantity).Take(5).ToList();

							await Task.Delay(3000);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetRecommendations: {ex.Message}");
			}

			// fallback mặc định
			return db.Products.OrderByDescending(p => p.Quantity).Take(5).ToList();
		}

	}
}
