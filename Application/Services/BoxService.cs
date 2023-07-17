using Application.Helpers;
using Box.V2;
using Box.V2.Config;
using Box.V2.Exceptions;
using Box.V2.JWTAuth;
using Box.V2.Models;
using System.Drawing.Imaging;
using Application.Domain;
using Application.Domain.Enums.MemberFile;
using Application.Domain.Enums.ProjectFile;
using Application.Domain.Enums.SponsorFile;
using Application.Domain.Enums.VoucherFile;
using Application.Domain.Models;

namespace Application.Services
{
  public class BoxService : IBoxService
  {
    public static string PROFILE_IMAGE_FOLDER_ID = "192177168721";
    public static string SPONSOR_IMAGE_FOLDER_ID = "195398086661";
    public static string VOUCHER_IMAGE_FOLDER_ID = "201874152167";
    public static string PROJECT_FINAL_REPORT_FOLDER_ID = "197382657727";

    BoxClient? boxClient = null;
    DateTime lastTokenTime;

    public BoxService()
    {
    }

    public async Task<BoxClient> GetClient()
    {
      if (boxClient == null || lastTokenTime.AddMinutes(59) < DateTimeHelper.Now()) // Token only valid for 60 minutes
      {
        var reader = new StreamReader("boxCfg.json");
        var json = reader.ReadToEnd();

        var boxConfig = BoxConfig.CreateFromJsonString(json);
        var sdk = new BoxJWTAuth(boxConfig);
        var token = await sdk.AdminTokenAsync();
        boxClient = sdk.AdminClient(token);

        lastTokenTime = DateTimeHelper.Now();
      }

      return boxClient!;
    }

    public async Task<BoxCollection<BoxItem>> GetFiles()
    {
      var client = await GetClient();

      // await client.FoldersManager.CreateAsync(new()
      // {
      //   Name = "Folder1",
      //   Parent = new() { Id = "0" }
      // });


      return await client.FoldersManager.GetFolderItemsAsync("0", 500);
    }

    public async Task<BoxCollection<BoxItem>> GetFolder(string folderId)
    {
      var client = await GetClient();
      return await client.FoldersManager.GetFolderItemsAsync(folderId, 500);
    }

    public async Task<BoxFile> UploadSponsorImage(string base64, Sponsor sponsor)
    {
      try
      {
        var bm = ImageHelper.Base64ToImage(base64);
        if (bm == null) throw new BadRequestException("The image is malformed!", ErrorNameValues.BadImage);

        var resizedBm = ImageHelper.ResizeImage(bm, 1024, 1024);

        MemoryStream resizedMs = new MemoryStream();
        resizedBm.Save(resizedMs, ImageFormat.Jpeg);

        var fileName = $"{sponsor.SponsorId}.jpg";

        BoxFile? returnResult = null;

        // If user already has image
        // var oldFile = await FindMemberProfileImageFile(Member.MemberId);
        BoxItem? oldFile = null;
        var oldDbFile = sponsor.SponsorFiles.FirstOrDefault(f => f.FileType == SponsorFileType.SponsorImage);

        if (oldDbFile != null)
        {
          oldFile = await ExistFile(oldDbFile.SystemFile.FileId);
        }

        if (oldFile != null)
        {
          returnResult = await UploadFileVersion(resizedMs, oldFile.Id, fileName);
        }
        else
        {
          var file = await UploadFile(resizedMs, SPONSOR_IMAGE_FOLDER_ID, fileName);
          returnResult = file;
        }

        return returnResult;
      }
      catch (BoxPreflightCheckConflictException<BoxFile>)
      {
        throw new BadRequestException("Error when uploading, please try again!", ErrorNameValues.ServerError);
      }
      catch (Exception ex)
      {
        throw new BadRequestException(ex.Message);
        // throw new BadRequestException("Error when uploading, please try again!");
      }
    }

