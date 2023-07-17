using Application.Domain;
using Application.Domain.Enums.ProjectEndRequest;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Models;
using Application.Helpers;
using Stateless;

namespace Application.StateMachines
{
    public class ProjectEndRequestStateMachine
    {
        private StateMachine<int, int> _machine;
        ProjectEndRequest _per;

        public ProjectEndRequestStateMachine(ProjectEndRequest per)
        {
            _per = per;
            _machine = new StateMachine<int, int>((int)_per.Status);

            _machine.Configure((int)ProjectEndRequestStatus.Created)
              .OnEntry(data => OnCreated())

              .Permit((int)ProjectEndRequestStatus.Accepted, (int)ProjectEndRequestStatus.Accepted)
              .Permit((int)ProjectEndRequestStatus.Rejected, (int)ProjectEndRequestStatus.Rejected)
              .Permit((int)ProjectEndRequestStatus.Cancelled, (int)ProjectEndRequestStatus.Cancelled);


            _machine.Configure((int)ProjectEndRequestStatus.Accepted)
              .OnEntry(data => OnAccepted());

            _machine.Configure((int)ProjectEndRequestStatus.Rejected)
              .OnEntry(data => OnRejected());

            _machine.Configure((int)ProjectEndRequestStatus.Cancelled)
              .OnEntry(data => OnCancelled());

            void OnCreated()
            {
                _per.Status = ProjectEndRequestStatus.Created;
            }

            void OnAccepted()
            {
                _per.Status = ProjectEndRequestStatus.Accepted;
                _per.ReviewedAt = DateTimeHelper.Now();
            }

            void OnRejected()
            {
                _per.Status = ProjectEndRequestStatus.Rejected;
                _per.ReviewedAt = DateTimeHelper.Now();
            }

            void OnCancelled()
            {
                _per.Status = ProjectEndRequestStatus.Cancelled;
            }

        }

        public ProjectEndRequest TriggerState(ProjectEndRequestStatus projState)
        {
            _machine.Fire((int)projState);
            return _per;
        }
    }
}