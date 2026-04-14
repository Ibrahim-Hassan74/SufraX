using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace EStoreX.API.Identity
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CustomIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public override IdentityError DefaultError()
            => new() { Code = nameof(DefaultError), Description = _localizer["DefaultError"] };

        public override IdentityError ConcurrencyFailure()
            => new() { Code = nameof(ConcurrencyFailure), Description = _localizer["ConcurrencyFailure"] };

        public override IdentityError PasswordMismatch()
            => new() { Code = nameof(PasswordMismatch), Description = _localizer["PasswordMismatch"] };

        public override IdentityError InvalidToken()
            => new() { Code = nameof(InvalidToken), Description = _localizer["InvalidToken"] };

        public override IdentityError LoginAlreadyAssociated()
            => new() { Code = nameof(LoginAlreadyAssociated), Description = _localizer["LoginAlreadyAssociated"] };

        public override IdentityError InvalidUserName(string userName)
            => new() { Code = nameof(InvalidUserName), Description = _localizer["InvalidUserName", userName] };

        public override IdentityError InvalidEmail(string email)
            => new() { Code = nameof(InvalidEmail), Description = _localizer["InvalidEmail", email] };

        public override IdentityError DuplicateUserName(string userName)
            => new() { Code = nameof(DuplicateUserName), Description = _localizer["DuplicateUserName", userName] };

        public override IdentityError DuplicateEmail(string email)
            => new() { Code = nameof(DuplicateEmail), Description = _localizer["DuplicateEmail", email] };

        public override IdentityError InvalidRoleName(string role)
            => new() { Code = nameof(InvalidRoleName), Description = _localizer["InvalidRoleName", role] };

        public override IdentityError DuplicateRoleName(string role)
            => new() { Code = nameof(DuplicateRoleName), Description = _localizer["DuplicateRoleName", role] };

        public override IdentityError UserAlreadyHasPassword()
            => new() { Code = nameof(UserAlreadyHasPassword), Description = _localizer["UserAlreadyHasPassword"] };

        public override IdentityError UserLockoutNotEnabled()
            => new() { Code = nameof(UserLockoutNotEnabled), Description = _localizer["UserLockoutNotEnabled"] };

        public override IdentityError UserAlreadyInRole(string role)
            => new() { Code = nameof(UserAlreadyInRole), Description = _localizer["UserAlreadyInRole", role] };

        public override IdentityError UserNotInRole(string role)
            => new() { Code = nameof(UserNotInRole), Description = _localizer["UserNotInRole", role] };

        public override IdentityError PasswordTooShort(int length)
            => new() { Code = nameof(PasswordTooShort), Description = _localizer["PasswordTooShort", length] };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer["PasswordRequiresNonAlphanumeric"] };

        public override IdentityError PasswordRequiresDigit()
            => new() { Code = nameof(PasswordRequiresDigit), Description = _localizer["PasswordRequiresDigit"] };

        public override IdentityError PasswordRequiresLower()
            => new() { Code = nameof(PasswordRequiresLower), Description = _localizer["PasswordRequiresLower"] };

        public override IdentityError PasswordRequiresUpper()
            => new() { Code = nameof(PasswordRequiresUpper), Description = _localizer["PasswordRequiresUpper"] };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new() { Code = nameof(PasswordRequiresUniqueChars), Description = _localizer["PasswordRequiresUniqueChars", uniqueChars] };
    }
}
