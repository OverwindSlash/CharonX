using System.ComponentModel.DataAnnotations;

namespace CharonX.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}