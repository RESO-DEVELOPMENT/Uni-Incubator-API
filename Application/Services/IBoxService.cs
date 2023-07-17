using Application.Domain.Enums.ProjectFile;
using Application.Domain.Models;
using Box.V2;
using Box.V2.Models;

namespace Application.Services
{
  public interface IBoxService
  {
    Task<BoxCollection<BoxItem>> GetFiles();
    Task<BoxCollection<BoxItem>> GetFolder(string folderId);
    Task<BoxClient> GetClient();
    
    Task<BoxFile> UploadProjectFile(IFormFile file, Project project, ProjectFileType fileType);

    Task<BoxFile> UploadProfileImage(string base64, Member member);
    Task<BoxFile> UploadSponsorImage(string base64, Sponsor sponsor);
    Task<BoxFile> UploadVoucherImage(string base64, Voucher voucher);
  }
}