using System;
using System.Collections.Generic;
using BetterGenshinImpact.Core.Simulator;
using static BetterGenshinImpact.GameTask.Common.TaskControl;
using OpenCvSharp;
using Microsoft.Extensions.Logging;

namespace BetterGenshinImpact.GameTask.AutoPathing;

public class EnergyManager
{
    private static double _maxPixels;
    private static double _pixels;

    public static double EnergyPercent
    {
        get
        {
            init();
            var percent = (double)_pixels *100 / _maxPixels;
            if (percent <= 5) //判断体力条是满的还是空的
            {
                Simulation.SendInput.Mouse.RightButtonClick();
                Sleep(500);
                init();
                percent = (double)_pixels *100 / _maxPixels;
            }
            Logger.LogInformation("体力条当前像素:{0}/{1}，当前体力:{2}", _pixels, _maxPixels, (int)percent);
            return percent;
        }
    } 
    public static void init()
    {
        (_pixels, _maxPixels) = CountPixelsFromScreen();
    }
    
    public static bool biggerThan(double x)
    {
        return EnergyPercent > x;
    }
    public static bool smallerThan(double x)
    {
        return EnergyPercent < x;
    }

    private static (double, double) CountPixelsFromScreen()
    {
        //Cv2.CvtColor(imagemat, imagemat, ColorConversionCodes.BGR2GRAY,MatType.CV_8UC1);
        Rect rectangle = EnergyBarFinder.getRect();
        var screen = new Mat(CaptureToRectArea().SrcMat, rectangle);
        var w = rectangle.Width;
        var h = rectangle.Height;
        var Y = rectangle.Y - h;
        List<double> x = new List<double>();
        List<double> y = new List<double>();
        /*
        foreach (var v in contours)
        {
            x.Add(v.X);
            y.Add(v.Y);
        }
        var circle = LeastSquaresFit(x.ToArray(), y.ToArray());*/
        double count1 = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                var pixel = screen.At<Vec3b>(i, j);
                var b = pixel[0];
                var g = pixel[1];
                var r = pixel[2];
                if (b == 255 || g == 255 || r == 255)
                {
                    count1++;
                }
            }
        }

        return (count1, h);
    }
    
    public static Circle LeastSquaresFit(double[] X, double[] Y)
    {
        if (X.Length < 3)
        {
            return null;
        }
        double cent_x = 0.0,
            cent_y = 0.0,
            radius = 0.0;
        double sum_x = 0.0f, sum_y = 0.0f;
        double sum_x2 = 0.0f, sum_y2 = 0.0f;
        double sum_x3 = 0.0f, sum_y3 = 0.0f;
        double sum_xy = 0.0f, sum_x1y2 = 0.0f, sum_x2y1 = 0.0f;
        int N = X.Length;
        double x, y, x2, y2;
        for (int i = 0; i < N; i++)
        {
            x = X[i];
            y = Y[i];
            x2 = x * x;
            y2 = y * y;
            sum_x += x;
            sum_y += y;
            sum_x2 += x2;
            sum_y2 += y2;
            sum_x3 += x2 * x;
            sum_y3 += y2 * y;
            sum_xy += x * y;
            sum_x1y2 += x * y2;
            sum_x2y1 += x2 * y;
        }
        double C, D, E, G, H;
        double a, b, c;
        C = N * sum_x2 - sum_x * sum_x;
        D = N * sum_xy - sum_x * sum_y;
        E = N * sum_x3 + N * sum_x1y2 - (sum_x2 + sum_y2) * sum_x;
        G = N * sum_y2 - sum_y * sum_y;
        H = N * sum_x2y1 + N * sum_y3 - (sum_x2 + sum_y2) * sum_y;
        a = (H * D - E * G) / (C * G - D * D);
        b = (H * C - E * D) / (D * D - G * C);
        c = -(a * sum_x + b * sum_y + sum_x2 + sum_y2) / N;
        cent_x = a / (-2);
        cent_y = b / (-2);
        radius = Math.Sqrt(a * a + b * b - 4 * c) / 2;
        var result = new Circle();
        result.X = cent_x;
        result.Y = cent_y;
        result.R = radius;
        return result;
    }

    
}