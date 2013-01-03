using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {

    public enum RoomsExceptions { NONEXISTENT_ROOM, ALREAD_RESERVED, RESERVATION_LIMIT, ROOM_NOT_RESERVED, NONEXISTENT_USER, UNAUTHORIZED_EDIT }

    public class RoomsException : Exception {

        private RoomsExceptions ex;
        public RoomsExceptions Exception { get { return ex; } }

        public RoomsException(RoomsExceptions except) : base() {

            this.ex = except;
        }

    }
}