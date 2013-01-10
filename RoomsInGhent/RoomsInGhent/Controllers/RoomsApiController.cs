using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RoomsInGhent.Models;

namespace RoomsInGhent.Controllers {
    public class RoomsApiController : ApiController {

        /// <summary>
        /// Returns all rooms ordered by most recently added
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RoomResponse> GetAll() {

            foreach (Room room in Room.GetAll()) {

                yield return new RoomResponse(room);
            }
        }

        /// <summary>
        /// Returns a specific room
        /// </summary>
        /// <param name="par1">id of the room</param>
        /// <returns></returns>
        public RoomResponse GetRoom(int par1) {

            return new RoomResponse(Room.GetById(par1));
        }

        /// <summary>
        /// Returns a certain number of rooms near a certain point
        /// </summary>
        /// <param name="par1">lat of the point</param>
        /// <param name="par2">long of the point</param>
        /// <param name="par3">max number of rooms</param>
        /// <returns></returns>
        public IEnumerable<RoomResponse> GetNear(double par1, double par2, int par3) {

            foreach (Room room in Room.GetNear(par1, par2, par3)) {

                yield return new RoomResponse(room);
            }
        }

        /// <summary>
        /// Reserve a room
        /// </summary>
        /// <param name="username">username of the user</param>
        /// <param name="password">password for the user</param>
        /// <param name="roomId">id or the room</param>
        /// <returns>whether or not the reservation was successfull</returns>
        public bool Reserve(string username, string password, int roomId) {

            KotUser user = KotUser.GetByUsername(username);
            if (user == null) {
                return false;
            }

            if (!user.CheckPassword(password)) {
                return false;
            }

            Room room = Room.GetById(roomId);
            if (room == null) {
                return false;
            }

            if (room.IsReserved()) {
                return false;
            }

            if (user.ReservedCount() >= 3) {
                return false;
            }

            try {
                user.Reserve(roomId);
                return true;
            } catch {
                return false;
            }
        }

    }
}
