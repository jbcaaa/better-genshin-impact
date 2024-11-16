using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Recognition.ONNX;
using BetterGenshinImpact.Core.Simulator;
using BetterGenshinImpact.GameTask.Model.Area;
using BetterGenshinImpact.View.Drawable;
using Compunet.YoloV8;
using static BetterGenshinImpact.GameTask.Common.TaskControl;
using OpenCvSharp;
using Microsoft.Extensions.Logging;

namespace BetterGenshinImpact.GameTask.AutoPathing;

public class EnergyManager
{
    private static YoloV8Predictor _predictor = YoloV8Builder.CreateDefaultBuilder()
        .UseOnnxModel(Global.Absolute(@"Assets\Model\Pathing\EnergyManager.onnx"))
        .WithSessionOptions(BgiSessionOption.Instance.Options)
        .Build();

    public static string GetStatus(ImageRegion region)
    {
        var memoryStream = new MemoryStream();
        region.SrcBitmap.Save(memoryStream, ImageFormat.Bmp);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var result = _predictor.Detect(memoryStream);
        if (result.Boxes.Length > 0)
        {
            return result.Boxes[0].Class.Name;
        }

        return "much";
    }
}