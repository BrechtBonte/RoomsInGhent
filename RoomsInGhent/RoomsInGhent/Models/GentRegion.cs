using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {

    /// <summary>
    /// Region where a room can be located
    /// </summary>
    public partial class GentRegion {

        #region - Getters -

        /// <summary>
        /// returns all regions, ordered by name
        /// </summary>
        /// <returns></returns>
        public static List<GentRegion> GetAll() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.GentRegions.OrderBy(r => r.Name).ToList();
        }

        #endregion

    }
}