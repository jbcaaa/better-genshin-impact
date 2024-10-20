using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Recognition.OpenCv;
using BetterGenshinImpact.Core.Recognition.OpenCv.FeatureMatch;
using OpenCvSharp;
using static BetterGenshinImpact.GameTask.Common.TaskControl;
namespace BetterGenshinImpact.GameTask.AutoPathing;

public class EnergyBarFinder
{
    //public static readonly FeatureStorage _energyBarStorage = new(@"..\..\GameTask\AutoPathing\Assets\EnergyBar.png");
    
    //这一大行代码其实就干一件事：加载原图像
    private static Mat image = Cv2.ImRead(Global.Absolute(@"GameTask\AutoPathing\Assets\EnergyBar.png"), ImreadModes.Grayscale);
    private static FeatureMatcher _featureMatcher
    {
        get
        {
            return new FeatureMatcher(CaptureToRectArea().SrcGreyMat, type:Feature2DType.FAST);
        }
    }
    public static double x
    {
        get
        {
            return Rectangle.X;
        }
    }
    public static double y
    {
        get
        {
            return Rectangle.Y;
        }
    }
    public static double width
    {
        get
        {
            return Rectangle.Width;
        }
    }
    public static double height
    {
        get
        {
            return Rectangle.Height;
        }
    }
    public static Rect Rectangle
    {
        get
        {
            return _featureMatcher.KnnMatchRect(image);
        }
    }
    
    public static Rect getRect(){
        return Rectangle;
    }
}