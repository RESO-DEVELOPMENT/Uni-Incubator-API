using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Application.Helpers;

namespace Application.DTOs.TicketDetail
{
  public class TicketDetailCreateDTO
  {
    [Required]
    public String Content { get; set; } = null!;
  }
}