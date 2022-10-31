using DBManager.Domains;
using System;
using System.Linq;
using static GreatechApp.Core.Enums.ACL;

namespace GreatechApp.Services.UserServices
{
    public class AuthService : IAuthService
    {
        public IUser CurrentUser { get; private set; }

        public AuthService(DefaultUser defaultUser)
        {
            CurrentUser = defaultUser;
        }

        public bool Authenticate(string username, string password)
        {
            using (var db = new AppDBContext())
            {
                TblUser query = null;
                try
                {
                    query = (from x in db.TblUser
                             orderby x.User_ID
                             where x.User_ID == username
                             select x).Single();
                }
                catch (Exception ex)
                {
                    return false;
                }

                string userPassword = query.Password.Trim();
                if (userPassword == password)
                {
                    CurrentUser.Username = query.UserName;
                    int userLevel = query.User_Level;
                    CurrentUser.UserLevel = userLevel == 0 ? UserLevel.Admin : userLevel == 1 ? UserLevel.Engineer : userLevel == 2 ? UserLevel.Technician : UserLevel.Operator;

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CheckAccess(string operation)
        {
            if(string.IsNullOrEmpty(operation))
            {
                return false;
            }

            if(CurrentUser.UserLevel == UserLevel.Admin)
            {
                return true;
            }

            using (var db = new AppDBContext())
            {
                try
                {
                    int userLevel = CurrentUser.UserLevel == UserLevel.Admin ? (int)UserLevel.Admin : CurrentUser.UserLevel == UserLevel.Engineer ? 
                                    (int)UserLevel.Engineer : CurrentUser.UserLevel == UserLevel.Technician ? (int)UserLevel.Technician : (int)UserLevel.Operator;

                    return Convert.ToBoolean((from x in db.TblAccessControl
                                  where x.User_Level == userLevel
                                  select x.GetType().GetProperty(operation).GetValue(x)).SingleOrDefault());
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }

    public class DefaultUser : IUser
    {
        public string Username { get; set; }
        public UserLevel UserLevel { get; set; }
        public bool IsAuthenticated { get; set; }

        public DefaultUser()
        {
            Init();
        }

        private void Init()
        {
            Username = string.Empty;
            UserLevel = UserLevel.None;
            IsAuthenticated = default;
        }
    }
}
