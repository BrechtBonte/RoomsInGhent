using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers
{
    public class LoginController : BaseController
    {
        //
        // GET: /Login/

        /// <summary>
        /// Loads the empty login screen
        /// </summary>
        /// <returns></returns>
        public ActionResult Index() {

            if (Request.IsAuthenticated) return RedirectToAction("Index", "Users", new { id = User.Identity.Name });

            ViewBag.NonExistent = false;
            ViewBag.WrongPass = false;

            return View();
        }

        /// <summary>
        /// Handles a submitted login form
        /// </summary>
        /// <param name="usr">user who's info has been submitted</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(KotUser usr) {

            if (Request.IsAuthenticated) return RedirectToAction("Index", "Users", new { id = User.Identity.Name });

            ViewBag.NonExistent = false;
            ViewBag.WrongPass = false;

            if (ModelState.IsValid) {

                // See if username exists
                KotUser user = KotUser.GetByUsername(usr.Username);

                if (user == null) {

                    ViewBag.NonExistent = true;
                } else {

                    // See if password is correct
                    if (!user.CheckPassword(usr.Password)) {

                        ViewBag.WrongPass = true;
                    } else {

                        // Log user in
                        FormsAuthentication.SetAuthCookie(user.ID.ToString(), false);

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return View(usr);
        }

        /// <summary>
        /// Shows the empty registration form
        /// </summary>
        /// <returns></returns>
        public ActionResult Register() {

            if (Request.IsAuthenticated) return RedirectToAction("Index", "Users", new { id = User.Identity.Name });

            ViewBag.Taken = false;

            return View();
        }

        /// <summary>
        /// Handles a submitted registration form
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Register(RegistrationUser reg) {

            if (Request.IsAuthenticated) return RedirectToAction("Index", "Users", new { id = User.Identity.Name });

            ViewBag.Taken = false;

            if (ModelState.IsValid) {

                if (KotUser.UsernameTaken(reg.Username)) {

                    ViewBag.Taken = true;
                } else {

                    KotUser user = KotUser.RegisterUser(reg);
                    FormsAuthentication.SetAuthCookie(user.ID.ToString(), false);

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(reg);
        }

        public ActionResult Logout() {

            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

    }
}
