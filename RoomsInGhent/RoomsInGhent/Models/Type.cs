using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {
    /// <summary>
    /// Type of room
    /// </summary>
    public partial class Type {

        #region - Getters -

        /// <summary>
        /// Returns all types from the database
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetAll() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Types.ToList();
        }

        #endregion

    }
}