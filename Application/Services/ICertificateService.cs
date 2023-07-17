using Application.DTOs.MemberExport;
using Microsoft.AspNetCore.Mvc;

namespace Application.Services;

public interface ICertificateService
{
    Task<FileStreamResult> GetMemberCert(Guid memberId, string code);
    Task<FileStreamResult> GetMemberCert(String memberEmail);
    Task<MemberExportLinkDTO> GetCertLink(string memberEmail);
}