namespace SAGM.Data
{
    public class SeedDb
    {
        private readonly SAGMContext _context;

        public SeedDb(SAGMContext context)
        {
            _context = context;
        }

        public async Task SeedAsync() 
        {
            await _context.Database.EnsureCreatedAsync();
        }
    }
}
