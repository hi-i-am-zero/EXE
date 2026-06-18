using AutoWork.Shared.Enums;

namespace AutoWork.Shared.Constants;

public static class PlanCredits
{
    public const int Free = 100;
    public const int Starter = 1_000;
    public const int Pro = 10_000;
    public const int Business = -1;

    public static int GetCredits(PlanType planType) =>
        planType switch
        {
            PlanType.Free => Free,
            PlanType.Starter => Starter,
            PlanType.Pro => Pro,
            PlanType.Business => Business,
            _ => Free
        };

    public static bool IsUnlimited(PlanType planType) => planType == PlanType.Business;
}
