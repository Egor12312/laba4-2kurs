using System;

namespace GoldenSectionLab {
  public enum OptimizationMode {
    Minimum,
    Maximum,
    Root
  }

  public sealed class OptimizationResult {
    public OptimizationResult(double point, double value, int iterationCount, bool success, string message)
    {
      Point = point;
      Value = value;
      IterationCount = iterationCount;
      Success = success;
      Message = message;
    }

    public double Point { get; }
    public double Value { get; }
    public int IterationCount { get; }
    public bool Success { get; }
    public string Message { get; }
  }

  public static class GoldenSectionOptimizer {
    private const double GoldenRatioNumerator = 5.0;
    private const double GoldenRatioShift = 1.0;
    private const double GoldenRatioDivisor = 2.0;
    private const double MidpointDivisor = 2.0;
    private const double NegationFactor = -1.0;

    private static readonly double GoldenRatioConjugate =
        (Math.Sqrt(GoldenRatioNumerator) - GoldenRatioShift) / GoldenRatioDivisor;

    public static OptimizationResult FindMinimum(Func<double, double> function, double intervalStart, double intervalEnd, double precision)
    {
      ValidateArguments(function, intervalStart, intervalEnd, precision);

      double lowerBound = intervalStart;
      double upperBound = intervalEnd;
      double leftProbe = upperBound - GoldenRatioConjugate * (upperBound - lowerBound);
      double rightProbe = lowerBound + GoldenRatioConjugate * (upperBound - lowerBound);
      double leftValue = function(leftProbe);
      double rightValue = function(rightProbe);
      int iterationCount = 0;

      while ((upperBound - lowerBound) > precision)
      {
        if (leftValue < rightValue)
        {
          upperBound = rightProbe;
          rightProbe = leftProbe;
          rightValue = leftValue;
          leftProbe = upperBound - GoldenRatioConjugate * (upperBound - lowerBound);
          leftValue = function(leftProbe);
        }
        else
        {
          lowerBound = leftProbe;
          leftProbe = rightProbe;
          leftValue = rightValue;
          rightProbe = lowerBound + GoldenRatioConjugate * (upperBound - lowerBound);
          rightValue = function(rightProbe);
        }
        ++iterationCount;
      }

      double minimumPoint = (lowerBound + upperBound) / MidpointDivisor;
      double minimumValue = function(minimumPoint);
      return new OptimizationResult(minimumPoint, minimumValue, iterationCount, true, "Минимум найден.");
    }

    public static OptimizationResult FindMaximum(Func<double, double> function, double intervalStart, double intervalEnd, double precision)
    {
      ValidateArguments(function, intervalStart, intervalEnd, precision);
      Func<double, double> negatedFunction = argumentValue => NegationFactor * function(argumentValue);
      OptimizationResult innerResult = FindMinimum(negatedFunction, intervalStart, intervalEnd, precision);
      return new OptimizationResult(innerResult.Point, NegationFactor * innerResult.Value, innerResult.IterationCount, innerResult.Success, "Максимум найден.");
    }

    public static OptimizationResult FindRoot(Func<double, double> function, double intervalStart, double intervalEnd, double precision)
    {
      ValidateArguments(function, intervalStart, intervalEnd, precision);
      Func<double, double> absoluteFunction = argumentValue => Math.Abs(function(argumentValue));
      OptimizationResult innerResult = FindMinimum(absoluteFunction, intervalStart, intervalEnd, precision);

      double rootPoint = innerResult.Point;
      double rootValue = function(rootPoint);
      double absoluteValue = Math.Abs(rootValue);
      bool success = absoluteValue <= precision;
      string message = success
          ? "Корень найден."
          : $"Корень не найден с точностью e = {precision:G4}: |f(x*)| = {absoluteValue:G4}.";
      return new OptimizationResult(rootPoint, rootValue, innerResult.IterationCount, success, message);
    }

    private static void ValidateArguments(Func<double, double> function, double intervalStart, double intervalEnd, double precision)
    {
      if (function == null) throw new ArgumentNullException(nameof(function));
      if (!(intervalEnd > intervalStart)) throw new ArgumentException("Правый конец интервала должен быть строго больше левого.");
      if (!(precision > 0.0)) throw new ArgumentException("Точность должна быть положительной.");
    }
  }
}