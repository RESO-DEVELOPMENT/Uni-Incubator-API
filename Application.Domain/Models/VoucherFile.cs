using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.VoucherFile;

namespace Application.Domain.Models
{
    public class VoucherFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid VoucherFileId { get; set; }

        public Guid VoucherId { get; set; }
        public virtual Voucher Voucher { get; set; } = null!;

        public Guid SystemFileId { get; set; }
        public virtual SystemFile SystemFile { get; set; } = null!;

        public VoucherFileType FileType { get; set; }
    }
}