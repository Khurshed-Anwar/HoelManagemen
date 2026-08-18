namespace HotelManagement.Services.Authorization
{
    public interface IResourcePermissionGenerator
    {
        // Generates Read, Create, Update, Delete permissions for a resource
        // Skips any that already exist in DB
        // e.g. GenerateAsync("Bookings") creates: Bookings.Read, Bookings.Create, Bookings.Update, Bookings.Delete
        // Returns the list of newly created permission names (skips existing ones)
        Task<IList<string>> GenerateAsync(string resourceName);

        // Returns all distinct resource names that have permissions in DB
        // e.g. ["Countries", "Hotels", "Bookings"]
        Task<IList<string>> GetAllResourcesAsync();

        // Ensures all 4 standard permissions exist for a resource
        // Used internally during seeding — silent, no return value
        Task EnsureExistsAsync(string resourceName);
    }
}
