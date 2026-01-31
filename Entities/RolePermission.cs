namespace AuthSystemAPI.Entities
{
    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public ApplicationRole Role { get; set; }
        public Permission Permission { get; set; }
    }
}
