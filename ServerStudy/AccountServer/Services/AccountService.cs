﻿using AccountDB;
using AccountServer.Data;


namespace AccountServer.Services
{
    public class AccountService
    {
        AccountDbContext _dbContext;
        FacebookService _facebook;
        JwtTokenService _jwt;

        public AccountService(AccountDbContext context, FacebookService facebook, JwtTokenService jwt)
        {
            _dbContext = context;
            _facebook = facebook;
            _jwt = jwt;
        }

        public async Task<LoginAccountPacketRes> LoginFacebookAccount(string token)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            FacebookTokenData? tokenData = await _facebook.GetUserTokenData(token);
            if (tokenData == null || tokenData.is_valid == false)
                return res;

            AccountDb? accountDb = _dbContext.Accounts.FirstOrDefault(a => a.LoginProviderUserId == tokenData.user_id && a.LoginProviderType == ProviderType.Facebook);
            if (accountDb == null)
            {
                accountDb = new AccountDb()
                {
                    LoginProviderUserId = tokenData.user_id,
                    LoginProviderType = ProviderType.Facebook
                };

                _dbContext.Accounts.Add(accountDb);
                await _dbContext.SaveChangesAsync();
            }

            res.success = true;
            res.accountDbId = accountDb.AccountDbId;
            res.jwt = _jwt.CreateJwtAccessToken(accountDb.AccountDbId);

            return res;
        }

        public async Task<LoginAccountPacketRes> LoginGuestAccount(string userID)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            AccountDb? accountDb = _dbContext.Accounts.FirstOrDefault(a => a.LoginProviderUserId == userID && a.LoginProviderType == ProviderType.Guest);
            if (accountDb == null)
            {
                accountDb = new AccountDb()
                {
                    LoginProviderUserId = userID,
                    LoginProviderType = ProviderType.Guest
                };

                _dbContext.Accounts?.Add(accountDb);
                await _dbContext.SaveChangesAsync();
            }

            res.success = true;
            res.accountDbId = accountDb.AccountDbId;

            return res;
        }
    }
}
