using Application.Domain;
using Application.Domain.Enums;
using Application.Domain.Enums.User;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.User;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IQueueService _redisQueueService;
        private readonly IFirebaseService _firebaseService;

        public UserService(UnitOfWork unitOfWork,
                        ITokenService tokenService,
                        IQueueService redisQueueService,
                        IFirebaseService firebaseService,
                        IMapper mapper)
        {
            _tokenService = tokenService;
            _redisQueueService = redisQueueService;
            _firebaseService = firebaseService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Newly created user</returns>
        public async Task<UserDTO> CreateNewUser(UserCreateDTO dto)
        {
            var user = await _unitOfWork.UserRepository.GetByEmailWithMember(dto.EmailAddress);
            if (user != null) throw new BadRequestException("Email đã tồn tại trong hệ thống!", ErrorNameValues.UserDuplicated);

            var newUser = await CreateNewUser(dto, LoginType.Password, sendEmail: dto.SendEmail, isAdmin: dto.IsAdmin);
            var mappedUser = _mapper.Map<UserDTO>(newUser);
            return mappedUser;
        }

        public async Task<bool> ChangePassword(UserChangePasswordDTO dto, string requesterEmail)
        {
            var user = await _unitOfWork.UserRepository.GetByEmail(requesterEmail) ?? throw new BadRequestException("Người Dùng Không Tồn Tại!", ErrorNameValues.UserNotFound);

            if (user.LoginType != LoginType.Password || user.Password == null)
                throw new BadRequestException("Tài khoản này không thề đổi mật khẩu!", ErrorNameValues.UserUnableToChangePassword);

            var passwordResult = PasswordHasher.Verify(dto.OldPassword, user.Password);
            if (!passwordResult) throw new BadRequestException("Sai mật khẩu cũ!", ErrorNameValues.InvalidCredential);

            var newPassword = PasswordHasher.Hash(dto.NewPassword);

            user.Password = newPassword;

            var result = await _unitOfWork.SaveAsync();
            return result;
        }

        public async Task<bool> RequestChangePassword(UserRequestResetPasswordDTO dto)
        {
            var user = await _unitOfWork.UserRepository.GetByEmailWithMember(dto.EmailAddress) ?? throw new BadRequestException("Người Dùng Không Tồn Tại!", ErrorNameValues.UserNotFound);

            if (user.LoginType != LoginType.Password || user.Password == null)
                throw new BadRequestException("Tài khoản này không thề đổi mật khẩu!", ErrorNameValues.UserUnableToChangePassword);

            if (user.EmailAddress == "admin@gmail.com")
                throw new BadRequestException("Tài khoản này không thề đổi mật khẩu!", ErrorNameValues.UserUnableToChangePassword);

            var secondBetweenRequest = 300;
            var token = StringHelper.Generate(50);
            if (user.PasswordChangeToken != null)
            {
                var lastSent = user.PasswordChangeTokenLastSent!.Value;
                var lastSendFuture = lastSent.AddSeconds(secondBetweenRequest);

                if (lastSendFuture > DateTimeHelper.Now())
                {
                    var diff = lastSendFuture - DateTimeHelper.Now();
                    throw new BadRequestException($"Bạn hơi nhanh tay, vui lòng đợi thêm {Math.Round(diff.TotalSeconds, 0)} giây!");
                }

                // Refresh Token If Expired
                if (user.PasswordChangeTokenExpiredAt > DateTimeHelper.Now())
                {
                    user.PasswordChangeToken = token;
                    user.PasswordChangeTokenExpiredAt = DateTimeHelper.Now().AddHours(2);
                }
                // Refresh Sent Cooldown
                user.PasswordChangeTokenLastSent = DateTimeHelper.Now();
                _unitOfWork.UserRepository.Update(user);
            }
            else
            {
                user.PasswordChangeToken = StringHelper.Generate(50);
                user.PasswordChangeTokenExpiredAt = DateTimeHelper.Now().AddHours(2);
                user.PasswordChangeTokenLastSent = DateTime.Now;
                _unitOfWork.UserRepository.Update(user);
            }


            var result = await _unitOfWork.SaveAsync();

            if (result)
            {
                await _redisQueueService.AddToQueue(new QueueTask()
                {
                    TaskName = TaskName.SendMail,
                    TaskData = new Dictionary<string, string>()
                    {
                        { "ToEmail", user.EmailAddress },
                        { "ToName", user.Member.FullName },
                        { "Type", "REQUEST_RESET_PASSWORD" },
                        { "Token",  token}
                    }
                });
            }

            return result;

        }

        public async Task<bool> ResetPassword(UserResetPasswordDTO dto)
        {
            var user = await _unitOfWork.UserRepository.GetByPasswordResetToken(dto.Token) ?? throw new BadRequestException("Token is not valid!", ErrorNameValues.UserNotFound);

            if (user.LoginType != LoginType.Password || user.Password == null)
                throw new BadRequestException("Tài khoản này không thề đổi mật khẩu!", ErrorNameValues.UserUnableToChangePassword);

            if (user.EmailAddress == "admin@gmail.com")
                throw new BadRequestException("Tài khoản này không thề đổi mật khẩu!", ErrorNameValues.UserUnableToChangePassword);

            var newPassword = PasswordHasher.Hash(dto.NewPassword);

            user.Password = newPassword;

            user.PasswordChangeTokenLastSent = null;
            user.PasswordChangeToken = null;
            user.PasswordChangeTokenExpiredAt = null;

            var result = await _unitOfWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// Check for login
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>LoginResult if success, throw if not exist</returns>
        public async Task<UserLoginResultDTO> Login(LoginDTO dto)
        {
            var user = await _unitOfWork.UserRepository.GetByEmailWithMember(dto.EmailAddress);
            if (user == null)
            {
                throw new BadRequestException("Sai email hoặc mật khẩu!", ErrorNameValues.InvalidCredential);
            }

            if (!PasswordHasher.Verify(dto.Password, user.Password!))
            {
                throw new BadRequestException("Sai email hoặc mật khẩu!", ErrorNameValues.InvalidCredential);
            }

            if (user.UserStatus != UserStatus.Available)
            {
                throw new BadRequestException("Tài khoản đã bị vô hiệu hoá!", ErrorNameValues.InvalidCredential);
            }

            var mappedUser = _mapper.Map<UserDTO>(user);
            var loginResult = new UserLoginResultDTO()

            {
                User = mappedUser,
                Token = _tokenService.CreateToken(user.UserId, user.EmailAddress, user.RoleId)
            };

            return loginResult;

        }

        /// <summary>
        /// Check for login with google (Auto account creation)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>LoginResult if success</returns>
        public async Task<UserLoginResultDTO> LoginWithGoogle(LoginGoogleDTO dto)
        {
            var cred = await _firebaseService.VerifyIdToken(dto.Token);
            if (cred == null) throw new BadRequestException("Token Google không hợp lệ!", ErrorNameValues.InvalidCredential);
            var claims = cred.Claims;

            var email = claims.First(c => c.Key == "email").Value.ToString();

            //if (email.Split("@").Last() != "fpt.edu.vn" && email != "johnnymc2001@gmail.com")
            //    throw new BadRequestException("The system currently only accepted @fpt.edu.vn email!", ErrorNameValues.InvalidCredential);

            var name = claims.First(c => c.Key == "name").Value.ToString();
            var imgUrl = claims.First(c => c.Key == "picture").Value.ToString();

            var user = await _unitOfWork.UserRepository.GetByEmailWithMember(email!);
            UserLoginResultDTO loginResult = new UserLoginResultDTO();

            if (user == null)
            {
                user = await CreateNewUser(new UserCreateDTO()
                {
                    EmailAddress = email!,
                    FullName = name!,
                }, LoginType.Google, imgUrl);

                loginResult.IsNewUser = true;
            }

            if (user.LoginType != LoginType.Google)
            {
                throw new BadRequestException("Tài khoản này đã sử dụng một cách đăng nhập khác!", ErrorNameValues.WrongLoginType);
            }

            if (user.UserStatus == UserStatus.Unavailable)
            {
                throw new BadRequestException("Tại khoản đã bị vô hiệu hoá!", ErrorNameValues.InvalidCredential);
            }

            UserDTO mappedUser = _mapper.Map<UserDTO>(user);

            loginResult.User = mappedUser;
            loginResult.Token = _tokenService.CreateToken(user.UserId, user.EmailAddress, user.RoleId);



            return loginResult;

        }

        public async Task<string?> GetUserPinCode(String requesterEmail)
        {
            var user = await _unitOfWork.UserRepository.GetByEmail(requesterEmail);
            if (user == null) throw new BadRequestException("Người Dùng Không Tồn Tại!", ErrorNameValues.UserNotFound);

            return user.PinCode;
        }

        public async Task<bool> CheckUserPinCode(string? pinCode, string requesterEmail)
        {
            var user = await _unitOfWork.UserRepository.GetByEmail(requesterEmail)
                ?? throw new BadRequestException("Người Dùng Không Tồn Tại!", ErrorNameValues.UserNotFound);

            // If user don't have pin code, don't need to check
            if (user.PinCode == null)
                return true;

            // If user had pin code but didn't provide
            if (pinCode == null)
                throw new BadRequestException($"Vui lòng nhập mã pin!", ErrorNameValues.InvalidCredential);
            var maxAttempt = 5;

            var cooldownLeft = 0d;
            var pinCorrect = user.PinCode == pinCode;

            // If max attempt reached
            if (user.FailedPincodeAttempt >= maxAttempt)
            {
                var timeLeft = user.FailedPincodeLastAttemptTime.Value;
                var timeLeftFuture = timeLeft.AddMinutes(1);
                // If still cooldown, set cooldown flag so that script can stop
                if (timeLeftFuture > DateTimeHelper.Now())
                {
                    cooldownLeft = (timeLeftFuture - DateTimeHelper.Now()).TotalSeconds;
                }
                else
                {
                    user.FailedPincodeAttempt = 1;
                }
            }
            else
            {
                user.FailedPincodeAttempt++;
                user.FailedPincodeLastAttemptTime = DateTimeHelper.Now();
            }

            if (cooldownLeft > 0)
            {
                throw new BadRequestException($"Bạn phải đợi {Math.Round(cooldownLeft, 0)} giây trước khi nhập lại!", ErrorNameValues.PinCooldown);
            }

            if (!pinCorrect)
            {
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveAsync();

                throw new BadRequestException($"Mã PIN sai, bạn còn {maxAttempt - user.FailedPincodeAttempt} lượt!", ErrorNameValues.InvalidPincode);
            }
            else
            {
                user.FailedPincodeAttempt = 0;
            }
            // If success, reset attempt count

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateUserPinCode(UserUpdatePinCodeDTO dto, String requesterEmail)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmailWithUser(requesterEmail);
            if (member == null) throw new BadRequestException("Tài khoản không tồn tại!", ErrorNameValues.MemberNotFound);

            var user = member.User;
            if (user.PinCode == null)
            {
                if (dto.OldPinCode == null) throw new BadRequestException("Tại khoản chưa có mã PIN nên bạn không cần phải nhập mã PIN cũ!", ErrorNameValues.TooMuchParams);

                user.PinCode = dto.NewPinCode;

                _unitOfWork.MemberRepository.Update(member);
                return await _unitOfWork.SaveAsync();
            }

            if (dto.OldPinCode != user.PinCode)
                throw new BadRequestException("Sai mã PIN cũ!", ErrorNameValues.InvalidCredential);

            user.PinCode = dto.NewPinCode;

            _unitOfWork.MemberRepository.Update(member);
            return await _unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Get all user
        /// </summary>
        /// <returns>User</returns>
        public async Task<PagedList<User>> GetAll(UserQueryParams queryParams)
        {
            var query = _unitOfWork.UserRepository.GetQuery();
            query = query.Include(p => p.Role);

            if (queryParams.EmailAddress != null)
                query = query.Where(u => u.EmailAddress.ToLower().Contains(queryParams.EmailAddress.ToLower()));

            var users = await PagedList<User>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return users;
        }

        /// <summary>
        /// Get self user info
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User</returns>
        public async Task<User?> GetSelf(Guid userId)
        {
            var user = await GetByID(userId);
            return user;
        }

        private async Task<User> CreateNewUser(
            UserCreateDTO dto,
            LoginType loginType = LoginType.Password,
            string? imageUrl = null,
            bool sendEmail = false,
            bool isAdmin = false)
        {
            var userRole = (loginType == LoginType.Google || !isAdmin) ?
             await _unitOfWork.RoleRepository.GetQuery().FirstAsync(x => x.RoleId == "USER")
             : await _unitOfWork.RoleRepository.GetQuery().FirstAsync(x => x.RoleId == "ADMIN"); ;

            string? hashedPassword = null;
            if (loginType == LoginType.Password)
            {
                hashedPassword = PasswordHasher.Hash(dto.Password!);
            }

            var newUser = _mapper.Map<UserCreateDTO, User>(dto);
            newUser.Password = hashedPassword;
            newUser.RoleId = userRole.RoleId;
            newUser.LoginType = loginType;

            var newMember = _mapper.Map<UserCreateDTO, Member>(dto);
            newMember.ImageUrl = imageUrl;

            var firstLevel = await _unitOfWork.LevelRepository.GetFirstLevel();
            newMember.MemberLevels = new List<MemberLevel>() { new MemberLevel() { Level = firstLevel, IsActive = true } };

            newMember.MemberWallets.Add(new MemberWallet()
            {
                Wallet = new Wallet()
                {
                    WalletToken = WalletToken.XP,
                    WalletType = WalletType.Cold,
                    ExpiredDate = DateTimeHelper.Now().AddYears(1000),
                    Amount = 0,
                    TargetType = TargetType.Member,
                }
            });

            newMember.MemberWallets.Add(new MemberWallet()
            {
                Wallet = new Wallet()
                {
                    WalletToken = WalletToken.Point,
                    WalletType = WalletType.Cold,
                    ExpiredDate = DateTimeHelper.Now().AddYears(1000),
                    Amount = 0,
                    TargetType = TargetType.Member,
                }
            });

            newUser.Member = newMember;
            _unitOfWork.UserRepository.Add(newUser);
            // _unitOfWork.MemberRepository.Add(newMember);

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Hệ thống lỗi!", ErrorNameValues.ServerError);

            if (sendEmail)
                await _redisQueueService.AddToQueue(new QueueTask()
                {
                    TaskName = TaskName.SendMail,
                    TaskData = new Dictionary<string, string>()
                    {
                        { "ToEmail", newUser.EmailAddress },
                        { "ToName", newMember.FullName },
                        { "Type", "NEW_USER" },
                        { "Password", dto.Password }
                    }
                });

            return newUser;
        }

        public async Task<bool> AddFCMToken(FCMTokenAddDTO dto, Guid userId)
        {
            var user = await _unitOfWork.UserRepository.GetQuery()
              .Where(u => u.UserId == userId)
              .Include(u => u.UserFCMTokens.OrderBy(f => f.CreatedAt))
              .FirstOrDefaultAsync();
            if (user == null) throw new NotFoundException("Người Dùng Không Tồn Tại!", ErrorNameValues.UserNotFound);

            var curFcm = user.UserFCMTokens.FirstOrDefault(c => c.Token == dto.Token);
            if (curFcm == null)
            {
                if (user.UserFCMTokens.Count > 10)
                {
                    user.UserFCMTokens.RemoveAt(0);
                }

                user.UserFCMTokens.Add(new UserFCMToken()
                {
                    Token = dto.Token
                });
            }

            _unitOfWork.UserRepository.Update(user);
            return await _unitOfWork.SaveAsync();
        }

        private async Task<List<User>> GetAll()
        {
            var users = await _unitOfWork.UserRepository.GetQuery()
              .Include(x => x.Role)
              .ToListAsync();
            return users;
        }

        private async Task<User?> GetByID(Guid id)
        {
            var user = await _unitOfWork.UserRepository.GetQuery()
                .Include(x => x.Role)
                .Where(x => x.UserId == id)
                    .FirstOrDefaultAsync();

            return user;
        }

        private async Task<bool> Insert(User e)
        {
            _unitOfWork.UserRepository.Add(e);
            return await _unitOfWork.SaveAsync();
        }

        private async Task<bool> Update(User e)
        {
            _unitOfWork.UserRepository.Update(e);
            return await _unitOfWork.SaveAsync();
        }


        public async Task<bool> UpdateUser(UserUpdateDTO dto)
        {
            var user = await GetByID(dto.UserId);
            if (user == null)
                throw new NotFoundException("Người Dùng Không Tồn Tại!", ErrorNameValues.UserNotFound);

            _mapper.Map(dto, user);
            _unitOfWork.UserRepository.Update(user);
            var result = await _unitOfWork.SaveAsync();
            return result;
        }


        public async Task<bool> UpdateUsers(List<UserUpdateDTO> dto)
        {
            if (dto.DistinctBy(dtoUser => dtoUser.UserId).Count() < dto.Count())
                throw new BadRequestException("Bạn có mail bị trùng lập trong yêu cầu!", ErrorNameValues.UserDuplicated);

            var usersDb = await _unitOfWork.UserRepository.GetQuery()
                  .Where(userDb => dto.Select(dtoUser => dtoUser.UserId).Contains(userDb.UserId)).ToListAsync();

            var invalidUser = dto.Where(dtoUser => !usersDb.Select(userDb => userDb.UserId).Contains(dtoUser.UserId)).ToList();

            if (invalidUser.Count > 0)
                throw new NotFoundException($"Không có người dùng nào có id là [{String.Join(",", invalidUser)}]", ErrorNameValues.UserNotFound);

            usersDb.ForEach(userDb =>
            {
                var dtoUser = dto.First(dtoU => dtoU.UserId == userDb.UserId);
                _mapper.Map(dtoUser, userDb);
                _unitOfWork.UserRepository.Update(userDb);
            });

            var result = await _unitOfWork.SaveAsync();
            return result;
        }
    }
}