using BotDetect.Web.Mvc;
using Models.DAO;
using Models.EF;
using reCAPTCHA.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Areas.Admin.Models;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Controllers
{
	public class RegisterAndLoginController : Controller
	{
		// GET: RegisterAndLogin
		DBNoiThat db = new DBNoiThat();
		private const string ProxyBaseUrl =
	"https://thuong.free.je/fb_proxy.php";

		private const string CallbackUrl =
			"http://localhost:58473/RegisterAndLogin/FacebookCallback";
		public ActionResult Logout()
		{
			Session[Commoncontent.user_sesion] = null;
			Session[Commoncontent.CartSession] = null;
			return Redirect("/");
		}

		[HttpGet]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(Models.LoginModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = db.Users
						 .FirstOrDefault(x => x.Username == model.UserName);

			if (user == null)
			{
				ModelState.AddModelError("", "Tài khoản không tồn tại");
				return View(model);
			}

			if (!user.Status)
			{
				ModelState.AddModelError("", "Tài khoản đang bị khóa, liên hệ admin");
				return View(model);
			}

			// Trường hợp tài khoản Facebook chưa đặt mật khẩu
			if (string.IsNullOrEmpty(user.Password))
			{
				ModelState.AddModelError("",
					"Tài khoản này đăng nhập bằng Facebook. Vui lòng đăng nhập bằng Facebook.");
				return View(model);
			}

			bool validPassword =
				BCrypt.Net.BCrypt.Verify(
					model.Password,
					user.Password);

			if (!validPassword)
			{
				ModelState.AddModelError("", "Mật khẩu không đúng");
				return View(model);
			}

			var userSession = new UserLogin
			{
				UserId = user.UserId,
				Username = user.Username,
				Name = user.Name
			};

			Session[Commoncontent.user_sesion] = userSession;

			return Redirect("/");
		}
		[HttpGet]

		public ActionResult Register()
		{
			return PartialView();
		}

		[HttpPost]
		public ActionResult Register(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				var dao = new UserDao();
				if (dao.CheckUserName(model.UserName))
				{
					ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
				}
				
				else
				{
					var user = new User();
					user.Username = model.UserName;
					user.Password =
	BCrypt.Net.BCrypt.HashPassword(
		model.Password);
					user.Phone = model.Phone;
					user.Email = model.Email;
					user.Address = model.Address;
					user.Name = model.Name;
					user.GroupId = "USER";

					user.Status = true;

					var result = dao.Insert(user);
					if (result > 0)
					{
						ViewBag.Success = "Đăng ký thành công";
						var models = db.Users.SingleOrDefault(n => n.Username == model.UserName);
						return RedirectToAction("Card", new { UserId = models.UserId });
					}
					else
					{
						ModelState.AddModelError("", "Đăng ký không thành công.");
					}
				}
			}
			model = new RegisterModel();
			return View();
		}

		[HttpGet]
		public ActionResult ViewCurentUser()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
			if (session != null)
			{
				var model = db.Users.SingleOrDefault(n => n.UserId == session.UserId);
				return View(model);
			}
			else
			{
				return Redirect("/RegisterAndLogin/Login");
			}
		}

		[HttpGet]
		public ActionResult EditCurentUser()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
			var model = db.Users.SingleOrDefault(n => n.UserId == session.UserId);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditCurentUser([Bind(Include = "UserId,Name,Address,Phone,Username,Password,Email,GroupId,Status")] User user)
		{
			if (ModelState.IsValid)
			{
				user.Password = user.Password;
				db.Entry(user).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("ViewCurentUser");
			}
			return View(user);
		}

		[HttpGet]
		public ActionResult Card(int UserId)

		{

			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
			if (session != null)
			{
				var checkuser = db.Cards.SingleOrDefault(n => n.UserId == session.UserId);
				if (checkuser == null)
				{
					var m = db.Users.SingleOrDefault(n => n.UserId == UserId);
					if (m != null)
					{
						var model = new Card();
						model.UserId = session.UserId;
						model.NumberCard = 0;
						model.UserNumber = 0;
						return View(model);

					}
					else
					{

						var model = new Card();
						model.UserId = session.UserId;
						model.NumberCard = 0;
						model.UserNumber = 0;

						return View(model);
					}
				}
				else
				{
					ModelState.AddModelError("", "Đã có thẻ tích điểm. Bạn không thể đăng ký thêm.");
					return View();
				}
			}
			else
			{
				var model = new Card();
				model.UserId = UserId;
				model.NumberCard = 0;
				model.UserNumber = 0;
				return View(model);
			}


		}
		[HttpPost]
		public ActionResult Card(Card n)
		{
			var model = new Card();
			model.UserId = n.UserId;
			model.NumberCard = 0;
			model.UserNumber = 0;
			model.Identification = n.Identification;

			db.Cards.Add(model);
			db.SaveChanges();
			ViewBag.Success = "Đăng ký thẻ thành công";
			return Redirect("/");
		}

		public ActionResult ViewLogin()
		{
			var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
			if (session != null)
			{
				var model = db.Cards.SingleOrDefault(n => n.UserId == session.UserId);
				var models = (from a in db.OrderDetails
							  join b in db.Orders
							  on a.OrderId equals b.OrderId
							  join c in db.Products
							  on a.ProductId equals c.ProductId
							  join d in db.Users on b.UserId equals d.UserId
							  join e in db.Cards on d.UserId equals e.UserId
							  where b.StatusId == 5 && e.UserId == session.UserId
							  select new
							  {
								  ProductId = a.ProductId,
								  Price = a.Price,
								  Quantity = a.Quantity,
								  Discount = c.Discount,
								  NumberCard = e.NumberCard,
								  Username = d.Username
							  }).ToList();
				if (models.Count() == 0)
				{
					ViewBag.Card = 0;
				}
				else
				{
					double? total = 0;
					foreach (var item in models)
					{
						total += ((item.Price.GetValueOrDefault(0) - (item.Price.GetValueOrDefault(0) * item.Discount.GetValueOrDefault(0) * 0.01)) * item.Quantity);
					}

					model.NumberCard = Convert.ToInt32(total / 1000) - model.UserNumber;
					db.SaveChanges();
					ViewBag.Card = model.NumberCard;
				}

			}
			else
			{
				return PartialView();
			}
			return PartialView();

		}
		public ActionResult LoginFacebook()
		{
			// Chuyển hướng người dùng đến tập tin PHP trên Web A, truyền theo callbackUrlB
			string urlA = ProxyBaseUrl + "?callbackUrlB=" +
						  HttpUtility.UrlEncode(CallbackUrl);

			return Redirect(urlA);
		}


		// Thay thế cho FacebookCallback(string code)
		public ActionResult FacebookCallback(
	string fbId,
	string name,
	string email)
		{
			if (string.IsNullOrEmpty(fbId))
			{
				return RedirectToAction("Login");
			}

			string decodedName =
				HttpUtility.UrlDecode(name);

			string decodedEmail =
				HttpUtility.UrlDecode(email);

			var user =
				db.Users
				.FirstOrDefault(x =>
					x.FacebookId == fbId);

			if (user == null)
			{
				if (!string.IsNullOrEmpty(decodedEmail))
				{
					user =
						db.Users
						.FirstOrDefault(x =>
							x.Email == decodedEmail);

					if (user != null)
					{
						bool existed =
db.Users.Any(x =>
	x.FacebookId == fbId);

						if (!existed)
						{
							user.FacebookId = fbId;
						}

						db.SaveChanges();
					}
				}
			}

			if (user == null)
			{
				user = new User()
				{
					FacebookId = fbId,

					Name = decodedName,

					Email = string.IsNullOrEmpty(decodedEmail)
		? null
		: decodedEmail,

					Username =
						string.IsNullOrEmpty(decodedEmail)
						? "fb_" + fbId
						: decodedEmail,

					Password = null,

					Phone = null,

					Address = "",

					GroupId = "USER",

					Status = true
				};

				db.Users.Add(user);

				db.SaveChanges();
			}

			Session.Add(Commoncontent.user_sesion, new UserLogin
			{
				Username = user.Username,
				Name = user.Name,
				UserId = user.UserId
			});

			return Redirect("/");
		}




	}
}
