using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using Application.Domain;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Models;
using Application.DTOs.MemberExport;
using Application.Helpers;
using Application.Persistence.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using MemoryStream = System.IO.MemoryStream;

namespace Application.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CertificateService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MemberExportLinkDTO> GetCertLink(string memberEmail)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(memberEmail) ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            MemberExportLinkDTO result = new MemberExportLinkDTO();
            string code;

            if (member.CertCode == null)
            {
                code = StringHelper.RandomString(100, false);
                member.CertCode = code;

                _unitOfWork.MemberRepository.Update(member);
                await _unitOfWork.SaveAsync();
            }
            else
            {
                code = member.CertCode;
            }

            result.Code = code;
            result.MemberId = member.MemberId;
            result.Url = $"https://api.uniinc-cnb.com/v1/certificate?memberId={member.MemberId}&code={code}";

            return result;
        }

        public async Task<FileStreamResult> GetMemberCert(string memberEmail)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(memberEmail) ??
                         throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);


            var projectStatus = new List<ProjectStatus>() { ProjectStatus.Started, ProjectStatus.Ended };

            var memberFull = await _unitOfWork.MemberRepository.GetQuery()
                .Include(x => x.ProjectMembers
                    .Where(x => x.Status == ProjectMemberStatus.Active &&
                                projectStatus.Contains(x.Project.ProjectStatus)))
                .ThenInclude(x => x.Project)

                .Include(x => x.ProjectMembers
                    .Where(x => x.Status == ProjectMemberStatus.Active &&
                                projectStatus.Contains(x.Project.ProjectStatus)))
                .ThenInclude(x =>
                    x.ProjectMemberReports.Where(x => x.ProjectReport.Status == ProjectReportStatus.Processed))
                .ThenInclude(x => x.ProjectReportMemberTasks)

                .Where(x => x.MemberId == member.MemberId)
                .FirstAsync();


            var excel = ExcelHelper.GetMemberCertificate(memberFull);

            var stream = new MemoryStream();
            await excel.SaveAsAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            //excel.Stream.Seek(0, SeekOrigin.Begin);

            var fileStream = new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = "Certificate.xlsx"
            };

            return fileStream;
        }

        public async Task<FileStreamResult> GetMemberCert(Guid memberId, string code)
        {
            var member = await _unitOfWork.MemberRepository.GetByID(memberId) ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            if (member.CertCode != code) throw new NotFoundException("Certificate not found!");

            return await GetMemberCert(member.EmailAddress);

        }
    }
}