    public async Task<BoxFile> UploadProfileImage(string base64, Member member)
    {
      try
      {
        var bm = ImageHelper.Base64ToImage(base64);
        if (bm == null) throw new BadRequestException("Ảnh upload đã bị hư!", ErrorNameValues.BadImage);

        var resizedBm = ImageHelper.ResizeImage(bm, 1024, 1024);

        MemoryStream resizedMs = new MemoryStream();
        resizedBm.Save(resizedMs, ImageFormat.Jpeg);

        var fileName = $"{member.MemberId}.jpg";

        BoxFile? returnResult = null;

        // If user already has image
        // var oldFile = await FindMemberProfileImageFile(Member.MemberId);
        BoxItem? oldFile = null;
        var oldDbFile = member.MemberFiles.FirstOrDefault(f => f.FileType == MemberFileType.ProfileImage);

        if (oldDbFile != null)
        {
          oldFile = await ExistFile(oldDbFile.SystemFile.FileId);
        }

        if (oldFile != null)
        {
          returnResult = await UploadFileVersion(resizedMs, oldFile.Id, fileName);
        }
        else
        {
          var file = await UploadFile(resizedMs, PROFILE_IMAGE_FOLDER_ID, fileName);
          returnResult = file;
        }

        return returnResult;
      }
      catch (BoxPreflightCheckConflictException<BoxFile>)
      {
        throw new BadRequestException("Error when uploading, please try again!", ErrorNameValues.ServerError);
      }
      catch (BadRequestException ex)
      {
        throw new BadRequestException(ex.Message);
      }
      catch (Exception ex)
      {
        throw new BadRequestException(ex.Message);
        // throw new BadRequestException("Error when uploading, please try again!");
      }
    }

    public async Task<BoxFile> UploadProjectFile(IFormFile file, Project project, ProjectFileType fileType)
    {
      try
      {
        var fileName = $"{project.ProjectId}";
        var targetFolder = "";

        if (file.ContentType != "application/pdf") throw new BadRequestException("Vui lòng chỉ tải lên file PDF", ErrorNameValues.FileWrongType);
        if (file.Length / 1000 / 1000 > 10) throw new BadRequestException("File chỉ tối đa 10MB!", ErrorNameValues.FileSizeTooBig);
        if (fileType == ProjectFileType.FinalReport)
        {
          fileName += ".pdf";
          targetFolder = PROJECT_FINAL_REPORT_FOLDER_ID;
        }

        Stream fileStream = file.OpenReadStream();
        BoxFile? returnResult = null;

        // If user already has image
        // var oldFile = await FindMemberProfileImageFile(Member.MemberId);
        BoxItem? oldFile = null;
        var oldDbFile = project.ProjectFiles.FirstOrDefault(f => f.FileType == ProjectFileType.FinalReport);

        if (oldDbFile != null)
        {
          // Fetch old File On Box
          oldFile = await ExistFile(oldDbFile.SystemFile.FileId);
        }

        if (oldFile != null)
        {
          returnResult = await UploadFileVersion(fileStream, oldFile.Id, fileName);
        }
        else
        {
          var boxfile = await UploadFile(fileStream, targetFolder, fileName);
          returnResult = boxfile;
        }

        return returnResult;
      }
      catch (BoxPreflightCheckConflictException<BoxFile>)
      {
        throw new BadRequestException("Error when uploading, please try again!", ErrorNameValues.ServerError);
      }
      catch (BadRequestException ex)
      {
        throw new BadRequestException(ex.Message);
      }
      catch (Exception ex)
      {
        throw new BadRequestException(ex.Message);
        // throw new BadRequestException("Error when uploading, please try again!");
      }
    }

