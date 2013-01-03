using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoomsInGhent.Models {
    public partial class School {

        public static List<School> GetAll() {

            DataClassesDataContext dbo = new DataClassesDataContext();

            return dbo.Schools.OrderBy(s => s.Name).ToList();
        }

    }
}