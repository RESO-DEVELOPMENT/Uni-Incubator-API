using Application.Domain;
using Application.Domain.Enums.MemberVoucher;
using Application.Domain.Models;
using Application.Helpers;
using Stateless;

namespace Application.StateMachines
{
    public class MemberVoucherStateMachine
    {
        private StateMachine<int, int> _machine;
        MemberVoucher _mv;

        public MemberVoucherStateMachine(MemberVoucher mv)
        {
            _mv = mv;
            _machine = new StateMachine<int, int>((int)_mv.Status);

            _machine.Configure((int)MemberVoucherStatus.Created)
               .Permit((int)MemberVoucherStatus.Used, (int)MemberVoucherStatus.Used)
               .Permit((int)MemberVoucherStatus.Expired, (int)MemberVoucherStatus.Expired);

            _machine.Configure((int)MemberVoucherStatus.Used)
                .OnEntry(data => OnUsed());

            _machine.Configure((int)MemberVoucherStatus.Expired)
                .OnEntry(data => OnExpired());

            void OnUsed()
            {
                _mv.Status = MemberVoucherStatus.Used;
                _mv.UpdatedAt = DateTimeHelper.Now();
            }

            void OnExpired()
            {
                _mv.Status = MemberVoucherStatus.Used;
                _mv.UpdatedAt = DateTimeHelper.Now();
            }

        }

        public MemberVoucher TriggerState(MemberVoucherStatus mvState)
        {
            _machine.Fire((int)mvState);
            return _mv;
        }
    }
}