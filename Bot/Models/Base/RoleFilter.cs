using Bot.Data;
using Bot.Models.Abstractions;

namespace Bot.Models.Base;

public abstract class RoleFilter(UsersDbContext context, params string[] roles) : IFilter<User> {
    public abstract int Order { get; set; }

    public async Task<bool> CanExecuteAsync(User obj) {
        if (await context.FindAsync<ApplicationUser>(obj.id) is { } user) {
            return roles.Any(x => user.Roles.Contains(x));
        }

        return false;
    }
}