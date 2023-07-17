using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Supplier;
using Application.Domain.Enums.Voucher;

namespace Application.Domain.Models
{
  public class Supplier
  {
    public Supplier()
    {
      Vouchers = new List<Voucher>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SupplierId { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public SupplierStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    public DateTime? UpdatedAt { get; set; }

    public virtual List<Voucher> Vouchers { get; set; }

  }
}