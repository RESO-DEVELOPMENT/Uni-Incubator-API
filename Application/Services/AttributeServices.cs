using Application.DTOs.AttributeGroup;
using Application.Persistence.Repositories;
using AutoMapper;

namespace Application.Services
{
  public class AttributeService : IAttributeService
  {
    private readonly IMapper _mapper;
    private readonly UnitOfWork _unitOfWork;

    public AttributeService(UnitOfWork unitOfWork,
                       IMapper mapper)
    {
      _unitOfWork = unitOfWork;
      _mapper = mapper;
    }
    /// <summary>
    /// Get all user
    /// </summary>
    /// <returns>User</returns>
    public async Task<List<AttributeGroupDTO>> GetAllAttributeGroups()
    {
      var ags = await _unitOfWork.AttributeGroupRepository.GetAll();

      var agsMap = _mapper.Map<List<AttributeGroupDTO>>(ags);
      return agsMap;
    }
  }
}