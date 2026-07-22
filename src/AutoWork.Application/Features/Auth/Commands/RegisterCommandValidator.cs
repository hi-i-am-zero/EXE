using AutoWork.Application.Common.Helpers;
using FluentValidation;

namespace AutoWork.Application.Features.Auth.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Vui lòng nhập email.")
            .EmailAddress().WithMessage("Email không hợp lệ (vd: ten@gmail.com).");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ in hoa (A-Z).")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường (a-z).")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số (0-9).");

        RuleFor(x => x.Request.ConfirmPassword)
            .Equal(x => x.Request.Password).WithMessage("Mật khẩu xác nhận không khớp.");

        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("Vui lòng nhập họ.")
            .MaximumLength(100);

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Vui lòng nhập tên.")
            .MaximumLength(100);

        RuleFor(x => x.Request.Phone)
            .NotEmpty().WithMessage("Vui lòng nhập số điện thoại.")
            .Must(PhoneHelper.IsValid).WithMessage("Số điện thoại không hợp lệ. Nhập 10 số bắt đầu bằng 0.");
    }
}
