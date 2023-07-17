using Application.Domain.Enums.MemberVoucher;
using Application.Helpers;

namespace Application.QueryParams.MemberVoucher
{
    public class SelfVoucherQueryParams : PaginationParams
    {
        public Guid? VoucherId { get; set; }
        public List<MemberVoucherStatus> Status { get; set; }= new List<MemberVoucherStatus>();
        public MemberVoucherOrderBy OrderBy { get; set; } = MemberVoucherOrderBy.CreatedAtDesc;
    }
}