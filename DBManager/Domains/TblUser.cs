﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>

namespace DBManager.Domains
{
    public partial class TblUser
    {
        public string User_ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int User_Level { get; set; }

        public virtual TblAccessControl User_LevelNavigation { get; set; }
    }
}