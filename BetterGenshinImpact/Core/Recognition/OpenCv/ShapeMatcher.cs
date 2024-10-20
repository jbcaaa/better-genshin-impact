using System.Collections.Generic;
using System.Linq;
using OpenCvSharp.Aruco;

namespace BetterGenshinImpact.Core.Recognition.OpenCv;
using OpenCvSharp;

public class ShapeMatcher
{
    private Point[] _contour;
    
    public ShapeMatcher(Mat ori)
    {
        Cv2.CvtColor(ori,ori,ColorConversionCodes.BGR2GRAY);
        Point[][] contours; // 存储轮廓的变量
        HierarchyIndex[] hierarchy; // 存储轮廓拓扑结构的变量
        Cv2.FindContours(ori, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        int index = 0; // 模板中十字轮廓的轮廓序号
        //遍历每个轮廓
        for (int i = 0; i < contours.Length; i++)
        {
            // 获取轮廓的外接矩形
            Rect rect = Cv2.BoundingRect(contours[i]);
            
            if (rect.Width > 10 && rect.Height > 10)
            {
                index = i; // 记录找到的目标轮廓的索引
                // 在模板图像上绘制找到的目标轮廓，标记为红色
                Cv2.DrawContours(ori, contours, i, new Scalar(0, 0, 255), -1);
                break; // 找到目标轮廓后跳出循环
            }
        }
        _contour = contours[index];
    }

    public Rect getBestMatchRect(Mat grayTest)
    {
        var Temp = grayTest;
        List<(double,Rect)> matchResult = new List<(double, Rect)>();
        
        void Process(int thresh)
        {
            Cv2.Threshold(Temp, grayTest, thresh, 255, ThresholdTypes.Binary);
            //遍历测试图像中的轮廓做轮廓匹配
            Point[][] contours2; //轮廓查找结果变量
            HierarchyIndex[] hierarchy2; //轮廓拓扑结构变量

            Cv2.FindContours(grayTest, out contours2, out hierarchy2, RetrievalModes.External,
                ContourApproximationModes.ApproxNone);

            Dictionary<int, double> matchingResult = new Dictionary<int, double>();

            for (int i = 0; i < contours2.Length; i++)
            {
                double matchValue = Cv2.MatchShapes(_contour, contours2[i], ShapeMatchModes.I3);
                matchingResult.Add(i, matchValue);
            }

            double minMV = 114514;
            var index = 0;
            foreach (var aa in matchingResult.Keys)
            {
                if (matchingResult[aa] < minMV)
                {
                    minMV = matchingResult[aa];
                    index = aa;
                }
            }
            Rect rect = Cv2.BoundingRect(contours2[index]);
            matchResult.Add((minMV, rect));
        }
        Process(125);
        Process(25);
        Process(85);
        Process(175);
        var min = 19198.10;
        Rect result = new Rect();
        foreach (var aa in matchResult)
        {
            var a = aa.Item1;
            if (a < min)
            {
                min = a;
                result = aa.Item2;
            }
        }
        return result;
    }

    public Point[] getBestMatchContour(Mat grayTest)
    {
        var Temp = grayTest;
        List<(double,Point[])> matchResult = new List<(double, Point[])>();
        
        void Process(int thresh)
        {
            Cv2.Threshold(Temp, grayTest, thresh, 255, ThresholdTypes.Binary);
            //遍历测试图像中的轮廓做轮廓匹配
            Point[][] contours2; //轮廓查找结果变量
            HierarchyIndex[] hierarchy2; //轮廓拓扑结构变量

            Cv2.FindContours(grayTest, out contours2, out hierarchy2, RetrievalModes.External,
                ContourApproximationModes.ApproxNone);

            Dictionary<int, double> matchingResult = new Dictionary<int, double>();

            for (int i = 0; i < contours2.Length; i++)
            {
                double matchValue = Cv2.MatchShapes(_contour, contours2[i], ShapeMatchModes.I3);
                matchingResult.Add(i, matchValue);
            }

            double minMV = 114514;
            var index = 0;
            foreach (var aa in matchingResult.Keys)
            {
                if (matchingResult[aa] < minMV)
                {
                    minMV = matchingResult[aa];
                    index = aa;
                }
            }
            matchResult.Add((minMV, contours2[index]));
        }
        Process(125);
        Process(25);
        Process(85);
        Process(175);
        var min = 19198.10;
        List<Point> result = new List<Point>();
        foreach (var aa in matchResult)
        {
            var a = aa.Item1;
            if (a < min)
            {
                min = a;
                result = aa.Item2.ToList();
            }
        }

        return result.ToArray();
    }
}