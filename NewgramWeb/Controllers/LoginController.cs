using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewgramWeb.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Post(string Senha)
        {

            if (Senha == "px9h2hd3")
            {
                Session["Logado"] = true;
                return RedirectToAction("Index", "Usuarios");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Sair()
        {
            Session["Logado"] = null;
            return RedirectToAction("Index");
        }

    }
}