using Application.Domain;
using Application.Domain.Enums.Project;
using Application.Domain.Models;
using Application.Helpers;
using Stateless;

namespace Application.StateMachines
{
    public class ProjectStateMachine
    {
        private StateMachine<int, int> _machine;
        Project _p;

        public ProjectStateMachine(Project p)
        {
            _p = p;
            _machine = new StateMachine<int, int>((int)_p.ProjectStatus);

            _machine.Configure((int)ProjectStatus.Created)
               .OnEntry(data => OnCreated())
               .Permit((int)ProjectStatus.Started, (int)ProjectStatus.Started)
               .Permit((int)ProjectStatus.Cancelled, (int)ProjectStatus.Cancelled);

            _machine.Configure((int)ProjectStatus.Started)
                .OnEntry(data => OnStarted())
                .Permit((int)ProjectStatus.Stopped, (int)ProjectStatus.Stopped);

            _machine.Configure((int)ProjectStatus.Stopped)
                .OnEntry(data => OnStoppped());

            _machine.Configure((int)ProjectStatus.Ended)
                .OnEntry(data => OnEnded());

            _machine.Configure((int)ProjectStatus.Cancelled)
                .OnEntry(data => OnCancelled());

            void OnCreated()
            {
                _p.ProjectStatus = ProjectStatus.Created;
            }

            void OnStarted()
            {
                _p.ProjectStatus = ProjectStatus.Started;
                //var now = DateTimeHelper.Now();

                _p.StartedAt = DateTimeHelper.Now();
                //var endDateTime = now.AddMonths(5);
                //var newEnd = DateTimeHelper.EndOfMonth(endDateTime.Month, endDateTime.Year);
            }

            void OnStoppped()
            {
                _p.ProjectStatus = ProjectStatus.Stopped;
            }

            void OnEnded()
            {
                _p.ProjectStatus = ProjectStatus.Ended;
                _p.EndedAt = DateTimeHelper.Now();
            }

            void OnCancelled()
            {
                _p.ProjectStatus = ProjectStatus.Cancelled;
            }
        }

        public Project TriggerState(ProjectStatus projState)
        {
            _machine.Fire((int)projState);
            return _p;
        }
    }
}