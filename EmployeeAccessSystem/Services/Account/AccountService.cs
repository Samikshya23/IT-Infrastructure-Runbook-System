using EmployeeAccessSystem.Helpers;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeAccessSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepositories _accountRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepositories accountRepo,
            IConfiguration config,
            ILogger<AccountService> logger)
        {
            _accountRepo = accountRepo;
            _config = config;
            _logger = logger;
        }

        public async Task<string> RegisterAsync(RegisterModel model)
        {
            try
            {
                if (model == null)
                {
                    return "Invalid registration data.";
                }

                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    return "Full name is required.";
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return "Email is required.";
                }

                if (model.RoleId <= 0)
                {
                    return "Please select role.";
                }

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return "Password is required.";
                }

                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    return "Confirm password is required.";
                }

                if (model.Password != model.ConfirmPassword)
                {
                    return "Password and confirm password do not match.";
                }

                model.FullName = model.FullName.Trim();
                model.Email = model.Email.Trim().ToLower();

                string passwordKey = _config["Security:PasswordKey"] ?? "";

                Helper.CreatePasswordHash(
                    model.Password,
                    passwordKey,
                    out byte[] passwordHash,
                    out byte[] passwordSalt
                );

                int createdBy = 1;

                DbResult result = await _accountRepo.RegisterAsync(
                    model,
                    passwordHash,
                    passwordSalt,
                    createdBy
                );

                if (result.ResultId <= 0)
                {
                    return result.Message;
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering account. Email: {Email}", model.Email);
                return "Something went wrong while registering user.";
            }
        }

        public async Task<string> LoginAsync(LoginModel model)
        {
            try
            {
                if (model == null)
                {
                    return "Invalid login data.";
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return "Email is required.";
                }

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return "Password is required.";
                }

                model.Email = model.Email.Trim().ToLower();

                Account? account = await _accountRepo.GetByEmailAsync(model.Email);

                if (account == null)
                {
                    return "Account not registered.";
                }

                if (account.IsActive == false)
                {
                    return "Your account is inactive. Please contact administrator.";
                }

                string passwordKey = _config["Security:PasswordKey"] ?? "";

                bool isPasswordValid = Helper.VerifyPassword(
                    model.Password,
                    passwordKey,
                    account.PasswordHash,
                    account.PasswordSalt
                );

                if (isPasswordValid == false)
                {
                    return "Invalid password.";
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while login. Email: {Email}", model.Email);
                return "Something went wrong while login.";
            }
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            try
            {
                email = (email ?? "").Trim().ToLower();

                Account? account = await _accountRepo.GetByEmailAsync(email);

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading account by email. Email: {Email}", email);
                return null;
            }
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            try
            {
                Account? account = await _accountRepo.GetByIdAsync(accountId);

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading account detail. AccountId: {AccountId}", accountId);
                return null;
            }
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            try
            {
                List<Account> accounts = await _accountRepo.GetAllAsync();

                return accounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading account list.");
                return new List<Account>();
            }
        }

        public async Task<string> UpdateAsync(int accountId, RegisterModel model)
        {
            try
            {
                if (accountId <= 0)
                {
                    return "Invalid account.";
                }

                if (model == null)
                {
                    return "Invalid user data.";
                }

                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    return "Full name is required.";
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return "Email is required.";
                }

                if (model.RoleId <= 0)
                {
                    return "Please select role.";
                }

                model.FullName = model.FullName.Trim();
                model.Email = model.Email.Trim().ToLower();

                int modifiedBy = 1;

                DbResult result = await _accountRepo.UpdateAsync(
                    accountId,
                    model,
                    modifiedBy
                );

                if (result.ResultId <= 0)
                {
                    return result.Message;
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating account. AccountId: {AccountId}", accountId);
                return "Something went wrong while updating user.";
            }
        }

        public async Task<string> DeleteAsync(int accountId)
        {
            try
            {
                if (accountId <= 0)
                {
                    return "Invalid account.";
                }

                int deletedBy = 1;

                DbResult result = await _accountRepo.DeleteAsync(
                    accountId,
                    deletedBy
                );

                if (result.ResultId <= 0)
                {
                    return result.Message;
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting account. AccountId: {AccountId}", accountId);
                return "Something went wrong while deleting user.";
            }
        }
    }
}