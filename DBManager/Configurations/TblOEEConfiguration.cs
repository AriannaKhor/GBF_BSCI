// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DBManager.Domains.Configurations
{
    public partial class TblOEEConfiguration : IEntityTypeConfiguration<TblOEE>
    {
        public void Configure(EntityTypeBuilder<TblOEE> entity)
        {
            entity.HasKey(e => e.OEEDateTime);

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<TblOEE> entity);
    }
}
