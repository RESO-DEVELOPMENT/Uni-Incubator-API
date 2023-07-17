using Application.Domain;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Models;
using Application.Helpers;
using Stateless;

namespace Application.StateMachines
{
    public class ProjectReportStateMachine
    {
        private StateMachine<int, int> _machine;
        ProjectReport _pr;

        public ProjectReportStateMachine(ProjectReport pr)
        {
            _pr = pr;
            _machine = new StateMachine<int, int>((int)_pr.Status);

            _machine.Configure((int)ProjectReportStatus.Drafted)
              .OnEntry(data => OnDrafted())
              .Permit((int)ProjectReportStatus.Created, (int)ProjectReportStatus.Created);

            _machine.Configure((int)ProjectReportStatus.Created)
              .OnEntry(data => OnCreated())
              .Permit((int)ProjectReportStatus.Drafted, (int)ProjectReportStatus.Drafted)

              .Permit((int)ProjectReportStatus.Accepted, (int)ProjectReportStatus.Accepted)
              .Permit((int)ProjectReportStatus.Rejected, (int)ProjectReportStatus.Rejected);

            _machine.Configure((int)ProjectReportStatus.Accepted)
              .OnEntry(data => OnAccepted());

            _machine.Configure((int)ProjectReportStatus.Rejected)
              .OnEntry(data => OnRejected());

            _machine.Configure((int)ProjectReportStatus.Cancelled)
              .OnEntry(data => OnCancelled());

            void OnDrafted()
            {
                _pr.Status = ProjectReportStatus.Drafted;
            }

            void OnCreated()
            {
                _pr.Status = ProjectReportStatus.Created;
            }

            void OnAccepted()
            {
                _pr.Status = ProjectReportStatus.Accepted;
                _pr.ReviewedAt = DateTimeHelper.Now();
            }

            void OnRejected()
            {
                _pr.Status = ProjectReportStatus.Rejected;
                _pr.ReviewedAt = DateTimeHelper.Now();
            }

            void OnCancelled()
            {
                _pr.Status = ProjectReportStatus.Cancelled;
            }

        }

        public ProjectReport TriggerState(ProjectReportStatus projState)
        {
            _machine.Fire((int)projState);
            return _pr;
        }
    }
}