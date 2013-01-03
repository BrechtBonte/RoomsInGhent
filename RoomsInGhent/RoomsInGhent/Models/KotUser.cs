using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;

namespace RoomsInGhent.Models {

    /// <summary>
    /// User of the website
    /// </summary>
    [MetadataType(typeof(KotUserMD))]
    public partial class KotUser {

        #region - Getters -

        /// <summary>
        /// Gets a user by it's id
        /// </summary>
        /// <param name="id">id of the user</param>
        /// <returns></returns>
        public static KotUser GetById(int id) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.KotUsers.SingleOrDefault(u => u.ID == id);
        }

        /// <summary>
        /// Gets a user by the string form of it's id
        /// </summary>
        /// <param name="id">string form of the user's id</param>
        /// <returns></returns>
        public static KotUser GetById(string id) {

            return GetById(Convert.ToInt32(id));
        }

        /// <summary>
        /// Gets a user by it's username
        /// </summary>
        /// <param name="name">username of the user</param>
        /// <returns></returns>
        public static KotUser GetByUsername(string name) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.KotUsers.SingleOrDefault(u => u.Username == name);
        }

        #endregion


        #region - Registration -

        /// <summary>
        /// Checks if a username is taken
        /// </summary>
        /// <param name="name">name that should be checked</param>
        /// <returns></returns>
        public static bool UsernameTaken(string name) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.KotUsers.SingleOrDefault(u => u.Username == name) != null;
        }

        /// <summary>
        /// registers a user
        /// </summary>
        /// <param name="reg">registration object</param>
        /// <returns></returns>
        public static KotUser RegisterUser(RegistrationUser reg) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            string salt;
            string pass;

            do {
                KeyValuePair<string, string> pair = Encrypt(reg.Password);
                salt = pair.Key;
                pass = pair.Value;
            } while(dbo.KotUsers.Count(u => u.Salt == salt) > 0);

            KotUser user = new KotUser();
            user.Username = reg.Username;
            user.Salt = salt;
            user.Password = pass;
            user.Email = reg.Email == ""? null : reg.Email;
            user.Phone = reg.Phone == ""? null : reg.Phone;

            dbo.KotUsers.InsertOnSubmit(user);
            dbo.SubmitChanges();

            return user;
        }

        #endregion


        #region - Reservations -

        /// <summary>
        /// Max number of reservations
        /// </summary>
        private const int MAX_RESERVATIONS = 3;

        /// <summary>
        /// returns all reservations
        /// </summary>
        /// <returns></returns>
        public List<UserSavesRoom> GetReservations() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.UserSavesRooms.Where(s => s.UserId == this.ID && s.DateSaved > DateTime.Now.AddHours(-24)).ToList();
        }

        /// <summary>
        /// Returns a certain reservation according to the room's id
        /// </summary>
        /// <param name="roomId">id of the room</param>
        /// <returns></returns>
        public UserSavesRoom GetReservation(int roomId) {

            return GetReservations().SingleOrDefault(r => r.RoomId == roomId);
        }

        /// <summary>
        /// Get all reserved rooms of the user
        /// </summary>
        /// <returns></returns>
        public List<Room> GetReserved() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            List<Room> list = new List<Room>();

            foreach (UserSavesRoom save in GetReservations().OrderByDescending(s => s.DateSaved)) {

                list.Add(dbo.Rooms.SingleOrDefault(r => r.ID == save.RoomId));
            }

            return list;
        }

        /// <summary>
        /// Gets the first reserved room of the user
        /// </summary>
        /// <returns></returns>
        public Room FirstReserved() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return GetReservations().OrderBy(r => r.DateSaved).Select(r => dbo.Rooms.Single(m => m.ID == r.RoomId)).FirstOrDefault();
        }

        /// <summary>
        /// Counts the users reserved rooms
        /// </summary>
        /// <returns></returns>
        public int ReservedCount() {

            return GetReservations().Count;
        }

        /// <summary>
        /// Checks if the user has reserved a certain room
        /// </summary>
        /// <param name="roomId">id of the room</param>
        /// <returns></returns>
        public bool HasReserved(int roomId) {

            return GetReserved().SingleOrDefault(r => r.ID == roomId) != null;
        }

        /// <summary>
        /// Reserves a room
        /// </summary>
        /// <param name="roomId">id of the room</param>
        public void Reserve(int roomId) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            if (Room.GetById(roomId) == null) {
                throw new RoomsException(RoomsExceptions.NONEXISTENT_ROOM);
            }

            if (Room.GetById(roomId).IsReserved()) {
                throw new RoomsException(RoomsExceptions.ALREAD_RESERVED);
            }

            if (GetReservations().Count >= MAX_RESERVATIONS) {
                throw new RoomsException(RoomsExceptions.RESERVATION_LIMIT);
            }

            UserSavesRoom save = new UserSavesRoom();
            save.UserId = this.ID;
            save.RoomId = roomId;
            save.DateSaved = DateTime.Now;

            dbo.UserSavesRooms.InsertOnSubmit(save);
            dbo.SubmitChanges();
        }

        /// <summary>
        /// Releases a room
        /// </summary>
        /// <param name="roomId">id of the room</param>
        public void UnReserve(int roomId) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            Room room = Room.GetById(roomId);

            if (room == null) {
                throw new RoomsException(RoomsExceptions.NONEXISTENT_ROOM);
            }

            if (!room.ReservedBy(this.ID)) {
                throw new RoomsException(RoomsExceptions.ROOM_NOT_RESERVED);
            }

            UserSavesRoom save = dbo.UserSavesRooms.Where(s => s.UserId == this.ID && s.RoomId == roomId).OrderByDescending(s => s.DateSaved).First();
            dbo.UserSavesRooms.DeleteOnSubmit(save);
            dbo.SubmitChanges();
        }

        #endregion


        #region - Rooms -

        /// <summary>
        /// Gets all rooms this user offers, ordered by their price difference with a give price
        /// </summary>
        /// <param name="id">id of the room that should be omitted</param>
        /// <param name="price">price to compare the rooms' prices with</param>
        /// <returns></returns>
        public List<Room> GetRooms(int id, double price) {

            return this.Rooms.Where(r => r.ID != id).OrderBy(r => Math.Abs(r.Price - price)).ToList();
        }

        /// <summary>
        /// Gets all rooms sorted by a given sort type
        /// </summary>
        /// <param name="sort">sort type</param>
        /// <param name="reverse">wehther or not the results should be reversed</param>
        /// <returns></returns>
        public List<Room> GetRooms(RoomSorts sort, bool reverse) {

            switch(sort) {

                case RoomSorts.Toegevoegd:
                    if(!reverse) return this.Rooms.OrderByDescending(r => r.Added).ToList();
                    else return this.Rooms.OrderBy(r => r.Added).ToList();
                
                case RoomSorts.Prijs:
                    if(!reverse) return this.Rooms.OrderBy(r => r.Price).ThenBy(r => r.Added).ToList();
                    else return this.Rooms.OrderByDescending(r => r.Price).ThenBy(r => r.Added).ToList();

                case RoomSorts.Regio:
                    if(!reverse) return this.Rooms.OrderBy(r => r.GentRegion.Name).ThenBy(r => r.Address).ToList();
                    else return this.Rooms.OrderByDescending(r => r.GentRegion.Name).ThenByDescending(r => r.Address).ToList();

                case RoomSorts.Grootte:
                    if(!reverse) return this.Rooms.OrderBy(r => r.Size).ThenBy(r => r.Added).ToList();
                    else return this.Rooms.OrderByDescending(r => r.Size).ThenBy(r => r.Added).ToList();

                case RoomSorts.Type:
                    if(!reverse) return this.Rooms.OrderBy(r => r.Type.Name).ThenBy(r => r.Added).ToList();
                    else return this.Rooms.OrderByDescending(r => r.Type.Name).ThenBy(r => r.Added).ToList();

                default:
                    return new List<Room>();
            }
        }

        /// <summary>
        /// Gets all rooms sorted by a given sort type
        /// </summary>
        /// <param name="sort">sort type</param>
        /// <param name="reverse">wehther or not the results should be reversed</param>
        /// <param name="start">how many rooms should be skipped</param>
        /// <param name="amount">how many rooms should be returned</param>
        /// <returns></returns>
        public List<Room> GetRooms(RoomSorts sort, bool reverse, int start, int amount) {

            return GetRooms(sort, reverse).Skip(start).Take(amount).ToList();
        }

        #endregion


        #region - Authentication -

        /// <summary>
        /// number of bytes user for the salt
        /// </summary>
        private const int SALT_BYTES = 24;

        /// <summary>
        /// number of bytes user for the hash
        /// </summary>
        private const int HASH_Bytes = 24;

        /// <summary>
        /// Number of iterations for encryption
        /// </summary>
        private const int ITERATIONS = 1000;


        /// <summary>
        /// generate a salt and hashed string for a password
        /// </summary>
        /// <param name="password">password that should be encrypted</param>
        /// <returns>KeyValuePair where the key is the salt, and the value the hashed string</returns>
        private static KeyValuePair<string, string> Encrypt(string password) {

            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SALT_BYTES];
            csprng.GetBytes(salt);

            string saltStr = Convert.ToBase64String(salt);

            return new KeyValuePair<string, string>(saltStr, Encrypt(saltStr, password));
        }

        /// <summary>
        /// encrypts a password with a given salt
        /// </summary>
        /// <param name="salt">salt used to hash the password</param>
        /// <param name="password">password that should be encrypted</param>
        /// <returns>hashed and salted string</returns>
        private static string Encrypt(string salt, string password) {

            Rfc2898DeriveBytes passKey = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(salt), ITERATIONS);
            byte[] hash = passKey.GetBytes(HASH_Bytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// checks if this user's password is right
        /// </summary>
        /// <param name="password">password to check</param>
        /// <returns></returns>
        public bool CheckPassword(string password) {

            string pass = Encrypt(this.Salt, password);
            return this.Password == pass;
        }

        #endregion


        #region - Propreties -

        /// <summary>
        /// updates the user's personal info
        /// </summary>
        /// <param name="email">new email for the user</param>
        /// <param name="phone">new phone for the user</param>
        public void UpdateInfo(string email, string phone) {

            DataClassesDataContext dbo = new DataClassesDataContext();

            KotUser user = dbo.KotUsers.Single(u => u.ID == this.ID);
            user.Email = email == "" ? null : email;
            user.Phone = phone == "" ? null : phone;

            dbo.SubmitChanges();
        }

        #endregion

    }


    #region - Model -

    /// <summary>
    /// Model for the user
    /// </summary>
    public class KotUserMD {

        [Display(Name = "Gebruikersnaam:")]
        [Required(ErrorMessage = "Gelieve een gebruikersnaam in te geven")]
        public object Username;

        [Display(Name = "Wachtwoord:")]
        [Required(ErrorMessage = "Gelieve een Wachtwoord in te geven")]
        public object Password;

        [Display(Name = "Email adres:")]
        [RegularExpression(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}", ErrorMessage = "Geef aub een geldig email adres")]
        [StringLength(50, ErrorMessage = "Beperk uw email adres tot 50 characters aub")]
        public object Email;

        [Display(Name = "Telefoon nummer:")]
        [StringLength(50, ErrorMessage = "Beperk uw telefoon nummer tot 50 characters aub")]
        public object Phone;
    }

    #endregion


    #region - Registration -

    /// <summary>
    /// Registration model for the user
    /// </summary>
    public class RegistrationUser {

        [Display(Name = "Gebruikersnaam: *")]
        [Required(ErrorMessage = "Vul aub een gebruikersnaam in")]
        [StringLength(50, ErrorMessage = "Beperk uw gebruikersnaam tot 50 characters aub")]
        public string Username { get; set; }

        [Display(Name = "Wachtwoord: *")]
        [Required(ErrorMessage = "Vul aub een wachtwoord in")]
        [StringLength(50, ErrorMessage = "Beperk uw wachtwoord tot 50 characters aub")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Opnieuw: *")]
        [Required(ErrorMessage = "Vul aub het wachtwoord opnieuw in")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "De ingevulde wachtwoorden komen niet overeen")]
        public string RePassword { get; set; }

        [Display(Name = "Email adres:")]
        [RegularExpression(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}", ErrorMessage = "Geef aub een geldig email adres")]
        [StringLength(50, ErrorMessage = "Beperk uw email adres tot 50 characters aub")]
        public string Email { get; set; }

        [Display(Name = "Telefoon nummer:")]
        [StringLength(50, ErrorMessage = "Beperk uw telefoon nummer tot 50 characters aub")]
        public string Phone { get; set; }

    }

    #endregion
}