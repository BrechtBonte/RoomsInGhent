using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {
    public partial class Type {

        #region - Getters -

        public static List<Type> GetAll() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Types.ToList();
        }

        #endregion

    }
}