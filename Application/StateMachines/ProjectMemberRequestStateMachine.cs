using Application.Domain;
using Application.Domain.Enums.ProjectMemberRequest;
using Application.Domain.Models;
using Application.Helpers;
using Stateless;

namespace Application.StateMachines
{
    public class ProjectMemberRequestStateMachine
    {
        private StateMachine<int, int> _machine;
        ProjectMemberRequest _pm;

        public ProjectMemberRequestStateMachine(ProjectMemberRequest pr)
        {
            _pm = pr;
            _machine = new StateMachine<int, int>((int)_pm.Status);

            _machine.Configure((int)ProjectMemberRequestStatus.Created)
               .OnEntry(data => OnCreated())
               .Permit((int)ProjectMemberRequestStatus.Accepted, (int)ProjectMemberRequestStatus.Accepted)
               .Permit((int)ProjectMemberRequestStatus.Rejected, (int)ProjectMemberRequestStatus.Rejected)
               .Permit((int)ProjectMemberRequestStatus.Cancelled, (int)ProjectMemberRequestStatus.Cancelled);

            _machine.Configure((int)ProjectMemberRequestStatus.Accepted)
                .OnEntry(data => OnAccepted());

            _machine.Configure((int)ProjectMemberRequestStatus.Rejected)
                .OnEntry(data => OnRejected());

            _machine.Configure((int)ProjectMemberRequestStatus.Cancelled)
                .OnEntry(data => OnCancelled());

            void OnCreated()
            {
                _pm.Status = ProjectMemberRequestStatus.Created;
            }

            void OnAccepted()
            {
                _pm.Status = ProjectMemberRequestStatus.Accepted;
                _pm.ReviewedAt = DateTimeHelper.Now();
            }

            void OnRejected()
            {
                _pm.Status = ProjectMemberRequestStatus.Rejected;
                _pm.ReviewedAt = DateTimeHelper.Now();
            }

            void OnCancelled()
            {
                _pm.Status = ProjectMemberRequestStatus.Cancelled;
                _pm.ReviewedAt = DateTimeHelper.Now();
            }

        }

        public ProjectMemberRequest TriggerState(ProjectMemberRequestStatus projState)
        {
            _machine.Fire((int)projState);
            return _pm;
        }
    }
}