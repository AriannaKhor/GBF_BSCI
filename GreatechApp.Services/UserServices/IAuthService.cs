namespace GreatechApp.Services.UserServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static GreatechApp.Core.Enums.ACL;

    public interface IAuthService
    {
        bool Authenticate(string username, string password);
    }

    public interface IUser
    {
        string Username { get; set; }
        UserLevel UserLevel { get; set; }
        bool IsAuthenticated { get; set; }
    }

    public interface IAccessService
    {
        IUser CurrentUser { get; }
        bool CanAccess { get; }
    }
}
