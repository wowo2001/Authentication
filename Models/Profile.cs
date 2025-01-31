namespace Authentication.Models
{
    public class Profile
    {
        public string uuid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string securitytoken { get; set; }
        public string lastTokenUpdateTime { get; set; }
    }
}
