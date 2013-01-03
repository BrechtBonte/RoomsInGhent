using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers
{
    public class RoomsController : BaseController
    {

        /// <summary>
        /// Number of rooms shown per page
        /// </summary>
        private const int ROOMS_PER = 6;

        //
        // GET: /Rooms/

        /// <summary>
        /// Returns a filtered list of rooms
        /// if none of the parameters are provided, rooms are returned according to the saved filter
        /// </summary>
        /// <param name="typeId">id of the type of room</param>
        /// <param name="minPrice">minimum price</param>
        /// <param name="maxPrice">maximum price</param>
        /// <param name="included">whether or not expenses are included in the price</param>
        /// <param name="minSize">minimum size</param>
        /// <param name="maxSize">maximum size</param>
        /// <param name="attributes">list if id's of attributes that need to be present</param>
        /// <param name="region">id of the region</param>
        /// <param name="query">text used to search</param>
        /// <param name="reserved">whether or not to include reserved rooms</param>
        /// <param name="sort">how the results should be sorted</param>
        /// <param name="reversed">whether or not the results should be reversed</param>
        /// <param name="page">page that should be shown</param>
        /// <param name="ignore">whether or not the saved filter should be ignored</param>
        /// <returns></returns>
        public ActionResult Index(int? typeId, double? minPrice, double? maxPrice, bool? included, int? minSize, int? maxSize, int[] attributes, int? region, string query, bool? reserved, RoomSorts? sort, bool? reversed, int? page, bool? ignore) {

            FilterObject filter;

            if(!typeId.HasValue && !minPrice.HasValue && !maxPrice.HasValue && !included.HasValue && !minSize.HasValue && !maxPrice.HasValue && attributes == null && !region.HasValue && (query == null || query == "") && (!ignore.HasValue || !ignore.Value)) {

                if(Session["filter"] == null) {
                    Session["filter"] = new FilterObject();
                }

                filter = (FilterObject)Session["filter"];
                    
            } else {
                filter = new FilterObject();

                filter.Update(typeId, minPrice, maxPrice, included, minSize, maxSize, attributes, region, query, sort, reversed);
            }

            int p = page.HasValue? page.Value : 1;
            int count = Room.GetFiltered(filter).Count;
            int pages = (count - 1) / ROOMS_PER + 1;

            ViewBag.Page = p;
            ViewBag.LastPage = pages;
            ViewBag.Pages = Enumerable.Range(1, pages).ToList();

            ViewBag.Sort = filter.Sort.HasValue ? filter.Sort.Value : RoomSorts.Toegevoegd;
            ViewBag.Sorts = Enum.GetValues(typeof(RoomSorts));
            ViewBag.Reversed = filter.Reversed;

            ViewBag.Rooms = Room.GetFiltered(filter, (p - 1) * ROOMS_PER, ROOMS_PER);


            return View(filter);
        }

        /// <summary>
        /// Saves or updates the filter, creates a string containing all it's propreties and redirects to show the filtered results
        /// </summary>
        /// <param name="filter">filterobject that should be saved</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(FilterObject filter) {

            Session["filter"] = filter;

            List<string> vars = new List<string>();

            // Url is built manually to facilitate array variables
            if (filter.Type.HasValue && filter.Type.Value != -1) vars.Add(string.Format("{0}={1}", "typeId", filter.Type));
            if (filter.MinPrice.HasValue) vars.Add(string.Format("{0}={1}", "minPrice", filter.MinPrice));
            if (filter.MaxPrice.HasValue) vars.Add(string.Format("{0}={1}", "maxPrice", filter.MaxPrice));
            if (filter.Included) vars.Add(string.Format("{0}={1}", "included", filter.Included));
            if (filter.MinSize.HasValue) vars.Add(string.Format("{0}={1}", "minSize", filter.MinSize));
            if (filter.MaxPrice.HasValue) vars.Add(string.Format("{0}={1}", "maxSize", filter.MaxSize));
            if (filter.Attributes != null && filter.Attributes.Length > 0) {
                foreach (int id in filter.Attributes) {
                    vars.Add(string.Format("{0}={1}", "attributes", id));
                }
            }
            if (filter.Region.HasValue) vars.Add(string.Format("{0}={1}", "region", filter.Region));
            if (filter.Query != null && filter.Query != "") vars.Add(string.Format("{0}={1}", "query", filter.Query));

            return Redirect("~/Rooms/Index?" + string.Join("&", vars));
        }

        /// <summary>
        /// Clears the currently saved filter
        /// </summary>
        /// <returns></returns>
        public ActionResult Clear() {

            Session["filter"] = new FilterObject();

            return RedirectToAction("Index", "Rooms");
        }


        /// <summary>
        /// Shows the detailed view of a room
        /// </summary>
        /// <param name="id">id of the room</param>
        /// <param name="reserve">if the room should be reserved</param>
        /// <param name="unreserve">if the room should be released</param>
        /// <returns></returns>
        public ActionResult Detail(int id, bool? reserve, bool? unreserve) {

            Room room = Room.GetById(id);

            if (room == null) {
                return RedirectToAction("Index", "Error", new { id = (int)RoomsExceptions.NONEXISTENT_ROOM });
            }

            ViewBag.Room = room;

            if (Request.IsAuthenticated) {
                KotUser user = KotUser.GetById(User.Identity.Name);

                if (reserve.HasValue && !unreserve.HasValue && reserve.Value) {
                    try {
                        user.Reserve(id);
                    } catch (RoomsException e) {
                        return RedirectToAction("Index", "Error", new { id = (int)e.Exception });
                    }
                } else if (!reserve.HasValue && unreserve.HasValue && unreserve.Value) {
                    try {
                        user.UnReserve(id);
                    } catch (RoomsException e) {
                        return RedirectToAction("Index", "Error", new { id = (int)e.Exception });
                    }
                }
            }

            return View();
        }

    }

}
