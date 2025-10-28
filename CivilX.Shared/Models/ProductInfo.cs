using System;

namespace CivilX.Shared.Models
{
    public class ProductInfo
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string RevitVersion { get; set; }
        public string ProductVersion { get; set; }
        public string ActivationStatus { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive => ActivationStatus == "active" || ActivationStatus == "activated";
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.Now;
    }
}
