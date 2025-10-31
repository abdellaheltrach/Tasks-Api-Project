namespace LoginApp.DataAccess.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresDate { get; set; }
        public bool IsCanceled { get; set; } = false;

        public string DeviceName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;

        public bool IsActive => !IsCanceled && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= ExpiresDate;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

}
