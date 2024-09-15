using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListApp.Data
{
    public class ShoppingProduct
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa produktu jest wymagana.")]
        [Display(Name = "Nazwa produktu")]
        public string ProductName { get; set; }

        [Display(Name = "Czy zaznaczone")]
        public bool IsChecked { get; set; }

   
        public int ShoppingListId { get; set; }
        public ShoppingListItem ShoppingList { get; set; }

       
        public string OwnerId { get; set; } 
        public ApplicationUser Owner { get; set; }
    }
}



