using System;

namespace CivilX.Shared.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string CreatedAt { get; set; }
        
        // Свойства для совместимости с существующим кодом
        public string Name => FullName ?? Username;
        public string Login => Username;
        public string UserType => "user";
        public string CompanyName => "";
        public string Phone => "";
    }
}
