using Dapper;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Infrastructure.DbContext;

namespace eCommerce.Infrastructure.Repositories;

internal class UsersRepository : IUserRepository
{
    private readonly DapperDbContext _dbContext;

    public UsersRepository(DapperDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<ApplicationUser?> AddUser(ApplicationUser user)
    {
        user.UserId = Guid.NewGuid();

        //SQL Query to insert user data into the "Users" table.
        string query = "INSERT INTO public.\"Users\"(\"UserID\", \"Email\", \"PersonName\", \"Gender\", \"Password\") VALUES(@UserID,@Email,@PersonName,@Gender,@Password)";

        int rowCountAffected = await _dbContext.DbConnection.ExecuteAsync(query, user);

        if (rowCountAffected > 0)
        {
            return user;
        }
        else
        {
            return null;
        }

    }

    public async Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password)
    {
        string query = "SELECT * FROM public.\"Users\" WHERE \"Email\"=@Email AND \"Password\"=@Password";

        var parameters = new { Email = email, Password = password };

        ApplicationUser? user = await _dbContext.DbConnection.QueryFirstOrDefaultAsync<ApplicationUser>(query,parameters);

        return user;
    }

    public async Task<ApplicationUser?> GetUsersByUserID(Guid? userID)
    {
        var query = "SELECT * FROM public.\"Users\" WHERE \"UserID\" = @UserID";
        var parameters = new { UserID = userID };

        using var connection = _dbContext.DbConnection;
        return await connection.QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);
    }

}
