using Models.EF;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Controllers;
using WebsiteNoiThat.Models;



namespace WenoithatTest.Test.Controllers
{
	[TestFixture]
	public class HomeControllerTests
	{

		private HomeController _controller;
		private Mock<HttpContextBase> _mockHttpContext;
		private Mock<HttpSessionStateBase> _mockSession;

		[SetUp]
		public void Setup()
		{
			_controller = new HomeController();

			_mockHttpContext = new Mock<HttpContextBase>();
			_mockSession = new Mock<HttpSessionStateBase>();

			_mockHttpContext.Setup(ctx => ctx.Session).Returns(_mockSession.Object);

			_controller.ControllerContext = new ControllerContext(_mockHttpContext.Object, new System.Web.Routing.RouteData(), _controller);
		}

		[Test]
		public void Index_Returns_ViewResult()
		{
			var result = _controller.Index() as ViewResult;
			Assert.That(result, Is.Not.Null);
		}
		[Test]
		public void HeaderCart_WhenSessionIsNull_ReturnsEmptyList()
		{
			// Session[CartSession] trả về null
			_mockSession.Setup(s => s[Commoncontent.CartSession]).Returns(null);

			var result = _controller.HeaderCart() as PartialViewResult;

			Assert.That(result, Is.Not.Null);

			var model = result.Model as List<CartItem>;
			Assert.That(model, Is.Not.Null);
			Assert.That(model.Count, Is.EqualTo(0));
		}
		[Test]
		public void HeaderCart_WhenSessionHasCart_ReturnsListFromSession()
		{
			// Arrange: Tạo dữ liệu giỏ hàng giả lập với thông tin từ class Product thật
			var cartItems = new List<CartItem>
	{
		new CartItem
		{
			Product = new Product
			{
				ProductId = 101,
				Name = "Bàn làm việc",
				Description = "Bàn gỗ công nghiệp",
				Price = 1500000,
				Quantity = 10,
				ProviderId = 1,
				CateId = 2,
				Photo = "ban-lam-viec.jpg",
				StartDate = new DateTime(2025, 10, 1),
				EndDate = new DateTime(2025, 12, 31),
				Discount = 10
			},
			Quantity = 2,
			Status = true
		}
	};

			_mockSession.Setup(s => s[Commoncontent.CartSession]).Returns(cartItems);

			// Act
			var result = _controller.HeaderCart() as PartialViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			var model = result.Model as List<CartItem>;
			Assert.That(model, Is.Not.Null);
			Assert.That(model.Count, Is.EqualTo(1));

			var item = model[0];
			var product = item.Product;

			// Kiểm tra CartItem
			Assert.That(item.Quantity, Is.EqualTo(2));
			Assert.That(item.Status, Is.True);

			// Kiểm tra thông tin Product
			Assert.That(product.ProductId, Is.EqualTo(101));
			Assert.That(product.Name, Is.EqualTo("Bàn làm việc"));
			Assert.That(product.Description, Is.EqualTo("Bàn gỗ công nghiệp"));
			Assert.That(product.Price, Is.EqualTo(1500000));
			Assert.That(product.Quantity, Is.EqualTo(10));
			Assert.That(product.ProviderId, Is.EqualTo(1));
			Assert.That(product.CateId, Is.EqualTo(2));
			Assert.That(product.Photo, Is.EqualTo("ban-lam-viec.jpg"));
			Assert.That(product.StartDate, Is.EqualTo(new DateTime(2025, 10, 1)));
			Assert.That(product.EndDate, Is.EqualTo(new DateTime(2025, 12, 31)));
			Assert.That(product.Discount, Is.EqualTo(10));
		}





	}
}