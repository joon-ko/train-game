using System.Collections.Generic;

public static class CargoUtils
{
    public static Dictionary<AccuracyGrade, int> awardedCargoForGrade = new Dictionary<AccuracyGrade, int>()
    {
        { AccuracyGrade.Perfect, 24 },
        { AccuracyGrade.Good, 18 },
        { AccuracyGrade.OK, 12 },
        { AccuracyGrade.Miss, 0 }
    };

    // Used to calculate point deductions for non-perfect deliveries.
    public static Dictionary<AccuracyGrade, int> divisorForGrade = new Dictionary<AccuracyGrade, int>()
    {
        { AccuracyGrade.Good, 6 },
        { AccuracyGrade.OK, 3 }
    };

    public static int GetAwardedCargoForGrade(AccuracyGrade grade)
    {
        return awardedCargoForGrade[grade];
    }

    public static int GetDeliveredCargoForGrade(AccuracyGrade grade, int cargoCount)
    {
        if (grade == AccuracyGrade.Perfect)
        {
            return cargoCount;
        }

        var divisor = divisorForGrade[grade];
        var deduction = cargoCount / divisor;
        return cargoCount - deduction;
    }
}