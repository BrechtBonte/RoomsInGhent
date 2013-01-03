using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RoomsInGhent.Models {

    /// <summary>
    /// Object used to save filter settings
    /// </summary>
    [Serializable]
    public class FilterObject {

        #region - Properties -

        [Display(Name = "Type")]
        public int? Type { get; set; }

        [Display(Name = "Min Prijs")]
        public double? MinPrice { get; set; }
        [Display(Name = "Max Prijs")]
        public double? MaxPrice { get; set; }
        
        [Display(Name = "Kosten Inbegrepen")]
        public bool Included { get; set; }

        [Display(Name = "Min Grootte")]
        public int? MinSize { get; set; }
        [Display(Name = "Max Grootte")]
        public int? MaxSize { get; set; }

        [Display(Name = "Eigenschappen")]
        public int[] Attributes { get; set; }

        [Display(Name = "Regio")]
        public int? Region { get; set; }

        [Display(Name = "Trefwoorden")]
        public string Query { get; set; }

        public RoomSorts? Sort { get; set; }
        public bool Reversed { get; set; }

        public List<SelectListItem> TypeList { get; private set; }
        public List<SelectListItem> AttrList { get; private set; }

        #endregion


        #region - Methods -

        public FilterObject() {
            Included = false;
            Reversed = false;
            Attributes = new int[0];

            RefreshList();
        }

        private void RefreshList() {
            AttrList = new List<SelectListItem>();
            TypeList = new List<SelectListItem>();

            foreach (Attribute attr in Attribute.GetAll()) {
                SelectListItem item = new SelectListItem();
                item.Selected = Attributes.Contains(attr.ID);
                item.Text = attr.Name;
                item.Value = attr.ID.ToString();
                AttrList.Add(item);
            }

            SelectListItem temp = new SelectListItem();
            temp.Selected = !Type.HasValue;
            temp.Text = "Alle";
            temp.Value = "-1";
            TypeList.Add(temp);

            foreach (Type type in Models.Type.GetAll()) {
                SelectListItem item = new SelectListItem();
                item.Selected = Type.HasValue && Type.Value == type.ID;
                item.Text = type.Name;
                item.Value = type.ID.ToString();
                TypeList.Add(item);
            }
        }

        public void Update(int? type, double? minPrice, double? maxPrice, bool? included, int? minSize, int? maxSize, int[] attributes, int? region, string query, RoomSorts? sort, bool? reversed) {

            if (type.HasValue) this.Type = type;
            if(minPrice.HasValue) this.MinPrice = minPrice;
            if(maxPrice.HasValue) this.MaxPrice = maxPrice;
            if(included.HasValue) this.Included = included.Value;
            if(minSize.HasValue) this.MinSize = minSize;
            if(maxSize.HasValue) this.MaxSize = maxSize;
            if(attributes != null) this.Attributes = attributes;
            if(region.HasValue) this.Region = region;
            if(query != null && query != "") this.Query = query;
            if(sort.HasValue) this.Sort = sort;
            if(reversed.HasValue) this.Reversed = reversed.Value;

            RefreshList();
        }

        #endregion

    }
}