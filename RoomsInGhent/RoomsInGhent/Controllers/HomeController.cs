using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers
{
    public class HomeController : BaseController
    {

        /// <summary>
        /// Number of rooms that are shown in the right sidebar
        /// </summary>
        internal const int HOME_ROOMS = 5;

        //
        // GET: /Home/

        /// <summary>
        /// Shows the homepage
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            // Load info for the user div
            if (Request.IsAuthenticated) {
                KotUser user = KotUser.GetById(User.Identity.Name);

                ViewBag.ReservedCount = user.ReservedCount();

                Room first = user.FirstReserved();
                ViewBag.FirstId = first.ID;
                ViewBag.Since = first.GetLastReservationDateString();
            }

            ViewBag.Schools = School.GetAll();
            ViewBag.Recent = Room.GetFiltered(new FilterObject(), 0, HOME_ROOMS);

            return View();
        }

        /// <summary>
        /// Shows the about us page
        /// </summary>
        /// <returns></returns>
        public ActionResult About() {

            // Load info for the user div
            if (Request.IsAuthenticated) {
                KotUser user = KotUser.GetById(User.Identity.Name);

                ViewBag.ReservedCount = user.ReservedCount();

                Room first = user.FirstReserved();
                ViewBag.FirstId = first.ID;
                ViewBag.Since = first.GetLastReservationDateString();
            }

            ViewBag.Recent = Room.GetFiltered(new FilterObject(), 0, HOME_ROOMS);

            return View();
        }

    }
}
