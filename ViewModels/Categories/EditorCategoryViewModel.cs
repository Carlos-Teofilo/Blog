using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Categories
{
    public class EditorCategoryViewModel
    {
        [Required(ErrorMessage = "[name] é obrigatório")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "O campo deve conter no mínimo 3 caracteres e no máximo 40")]
        public string Name { get; set; }

        [Required(ErrorMessage = "[slug] é obrigatório")]
        public string Slug { get; set; }
    }
}
