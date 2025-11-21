namespace Manisik.Enums
{
    // Application roles used for authorization and seeding
    public enum Role
    {
        Admin,
        User,
        HotelManager,
        TransportManager
    }

    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string HotelManager = "HotelManager";
        public const string TransportManager = "TransportManager";

        public static readonly string[] All = new[] { Admin, User, HotelManager, TransportManager };
    }
}
