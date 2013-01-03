using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers {
    public class ErrorController : BaseController {
        //
        // GET: /Error/

        /// <summary>
        /// Shows a specified or unknown error message
        /// </summary>
        /// <param name="id">id of the encountered error</param>
        /// <returns></returns>
        public ActionResult Index(int? id) {

            // Try to parse the exception
            RoomsExceptions? except = null;
            try {
                except = (RoomsExceptions)id;
            } catch {}

            // Fill in data for login div
            if (Request.IsAuthenticated) {
                KotUser user = KotUser.GetById(User.Identity.Name);

                ViewBag.ReservedCount = user.ReservedCount();

                Room first = user.FirstReserved();
                ViewBag.FirstId = first.ID;
                ViewBag.Since = first.GetLastReservationDateString();
            }

            ViewBag.Schools = School.GetAll();
            ViewBag.Recent = Room.GetFiltered(new FilterObject(), 0, HomeController.HOME_ROOMS);


            #region - Generate Error Message -

            // Load error tittle and message
            if (!except.HasValue) {

                ViewBag.Title = "Ongekende fout";
                ViewBag.Message = "Er is een ongekende fout opgetreden.\nWij verontschuldigen ons voor het ongemak";
            } else {

                switch (except.Value) {

                    case RoomsExceptions.ALREAD_RESERVED:
                        ViewBag.Title = "Al gereserveerd";
                        ViewBag.Message = "Het kot dat u probeerde te reserveren is al gereserveerd door een andere gebruiker";
                        break;

                    case RoomsExceptions.NONEXISTENT_ROOM:
                        ViewBag.Title = "Onbestaand kot";
                        ViewBag.Message = "Het kot dat u specifieerde bestaat helaas niet";
                        break;

                    case RoomsExceptions.NONEXISTENT_USER:
                        ViewBag.Title = "Onbestaande gebruiker";
                        ViewBag.Message = "De gebruiker die u specifieerde bestaat helaas niet";
                        break;

                    case RoomsExceptions.RESERVATION_LIMIT:
                        KotUser user = KotUser.GetById(User.Identity.Name);
                        string message = "U heeft helaas al het maximum aantal reservaties bereikt.";
                        if (user != null && user.FirstReserved() != null) {
                            message += "\nWacht aub nog <span class=\"countdown\" data-since=\"" + user.FirstReserved().GetLastReservationDateString() + "\"></span> of laat enkele koten los";
                        }
                        ViewBag.Title = "Limit bereikt";
                        ViewBag.Message = message;
                        break;

                    case RoomsExceptions.ROOM_NOT_RESERVED:
                        ViewBag.Title = "Kot niet gereserveerd";
                        ViewBag.Message = "Het kot dat u probeerde los te laten was nog niet gereserveerd";
                        break;

                    case RoomsExceptions.UNAUTHORIZED_EDIT:
                        ViewBag.Title = "Ongemachtigde uitvoering";
                        ViewBag.Message = "U bent helaas niet gemachtigd om uw gepoogde operatie uit te voeren";
                        break;

                    default:
                        ViewBag.Title = "Ongekende fout";
                        ViewBag.Message = "Er is een ongekende fout opgetreden.\nWij verontschuldigen ons voor het ongemak";
                        break;
                }
            }

            #endregion


            return View();
        }

    }
}
