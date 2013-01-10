using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {
    /// <summary>
    /// object linking a school campus to its address
    /// </summary>
    public partial class School {

        /// <summary>
        /// Returns all schools in the database
        /// </summary>
        /// <returns></returns>
        public static List<School> GetAll() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Schools.OrderBy(s => s.Name).ToList();
        }

    }
}