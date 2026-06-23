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
        private readonly IEmailService _emailService;

        public AccountService(
            IAccountRepositories accountRepo,
            IConfiguration config,
            ILogger<AccountService> logger,
            IEmailService emailService)
        {
            _accountRepo = accountRepo;
            _config = config;
            _logger = logger;
            _emailService = emailService;
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

                try
                {
                    await _emailService.SendUserPasswordEmailAsync(model.Email, model.FullName, model.Password);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send registration password email to {Email}", model.Email);
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

        public async Task<string> ForgotPasswordAsync(string email)
        {
            try
            {
                email = (email ?? "").Trim().ToLower();
                if (string.IsNullOrWhiteSpace(email))
                {
                    return "Email is required.";
                }

                Account? account = await _accountRepo.GetByEmailAsync(email);
                if (account == null)
                {
                    return "User account not found.";
                }

                string tempPassword = Guid.NewGuid().ToString("N").Substring(0, 8);

                string passwordKey = _config["Security:PasswordKey"] ?? "";

                Helper.CreatePasswordHash(
                    tempPassword,
                    passwordKey,
                    out byte[] passwordHash,
                    out byte[] passwordSalt
                );

                DbResult result = await _accountRepo.UpdatePasswordAsync(email, passwordHash, passwordSalt);

                if (result.ResultId <= 0)
                {
                    return result.Message;
                }

                try
                {
                    await _emailService.SendForgotPasswordEmailAsync(email, account.FullName ?? email, tempPassword);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send forgot password email to {Email}", email);
                    return "Failed to send temporary password email, but password was reset in the database.";
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password process for {Email}", email);
                return "Something went wrong while resetting the password.";
            }
        }

        public async Task<string> ResetPasswordAsync(string email, string password)
        {
            try
            {
                email = (email ?? "").Trim().ToLower();
                if (string.IsNullOrWhiteSpace(email))
                {
                    return "Email is required.";
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return "Password is required.";
                }

                Account? account = await _accountRepo.GetByEmailAsync(email);
                if (account == null)
                {
                    return "User account not found.";
                }

                string passwordKey = _config["Security:PasswordKey"] ?? "";

                Helper.CreatePasswordHash(
                    password,
                    passwordKey,
                    out byte[] passwordHash,
                    out byte[] passwordSalt
                );

                DbResult result = await _accountRepo.UpdatePasswordAsync(email, passwordHash, passwordSalt);

                if (result.ResultId <= 0)
                {
                    return result.Message;
                }

                try
                {
                    await _emailService.SendForgotPasswordEmailAsync(email, account.FullName ?? email, password);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reset password email to {Email}", email);
                    return "Failed to send email, but password was updated in the database.";
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset process for {Email}", email);
                return "Something went wrong while resetting the password.";
            }
        }
    }
}