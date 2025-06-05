namespace EduSyncAPI.Dto
{
    public class UserRegisterDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty; // "Student" or "Instructor"

        //public string PasswordHash { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
