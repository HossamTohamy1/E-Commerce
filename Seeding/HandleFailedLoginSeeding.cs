using Microsoft.EntityFrameworkCore;

namespace E_Commers.Seeding
{
    public class HandleFailedLoginSeeding
    {
        public readonly ECommerceDbContext _context;
        public HandleFailedLoginSeeding(ECommerceDbContext context)
        {
            _context = context;
        }
        public async Task HandleFailedLogin(string userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                // إذا كان المستخدم غير موجود، يمكنك التعامل مع الحالة كما يناسبك
                return;
            }

            user.FailedAttempts++;

            if (user.FailedAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEnd = DateTime.Now.AddMinutes(15);
            }

            await _context.SaveChangesAsync();
        }
    }
}
