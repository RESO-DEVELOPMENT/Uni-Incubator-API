using Application.DTOs.System;

namespace Application.Services
{
  public interface ISystemService
  {
    Task<SystemStatisticDTO> GetStatistic();
    Task<bool> SpamYourself(String email);
  }
}