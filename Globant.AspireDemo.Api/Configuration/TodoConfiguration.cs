using Globant.AspireDemo.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Globant.AspireDemo.Api.Configuration;

public class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
               .ValueGeneratedOnAdd();
        builder.Property(t => t.Title)
               .IsRequired()
               .HasMaxLength(255);
        builder.Property(t => t.Status)
               .HasConversion<string>()
               .IsRequired();
        builder.Property(t => t.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");
    }
}
