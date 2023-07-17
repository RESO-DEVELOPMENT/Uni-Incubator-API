using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Models;
using Stateless;

namespace Application.StateMachines
{
    public class SalaryCycleStateMachine
    {
        private StateMachine<int, int> _machine;
        SalaryCycle _sc;

        public SalaryCycleStateMachine(SalaryCycle sc)
        {
            _sc = sc;
            _machine = new StateMachine<int, int>((int)_sc.SalaryCycleStatus);

            _machine.Configure((int)SalaryCycleStatus.Ongoing)
                .OnEntry(data => OnCreated())
                .Permit((int)SalaryCycleStatus.Locked, (int)SalaryCycleStatus.Locked)
                .Permit((int)SalaryCycleStatus.Cancelled, (int)SalaryCycleStatus.Cancelled);

            _machine.Configure((int)SalaryCycleStatus.Locked)
                .Permit((int)SalaryCycleStatus.Ongoing, (int)SalaryCycleStatus.Ongoing)
                .Permit((int)SalaryCycleStatus.Paid, (int)SalaryCycleStatus.Paid)
                .Permit((int)SalaryCycleStatus.Cancelled, (int)SalaryCycleStatus.Cancelled)
                .OnEntry(data => OnLocked());

            _machine.Configure((int)SalaryCycleStatus.Paid)
                .OnEntry(data => OnPaid());

            _machine.Configure((int)SalaryCycleStatus.Cancelled)
                .OnEntry(data => OnCancelled());

            void OnCreated()
            {
                _sc.SalaryCycleStatus = SalaryCycleStatus.Ongoing;
            }

            void OnLocked()
            {
                _sc.SalaryCycleStatus = SalaryCycleStatus.Locked;
            }

            void OnPaid()
            {
                _sc.SalaryCycleStatus = SalaryCycleStatus.Paid;
            }

            void OnCancelled()
            {
                _sc.SalaryCycleStatus = SalaryCycleStatus.Cancelled;
            }
        }

        public SalaryCycle TriggerState(SalaryCycleStatus projState)
        {
            _machine.Fire((int)projState);
            return _sc;
        }
    }
}