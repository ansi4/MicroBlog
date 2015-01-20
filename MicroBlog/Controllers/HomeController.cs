using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MicroBlog.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Users()
		{
			ViewBag.Title = "Users list";
			return View();
		}

		public ActionResult Index()
		{
			ViewBag.Title = "Post list";
			return View();
		}
	}
}
