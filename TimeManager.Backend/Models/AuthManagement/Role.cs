using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TimeManager.Backend.Models.AuthManagement
{
    public class Role: IdentityRole<int>
    {
        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string? Description { get; set; }
    }
}
