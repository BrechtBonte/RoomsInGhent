using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data.Linq.SqlClient;

namespace RoomsInGhent.Models {

    /// <summary>
    /// Different categories to sort rooms by
    /// </summary>
    public enum RoomSorts { Grootte, Prijs, Regio, Toegevoegd, Type }

    /// <summary>
    /// Rooms that are advertised on the website
    /// </summary>
    public partial class Room {

        #region - Getters -

        /// <summary>
        /// Gets all the rooms sorted by the date they were added
        /// </summary>
        /// <returns></returns>
        public static List<Room> GetAll() {

            return GetFiltered(new FilterObject());
        }

        /// <summary>
        /// Gets a certain room by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Room GetById(int id) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Rooms.SingleOrDefault(r => r.ID == id);
        }

        #endregion


        #region - Propreties -

        /// <summary>
        /// Gets the name of this rooms type
        /// </summary>
        /// <returns></returns>
        public string GetTypeName() {

            return this.Type.Name;
        }

        /// <summary>
        /// Gets the name of this rooms region
        /// </summary>
        /// <returns></returns>
        public string GetRegion() {

            return this.GentRegion.Name;
        }

        /// <summary>
        /// Checks wether or not this room is reserved
        /// </summary>
        /// <returns></returns>
        public bool IsReserved() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.UserSavesRooms.SingleOrDefault(s => s.RoomId == this.ID && s.DateSaved > DateTime.Now.AddHours(-24)) != null;
        }

        /// <summary>
        /// Gets all attributes this room has
        /// </summary>
        /// <returns></returns>
        public List<RoomHasAttribute> GetAttributes() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.RoomHasAttributes.Where(l => l.RoomID == this.ID).ToList();
        }

        /// <summary>
        /// gets a certain number of attributes
        /// </summary>
        /// <param name="amount">max number of attributes that should be returned</param>
        /// <returns></returns>
        public List<RoomHasAttribute> GetAttributes(int amount) {

            return GetAttributes().Take(amount).ToList();
        }

        /// <summary>
        /// gets an array with the names of this room's attributes
        /// </summary>
        /// <returns></returns>
        public string[] GetAttributeNames() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.RoomHasAttributes.Where(l => l.RoomID == this.ID).Select(a => dbo.Attributes.Single(t => t.ID == a.AttributeId).Name).ToArray();
        }

        #endregion


        #region - Reservations -

        /// <summary>
        /// gets the save objects for this room
        /// </summary>
        /// <returns></returns>
        private List<UserSavesRoom> GetSaves() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.UserSavesRooms.Where(s => s.RoomId == this.ID).ToList();
        }

        /// <summary>
        /// Gets the date for the last reservation of this room formatted in a way javascript can read
        /// </summary>
        /// <returns>return a date in the format 'ddd, dd MMM yyyy HH:mm:ss'</returns>
        public string GetLastReservationDateString() {
            //TODO: unix time string (so firefox can translate)
            DateTime time = GetSaves().OrderByDescending(s => s.DateSaved).Select(s => s.DateSaved).FirstOrDefault();
            if (time == null) return null;
            return time.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds.ToString();
        }

        /// <summary>
        /// check if this room is reserved by a certain user
        /// </summary>
        /// <param name="id">id of the user</param>
        /// <returns></returns>
        public bool ReservedBy(int id) {

            return GetSaves().Count(s => s.UserId == id && s.DateSaved > DateTime.Now.AddHours(-24)) > 0;
        }

        /// <summary>
        /// check if this room is reserved by a certain user
        /// </summary>
        /// <param name="id">string form of the id of the user</param>
        /// <returns></returns>
        public bool ReservedBy(string id) {

            int temp = Convert.ToInt32(id);

            return ReservedBy(temp);
        }

        #endregion


        #region - Images -

        /// <summary>
        /// Gets the rooms image with the lowest index
        /// </summary>
        /// <returns></returns>
        public string GetMainImage() {

            return GetImages().First();
        }

        /// <summary>
        /// Gets all the rooms' images
        /// </summary>
        /// <returns></returns>
        public List<string> GetImages() {

            return this.RoomImages.OrderBy(i => i.Index).Select(i => i.ImageName).ToList();
        }

        #endregion


        #region - Related -

        /// <summary>
        /// Gets all rooms in the same region, sorted by the price difference
        /// </summary>
        /// <returns></returns>
        public List<Room> GetRelated() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Rooms.Where(r => r.RegionId == this.RegionId && r.ID != this.ID).OrderBy(r => Math.Abs(r.Price - this.Price)).ToList();
        }

        #endregion


        #region - Search -
        
        /// <summary>
        /// Gets all rooms according to a filter
        /// </summary>
        /// <param name="filter">filter object used to filter the rooms</param>
        /// <returns></returns>
        public static List<Room> GetFiltered(FilterObject filter) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            var rooms = dbo.Rooms.Where(r => r.ID > -1);

            if (filter.Type.HasValue && filter.Type.Value != -1) rooms = rooms.Where(r => r.TypeId == filter.Type.Value);

            if (filter.MinPrice.HasValue) rooms = rooms.Where(r => r.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue) rooms = rooms.Where(r => r.Price <= filter.MaxPrice.Value);

            if (filter.Included) rooms = rooms.Where(r => r.Included == filter.Included);

            if (filter.MinSize.HasValue) rooms = rooms.Where(r => r.Size >= filter.MinSize.Value);
            if (filter.MaxSize.HasValue) rooms = rooms.Where(r => r.Size <= filter.MaxSize.Value);

            if (filter.Attributes != null && filter.Attributes.Length > 0) {
                rooms = rooms.Where(r => dbo.RoomHasAttributes.Where(a => filter.Attributes.Contains(a.AttributeId)).Select(a => a.RoomID).Contains(r.ID));
            }

            if (filter.Region.HasValue) rooms = rooms.Where(r => r.RegionId == filter.Region.Value);

            if (filter.Query != null && filter.Query != "") {
                foreach (string q in filter.Query.Split(' ')) {
                    rooms = rooms.Where(r =>
                     SqlMethods.Like(r.KotUser.Username, "%" + q + "%") ||
                     SqlMethods.Like(r.Location, "%" + q + "%") ||
                     SqlMethods.Like(r.Decsription, "%" + q + "%") ||
                     SqlMethods.Like(r.Address, "%" + q + "%"));
                }
            }

            if (filter.Sort.HasValue) {
                switch (filter.Sort.Value) {

                    case RoomSorts.Toegevoegd:
                        if (!filter.Reversed) rooms = rooms.OrderByDescending(r => r.Added);
                        else rooms = rooms.OrderBy(r => r.Added);
                        break;

                    case RoomSorts.Prijs:
                        if (!filter.Reversed) rooms = rooms.OrderBy(r => r.Price).ThenBy(r => r.Added);
                        else rooms = rooms.OrderByDescending(r => r.Price).ThenBy(r => r.Added);
                        break;

                    case RoomSorts.Regio:
                        if (!filter.Reversed) rooms = rooms.OrderBy(r => r.GentRegion.Name).ThenBy(r => r.Address);
                        else rooms = rooms.OrderByDescending(r => r.GentRegion.Name).ThenByDescending(r => r.Address);
                        break;

                    case RoomSorts.Grootte:
                        if (!filter.Reversed) rooms = rooms.OrderBy(r => r.Size).ThenBy(r => r.Added);
                        else rooms = rooms.OrderByDescending(r => r.Size).ThenBy(r => r.Added);
                        break;

                    case RoomSorts.Type:
                        if (!filter.Reversed) rooms = rooms.OrderBy(r => r.Type.Name).ThenBy(r => r.Added);
                        else rooms = rooms.OrderByDescending(r => r.Type.Name).ThenBy(r => r.Added);
                        break;
                }
            }


            return rooms.ToList();

        }


        /// <summary>
        /// Gets all rooms according to a filter
        /// </summary>
        /// <param name="filter">filter object used to filter the rooms</param>
        /// <param name="start">amount of rooms to skip</param>
        /// <param name="amount">amount of rooms to return</param>
        /// <returns></returns>
        public static List<Room> GetFiltered(FilterObject filter, int start, int amount) {

            return GetFiltered(filter).Skip(start).Take(amount).ToList();
        }


        /// <summary>
        /// Gets all rooms ordered by their distance from a point
        /// </summary>
        /// <param name="lat">lat of the point</param>
        /// <param name="lng">long of the point</param>
        /// <returns></returns>
        public static List<Room> GetNear(double lat, double lng) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Rooms.OrderBy(r => Math.Sqrt(Math.Pow(r.Lat - lat, 2) + Math.Pow(r.Long - lng, 2))).ToList();
        }

        /// <summary>
        /// Gets all rooms ordered by their distance from a point
        /// </summary>
        /// <param name="lat">lat of the point</param>
        /// <param name="lng">long of the point</param>
        /// <param name="amount">number of rooms to return</param>
        /// <returns></returns>
        public static List<Room> GetNear(double lat, double lng, int amount) {

            return GetNear(lat, lng).Take(amount).ToList();
        }

        #endregion

    }


    #region - Response -

    /// <summary>
    /// Object used to return rooms in REST requests
    /// </summary>
    [Serializable]
    public class RoomResponse {

        private string user;
        private int size;
        private string address;
        private string location;
        private string description;
        private string type;
        private double price;
        private bool included;
        private string email;
        private string telephone;
        private DateTime added;
        private string region;
        private string city;
        private string[] attributes;

        private double lat;
        private double lng;
        private string link;
        private string mainImg;


        public RoomResponse(Room room) {

            user = room.KotUser.Username;
            size = room.Size;
            address = room.Address;
            location = room.Location;
            description = room.Decsription;
            type = room.GetTypeName();
            price = room.Price;
            included = room.Included;
            email = room.Email;
            telephone = room.Telephone;
            added = room.Added;
            region = room.GetRegion();
            city = room.AltCity != null && room.AltCity != "" ? room.AltCity : "9000 Gent";
            attributes = room.GetAttributeNames();
            lat = room.Lat;
            lng = room.Long;
            link = "/Rooms/Detail/" + room.ID;
            mainImg = "/Content/files/imgs/" + room.GetMainImage();
        }

    }

    #endregion
}