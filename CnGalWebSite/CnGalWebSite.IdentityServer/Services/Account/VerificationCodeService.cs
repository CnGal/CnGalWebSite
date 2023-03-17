using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.DataReositories;
using System.Linq;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;

namespace CnGalWebSite.IdentityServer.Services.Account
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly IRepository<VerificationCode, long> _verificationCodeRepository;
        private readonly int _maxTimeSpan = 15;
        private readonly int _maxFailedCount = 5;

        public VerificationCodeService(IRepository<VerificationCode, long> verificationCodeRepository)
        {
            _verificationCodeRepository = verificationCodeRepository;
        }

        public async Task<int> GetCodeAsync(string token, string userId, VerificationCodeType type)
        {
            var now = DateTime.UtcNow;


            //查找是否存在令牌
            var code = await _verificationCodeRepository.GetAll().OrderByDescending(s => s.Time).FirstOrDefaultAsync(s => s.UserId == userId && s.Type == type && (token == null || (token == s.Token && s.Time.AddMinutes(_maxTimeSpan) > now)));
            if (code != null)
            {
                return code.Code;
            }

            if (token == null)
            {
                throw new Exception("没有能够重新发送的验证码");
            }

            //生成新的验证码
            var random = new Random().Next(100000, 999999);
            while (true)
            {
                code = await _verificationCodeRepository.FirstOrDefaultAsync(s => s.Code == random);
                if (code == null)
                {
                    _ = await _verificationCodeRepository.InsertAsync(new VerificationCode
                    {
                        Code = random,
                        Token = token,
                        Time = now,
                        UserId = userId,
                        Type = type
                    });
                    break;
                }

                //覆盖已过期的数据
                if (code.Time.AddMinutes(_maxTimeSpan) < now)
                {
                    code.Token = token;
                    code.Code = random;
                    code.Time = now;
                    code.UserId = userId;
                    code.Type = type;
                    _ = await _verificationCodeRepository.UpdateAsync(code);
                    break;
                }

                //该验证码正在使用 重新生成
                random++;
            }

            return random;
        }

        public async Task<string> GetTokenAsync(string code, string userId, VerificationCodeType type)
        {
            var now = DateTime.UtcNow;
            var token = await _verificationCodeRepository.FirstOrDefaultAsync(s => s.UserId == userId && s.Type == type && s.Code.ToString() == code && s.Time.AddMinutes(_maxTimeSpan) > now);
            if (token == null)
            {
                //检查失败 将最接近的Token失败次数+1
                await AddFailedCount(userId, type);
                return null;
            }

            var temp = token.Token;
            //await _verificationCodeRepository.DeleteAsync(token);
            return temp;
        }

        public async Task<bool> CheckAsync(string code, string userName, VerificationCodeType type)
        {
            var now = DateTime.UtcNow;
            var token = await _verificationCodeRepository.FirstOrDefaultAsync(s => s.UserId == userName && s.Type == type && s.Code.ToString() == code && s.Time.AddMinutes(_maxTimeSpan) > now);

            if (token == null)
            {
                //检查失败 将最接近的Token失败次数+1
                await AddFailedCount(userName, type);

                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task AddFailedCount(string userId, VerificationCodeType type)
        {
            var token = await _verificationCodeRepository.FirstOrDefaultAsync(s => s.UserId == userId && s.Type == type);
            if (token != null)
            {
                token.FailedCount++;
                if (token.FailedCount > _maxFailedCount)
                {
                    await _verificationCodeRepository.DeleteAsync(token);
                }
                else
                {
                    await _verificationCodeRepository.UpdateAsync(token);
                }
            }
        }
    }
}
