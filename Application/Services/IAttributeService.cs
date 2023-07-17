using Application.DTOs.AttributeGroup;

namespace Application.Services
{
  public interface IAttributeService
  {
    Task<List<AttributeGroupDTO>> GetAllAttributeGroups();
  }
}