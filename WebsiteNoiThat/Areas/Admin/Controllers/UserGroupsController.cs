using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Models.EF;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class UserGroupsController : Controller
    {
        private DBNoiThat db = new DBNoiThat();

        // GET: Admin/UserGroups
        [HasCredential(RoleId = "VIEW_GROUP")]
        public ActionResult Index()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            return View(db.UserGroups.ToList());
        }

        // GET: Admin/UserGroups/Details/5
        [HasCredential(RoleId = "VIEW_GROUP")]
        public ActionResult Details(string id)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserGroup userGroup = db.UserGroups.Find(id);
            if (userGroup == null)
            {
                return HttpNotFound();
            }
            return View(userGroup);
        }

      

       
		[HasCredential(RoleId = "EDIT_GROUP")]
		public ActionResult Edit(string id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var userGroup = db.UserGroups.Find(id);
			if (userGroup == null)
				return HttpNotFound();

			ViewBag.GroupName = userGroup.Name;

			var users = db.Users.Where(x => x.GroupId == id).ToList();
			return View("~/Areas/Admin/Views/User/Show.cshtml", users);
		}

		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
		public ActionResult UsersByGroup(string id)
		{
			var users = db.Users.Where(x => x.GroupId == id).ToList();
			return PartialView("_UsersByGroup", users);
		}

	}
}
