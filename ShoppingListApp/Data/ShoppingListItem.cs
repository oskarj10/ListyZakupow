using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListApp.Data
{
    public class ShoppingListItem
    {
        public int Id { get; set; }
        public string ListName { get; set; } = string.Empty;
        public DateTime ShoppingDate { get; set; }
        public string? Description { get; set; }
        public bool IsChecked { get; set; }
        public List<ShoppingProduct> Products { get; set; } = new List<ShoppingProduct>();
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        public ShoppingListItem()
        {
        }
    }

    public class FutureOrPresentDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime date = (DateTime)value;
            return date >= DateTime.Today;
        }
    }
}











