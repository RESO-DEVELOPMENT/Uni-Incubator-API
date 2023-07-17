using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Application.Domain.Enums.Supplier;

namespace Application.DTOs.Supplier
{
  public class SupplierUpdateDTO
  {
    [Required]
    public Guid SupplierId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public SupplierStatus Status { get; set; }
  }
}