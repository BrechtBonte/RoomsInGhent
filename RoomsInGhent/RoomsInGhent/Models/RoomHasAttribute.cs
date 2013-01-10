using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {
    /// <summary>
    /// Object linking a room to an attribute
    /// </summary>
    public partial class RoomHasAttribute {

        /// <summary>
        /// Gets the name of an attribute
        /// </summary>
        /// <returns></returns>
        public string GetAttributeName() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Attributes.Single(a => a.ID == this.AttributeId).Name;
        }

    }
}