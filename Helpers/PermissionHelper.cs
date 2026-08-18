namespace HotelManagement.Helpers
{
    public static class PermissionHelper
    {
        // Standard actions used across all resources
        public static class Actions
        {
            public const string Read   = "Read";
            public const string Create = "Create";
            public const string Update = "Update";
            public const string Delete = "Delete";
        }

        // Formats a permission name from resource + action
        // e.g. Format("Hotels", Actions.Create) → "Hotels.Create"
        public static string Format(string resource, string action)
            => $"{resource}.{action}";

        // Returns all 4 standard permissions for a resource
        // e.g. GetAll("Hotels") → ["Hotels.Read", "Hotels.Create", "Hotels.Update", "Hotels.Delete"]
        public static IEnumerable<string> GetAll(string resource) =>
        [
            Format(resource, Actions.Read),
            Format(resource, Actions.Create),
            Format(resource, Actions.Update),
            Format(resource, Actions.Delete)
        ];

        // Splits "Hotels.Create" into ("Hotels", "Create")
        public static (string Resource, string Action) Parse(string permission)
        {
            var parts = permission.Split('.', 2);
            return parts.Length == 2
                ? (parts[0], parts[1])
                : (permission, string.Empty);
        }
    }
}
