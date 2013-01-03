using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {

    /// <summary>
    /// Attribute for a room
    /// </summary>
    public partial class Attribute {

        /// <summary>
        /// Returns all attributes
        /// </summary>
        /// <returns></returns>
        public static List<Attribute> GetAll() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Attributes.ToList();
        }

    }
}