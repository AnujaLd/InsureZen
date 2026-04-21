using System;
using System.Collections.Generic;
using InsureZen.Core.Enums;

namespace InsureZen.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        
        // Navigation properties - make them nullable
        public virtual ICollection<MakerReview>? MakerReviews { get; set; }
        public virtual ICollection<CheckerReview>? CheckerReviews { get; set; }
    }
}