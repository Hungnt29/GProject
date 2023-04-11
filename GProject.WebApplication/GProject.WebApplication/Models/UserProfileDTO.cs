using System.ComponentModel.DataAnnotations;

namespace GProject.WebApplication.Models
{
    public class UserProfileDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public int Sex { get; set; } // 0:Nam || 1: Nữ
        public string Address { get; set; }
        public string Image { get; set; }
        public IFormFile? Image_Upload { get; set; }
    }
}
