using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers
{
    public class UsersController : BaseController
    {

        /// <summary>
        /// Number of rooms per page
        /// </summary>
        private const int ROOMS_PER = 5;

        //
        // GET: /Users/

        /// <summary>
        /// Shows the datail view of a certain user
        /// </summary>
        /// <param name="id">id of the user</param>
        /// <param name="page">page number for the user's advertised rooms</param>
        /// <param name="sort">sort type for advertised rooms</param>
        /// <param name="reversed">whether or not advertised rooms are reversed</param>
        /// <returns></returns>
        public ActionResult Index(int id, int? page, RoomSorts? sort, bool? reversed)
        {

            KotUser user = KotUser.GetById(id);

            if (user == null) {
                return RedirectToAction("Index", "Error", new { id = (int)RoomsExceptions.NONEXISTENT_USER });
            }
            
            /* Parameter checking */
            if (!sort.HasValue) sort = RoomSorts.Toegevoegd;
            if (!reversed.HasValue) reversed = false;
            int pages = (user.GetRooms(sort.Value, reversed.Value).Count - 1) / ROOMS_PER + 1;
            if (pages < 1) pages = 1;
            if (!page.HasValue || page.Value < 1 || page.Value > pages) page = 1;

            /* Content */
            ViewBag.User = user;
            ViewBag.ReservedRooms = user.GetReserved();
            ViewBag.Advertised = user.GetRooms(sort.Value, reversed.Value, (page.Value - 1) * ROOMS_PER, ROOMS_PER);

            /* Pagination */
            ViewBag.Page = page.Value;
            ViewBag.LastPage = pages;
            ViewBag.Pages = Enumerable.Range(1, pages).ToList();

            /* Sorting */
            ViewBag.Sort = sort.Value;
            ViewBag.Reversed = reversed.Value;
            ViewBag.Sorts = Enum.GetValues(typeof(RoomSorts));

            ViewBag.Edit = false;

            return View();
        }

        /// <summary>
        /// View to edit the personal info
        /// </summary>
        /// <param name="id">id of the user</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Edit(int id) {

            KotUser user = KotUser.GetById(id);

            if (user == null) {
                return RedirectToAction("Index", "Error", new { id = (int)RoomsExceptions.NONEXISTENT_USER });
            }

            if (!Request.IsAuthenticated || id.ToString() != User.Identity.Name) {
                return RedirectToAction("Index", "Error", new { id = (int)RoomsExceptions.UNAUTHORIZED_EDIT });
            }


            /* Content */
            ViewBag.User = user;
            ViewBag.ReservedRooms = user.GetReserved();
            ViewBag.Advertised = user.GetRooms(RoomSorts.Toegevoegd, false, 0, ROOMS_PER);

            /* Pagination */
            int pages = (user.GetRooms(RoomSorts.Toegevoegd, false).Count - 1) / ROOMS_PER + 1;
            ViewBag.Page = 1;
            ViewBag.LastPage = pages;
            ViewBag.Pages = Enumerable.Range(1, pages).ToList();

            /* Sorting */
            ViewBag.Sort = RoomSorts.Toegevoegd;
            ViewBag.Reversed = false;
            ViewBag.Sorts = Enum.GetValues(typeof(RoomSorts));


            ViewBag.Edit = true;

            return View("Index", user);
        }

        /// <summary>
        /// Handle edit of the users personal info
        /// </summary>
        /// <param name="usr">modified user</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult Edit(KotUser usr) {

            KotUser user = KotUser.GetById(usr.ID);

            if (user == null) {
                return RedirectToAction("Index", "Error", new { id = (int)RoomsExceptions.NONEXISTENT_USER });
            }

            if (!Request.IsAuthenticated || usr.ID.ToString() != User.Identity.Name) {
                return RedirectToAction("Index", "Error", new { id = (int)RoomsExceptions.UNAUTHORIZED_EDIT });
            }

            if (ModelState.IsValidField("Email") && ModelState.IsValidField("Phone")) {

                user.UpdateInfo(usr.Email, usr.Phone);

                return RedirectToAction("Index", "Users", new { id = usr.ID });
            }

            ViewBag.Edit = true;


            /* Content */
            ViewBag.User = user;
            ViewBag.ReservedRooms = user.GetReserved();
            ViewBag.Advertised = user.GetRooms(RoomSorts.Toegevoegd, false, 0, ROOMS_PER);

            /* Pagination */
            int pages = (user.GetRooms(RoomSorts.Toegevoegd, false).Count - 1) / ROOMS_PER + 1;
            ViewBag.Page = 1;
            ViewBag.LastPage = pages;
            ViewBag.Pages = Enumerable.Range(1, pages).ToList();

            /* Sorting */
            ViewBag.Sort = RoomSorts.Toegevoegd;
            ViewBag.Reversed = false;
            ViewBag.Sorts = Enum.GetValues(typeof(RoomSorts));

            return View("Index", user);

        }

    }
}
