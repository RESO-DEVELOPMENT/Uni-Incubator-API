using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Domain.Enums.Supplier;

namespace Application.DTOs.Supplier
{
    public class SupplierDTO
    {
        public Guid SupplierId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public SupplierStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}