    private async Task<BoxFile> UploadFile(Stream stream, string folderId, string fileName)
    {
      var client = await GetClient();

      var fileRequest = new BoxFileRequest
      {
        Name = fileName,
        Parent = new BoxFolderRequest { Id = folderId },
        Type = BoxType.file,
      };

      var bFile = await client.FilesManager.UploadAsync(fileRequest, stream);
      var sLink = await client.FilesManager.CreateSharedLinkAsync(bFile.Id,
        new BoxSharedLinkRequest()
        {
          Access = BoxSharedLinkAccessType.open,
          UnsharedAt = DateTimeHelper.Now().AddYears(100),
          Permissions = new BoxPermissionsRequest() { Download = true, Edit = false }
        });

      return sLink;
    }

    private async Task<BoxFile> UploadFileVersion(Stream stream, string oldFileId, string newFileName)
    {
      var client = await GetClient();
      var versions = await client.FilesManager.ViewVersionsAsync(oldFileId);

      if (versions.Entries.Count() > 5)
      {
        var oldestVersion = versions.Entries.OrderByDescending(e => e.CreatedAt).First();
        await client.FilesManager.DeleteOldVersionAsync(oldFileId, oldestVersion.Id);
      }

      var newFile = await client.FilesManager.UploadNewVersionAsync(newFileName, oldFileId, stream);

      return newFile;
    }

    private async Task<BoxFile?> FindImageFile(Guid id, string folderId)
    {
      var client = await GetClient();
      string fileName = id.ToString();

      var oldFile = await client.SearchManager.QueryAsync(
             query: fileName,
             ancestorFolderIds: new List<string>() { folderId },
             limit: 10
            );

      return oldFile.TotalCount > 0 ? (BoxFile)oldFile.Entries[0] : null;
    }

    private async Task<BoxItem?> ExistImageFile(Guid id, string targetType)
    {
      var client = await GetClient();
      string fileName = id.ToString();

      var folderId = "";
      switch (targetType)
      {
        case nameof(Member):
          {
            folderId = PROFILE_IMAGE_FOLDER_ID;
            break;
          }
        case nameof(Sponsor):
          {
            folderId = SPONSOR_IMAGE_FOLDER_ID;
            break;
          }
        default:
          break;
      }

      var files = await client.FoldersManager.GetFolderItemsAsync(folderId, 100);
      return files.Entries.FirstOrDefault(f => f.Name.Contains(fileName));
    }

    private async Task<BoxFile?> ExistFile(string fileId)
    {
      var client = await GetClient();
      string id = fileId;

      var file = await client.FilesManager.GetInformationAsync(id);
      return file;
    }

    public async Task<BoxFile> UploadVoucherImage(string base64, Voucher voucher)
    {
      try
      {
        var bm = ImageHelper.Base64ToImage(base64);
        if (bm == null) throw new BadRequestException("The image is malformed!", ErrorNameValues.BadImage);

        var resizedBm = ImageHelper.ResizeImage(bm, 1024, 1024);

        MemoryStream resizedMs = new MemoryStream();
        resizedBm.Save(resizedMs, ImageFormat.Jpeg);

        var fileName = $"{voucher.VoucherId}.jpg";

        BoxFile? returnResult = null;

        // If user already has image
        // var oldFile = await FindMemberProfileImageFile(Member.MemberId);
        BoxItem? oldFile = null;
        var oldDbFile = voucher.VoucherFiles.FirstOrDefault(f => f.FileType == VoucherFileType.VoucherImage);

        if (oldDbFile != null)
        {
          oldFile = await ExistFile(oldDbFile.SystemFile.FileId);
        }

        if (oldFile != null)
        {
          returnResult = await UploadFileVersion(resizedMs, oldFile.Id, fileName);
        }
        else
        {
          var file = await UploadFile(resizedMs, VOUCHER_IMAGE_FOLDER_ID, fileName);
          returnResult = file;
        }

        return returnResult;
      }
      catch (BoxPreflightCheckConflictException<BoxFile>)
      {
        throw new BadRequestException("Error when uploading, please try again!", ErrorNameValues.ServerError);
      }
      catch (Exception ex)
      {
        throw new BadRequestException(ex.Message);
        // throw new BadRequestException("Error when uploading, please try again!");
      }
    }
  }
}