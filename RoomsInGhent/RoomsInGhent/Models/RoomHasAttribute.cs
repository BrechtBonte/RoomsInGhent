using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {
    public partial class RoomHasAttribute {

        public string GetAttributeName() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Attributes.Single(a => a.ID == this.AttributeId).Name;
        }

    }
}