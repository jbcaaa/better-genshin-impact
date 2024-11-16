using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ABI.System;
using BetterGenshinImpact.Core.Recognition.OpenCv;
using BetterGenshinImpact.Core.Recognition.OpenCv.FeatureMatch;
using OpenCvSharp;
using static BetterGenshinImpact.GameTask.Common.TaskControl;
using Exception = System.Exception;
using BetterGenshinImpact.Core.Simulator;
using Vanara.PInvoke;

namespace BetterGenshinImpact.GameTask.AutoPathing;

public class DepthCalculator
{
    private Mat _depthMat = new Mat();
    private readonly DepthMapHelper _depthMapHelper = new DepthMapHelper();
    public double LastTimer = 0;
    
    private void Calc(Mat leftImage, Mat rightImage,
        double baseline, double focalLength)
    {
        _depthMat = _depthMapHelper.ComputeDepthMap(leftImage, rightImage, baseline, focalLength);
    }
    
    private void Calc(Mat leftImage, Mat rightImage)
    {
        Calc(leftImage,rightImage,1,1);
    }

    public Mat GetDepthMat()
    {
        Mat normalizedDepthMap = new Mat();
        Cv2.Normalize(_depthMat, normalizedDepthMap, 0, 255, NormTypes.MinMax, -1, new Mat());
        return normalizedDepthMap;
    }

    private double GetDepth(Rect res)
    {
        var image = new Mat(_depthMat, res);
        var depth = new List<double>();
        for (int i = 0; i < image.Rows; i++) {
            for (int j = 0; j < image.Cols; j++) {
                depth.Add(image.At<byte>(i,j));
            }
        }

        return depth.Min();
    }
    
    public double GetDepth(Mat image)
    {
        Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
        var featureMatcher = new FeatureMatcher(_depthMat);
        Rect p = new Rect();
        try
        {
            p = featureMatcher.KnnMatchRect(image);
        }
        catch (Exception)
        {
            
        }
        return GetDepth(p);
    }

    public async Task AutoCalc(CancellationToken ct)
    {
        await Delay(500, ct);
        var imageb = CaptureToRectArea();
        
        Simulation.SendInput.Keyboard.KeyDown(User32.VK.VK_A);
        await Delay(750, ct);
        Simulation.SendInput.Keyboard.KeyUp(User32.VK.VK_A);
        await Delay(1000, ct);
        var imagea = CaptureToRectArea();
        
        Simulation.SendInput.Keyboard.KeyDown(User32.VK.VK_D);
        await Delay(750, ct);
        Simulation.SendInput.Keyboard.KeyUp(User32.VK.VK_D);
        
        Calc(imageb.SrcGreyMat, imagea.SrcGreyMat);
        //Cv2.ImShow("114", GetDepthMat());
        //Cv2.WaitKey(30);
    }

    public async Task AutoCalcTimer(CancellationToken ct)
    {
        var startTime = DateTime.UtcNow;
        await AutoCalc(ct);
        LastTimer = (DateTime.UtcNow - startTime).TotalMilliseconds;
    }
}