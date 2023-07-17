using Application.Domain.Enums;
using Application.Domain.Models;
using Application.DTOs;
using Application.DTOs.System;
using Application.Helpers;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    public class SystemController : BaseApiController
    {
        private readonly IQueueService _redisQueueService;
        private readonly ISystemService _systemService;
        private readonly GlobalVar _globalVar;

        public SystemController(
                              IQueueService redisQueueService,
                              ISystemService systemService,
                              GlobalVar globalVar)
        {
            _redisQueueService = redisQueueService;
            _systemService = systemService;
            this._globalVar = globalVar;
        }

        [SwaggerOperation("Get Test Response")]
        [HttpGet("test")]
        public ActionResult<ResponseDTO<bool>> Test()
        {
            return true.FormatAsResponseDTO(200);
        }

        [SwaggerOperation("Get Test Response Full Types")]
        [HttpGet("test-full")]
        public ActionResult<ResponseDTO<Dictionary<string, dynamic>>> TestFullTypes()
        {
            var dataDict = new Dictionary<string, dynamic>();

            dataDict.Add("Number", 123456789);
            dataDict.Add("Float", 123.456789f);
            dataDict.Add("Double", 123.456789d);
            dataDict.Add("String", "Everything seem working!");
            dataDict.Add("Collection", new List<string>() { "Never", "Gonna", "Give", "You", "Up" });
            dataDict.Add("Object", new { Name = "Johnnychiwa", Role = "BackEndDesu" });
            return dataDict.FormatAsResponseDTO(200);
        }

        [SwaggerOperation("Get Test Error Response")]
        [HttpGet("test-error")]
        public ActionResult<ResponseDTO<bool>> TestError()
        {
            throw new BadRequestException("This is a bad request exception!", "SYSTEM_TESTING_OK");
        }

        [Authorize]
        [HttpGet("statistic")]
        [SwaggerOperation("Get Service Overrall Statistic")]
        public async Task<ActionResult<ResponseDTO<SystemStatisticDTO>>> GetStatistics()
        {
            SystemStatisticDTO result = await _systemService.GetStatistic();

            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("test-noti")]
        [SwaggerOperation("Test")]
        public async Task<ActionResult<ResponseDTO<bool>>> TestNoti()
        {
            var result = await _systemService.SpamYourself(User.GetEmail());

            return result.FormatAsResponseDTO(200);
        }

        [HttpGet("cfg")]
        public async Task<ActionResult<ResponseDTO<SystemConfig>>> GetConfig()
        {
            var cfg = GlobalVar.SystemConfig;
            return cfg.FormatAsResponseDTO(200);
        }
        // [Authorize]
        // [HttpGet("test-mail")]
        // [SwaggerOperation("Test")]
        // public async Task<ActionResult<ResponseDTO<bool>>> TestMail([FromQuery] String email)
        // {

        //   await _redisQueueService.AddToQueue(new QueueTask()
        //   {
        //     TaskName = TaskName.SendMail,
        //     TaskData = new Dictionary<string, string>()
        //             {
        //                 { "ToEmail", email },
        //                 { "ToName", email },
        //                 { "Type", "NEW_USER" },
        //                 { "Password", "Konichiwa" }
        //             }
        //   });


        //   return true.FormatAsResponseDTO(200);
        // }
    }
}
