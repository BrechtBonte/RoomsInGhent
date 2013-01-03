using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers
{
    public class BaseController : Controller
    {

        /// <summary>
        /// Sizes that are advertised in the main nav bar
        /// </summary>
        public Dictionary<int, int> SIZES = new Dictionary<int, int>() {
            { -1, 10 },
            { 10, 20 },
            { 20, 30 },
            { 30, 40 },
            { 40, 50 },
            { 50, 60 },
            { 60, -1 }
        };

        /// <summary>
        /// Prices that are advertised in the main nav bar
        /// </summary>
        public Dictionary<int, int> PRICES = new Dictionary<int, int>() {
            { -1, 100 },
            { 100, 200 },
            { 200, 300},
            { 300, 400 },
            { 400, 500 },
            { 500, 600 },
            {600, -1 }
        };

        /// <summary>
        /// Override to make sure all shown views have the generally needed variables
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnResultExecuting(ResultExecutingContext filterContext) {

            ViewBag.Types = Models.Type.GetAll();
            ViewBag.Sizes = SIZES;
            ViewBag.Prices = PRICES;
            ViewBag.Regions = GentRegion.GetAll();

            base.OnResultExecuting(filterContext);
        }
    }
}
