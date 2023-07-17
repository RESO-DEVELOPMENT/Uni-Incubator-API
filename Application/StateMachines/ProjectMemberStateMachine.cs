using Application.Domain;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Models;
using Application.Helpers;
using Stateless;

namespace Application.StateMachines
{
  public class ProjectMemberStateMachine
  {
    private StateMachine<int, int> _machine;
    ProjectMember _pm;

    public ProjectMemberStateMachine(ProjectMember pm)
    {
      _pm = pm;
      _machine = new StateMachine<int, int>((int)_pm.Status);

      _machine.Configure((int)ProjectMemberStatus.Active)
         .Permit((int)ProjectMemberStatus.Inactive, (int)ProjectMemberStatus.Inactive);

      _machine.Configure((int)ProjectMemberStatus.Inactive)
          .OnEntry(data => OnInactive());

      void OnInactive()
      {
        _pm.Status = ProjectMemberStatus.Inactive;
        _pm.UpdatedAt = DateTimeHelper.Now();
      }

    }

    public ProjectMember TriggerState(ProjectMemberStatus projMState)
    {
      _machine.Fire((int)projMState);
      return _pm;
    }
  }
}