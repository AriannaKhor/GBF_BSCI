// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DBManager.Domains.Configurations
{
    public partial class TblUserConfiguration : IEntityTypeConfiguration<TblUser>
    {
        public void Configure(EntityTypeBuilder<TblUser> entity)
        {
            entity.HasKey(e => e.User_ID)
                .HasName("PK_Tbl_User");

            entity.Property(e => e.User_ID).HasMaxLength(20);

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.User_LevelNavigation)
                .WithMany(p => p.TblUser)
                .HasForeignKey(d => d.User_Level)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_User_Tbl_AccessControl");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<TblUser> entity);
    }
}
