using System;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace BetterGenshinImpact.Core.Recognition.OpenCv;


public class DepthMapHelper
{
    public Mat ComputeDepthMap(Mat leftImage, Mat rightImage, 
        double baseline, double focalLength)
    {
        int bs = 1;
        int p1 = 8 * leftImage.Channels() * bs;
        int p2 = 32 * leftImage.Channels() * bs;
        var stereoBM = StereoSGBM.Create(0, 256, bs);

        Mat disparityMap = new Mat();
        stereoBM.Compute(leftImage, rightImage, disparityMap);

        Mat depthMap = new Mat();
        Cv2.ConvertScaleAbs(disparityMap,depthMap,(float)(focalLength * baseline / 16.0));
        
        //depthMap = InsertDepth32f(depthMap);
        depthMap = OptimizeDepthMapVisuals(depthMap);
        return depthMap;
    }
    public Mat InsertDepth32f(Mat depth)
    {
        int width = depth.Width;
        int height = depth.Height;
        depth.GetArray(out byte[] data);

        Mat integralMap = new Mat(height, width, MatType.CV_64F, Scalar.All(0));
        Mat ptsMap = new Mat(height, width, MatType.CV_32S, Scalar.All(0));
        integralMap.GetArray(out double[] integral);
        ptsMap.GetArray(out int[] ptsIntegral);

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int id2 = i * width + j;
                if (data[id2] > 1e-3)
                {
                    integral[id2] = data[id2];
                    ptsIntegral[id2] = 1;
                }
                else
                {
                    integral[id2] = 0;
                    ptsIntegral[id2] = 0;
                }
            }
        }

        // 积分区间
        for (int i = 0; i < height; ++i)
        {
            for (int j = 1; j < width; ++j)
            {
                int id2 = i * width + j;
                integral[id2] += integral[id2 - 1];
                ptsIntegral[id2] += ptsIntegral[id2 - 1];
            }
        }
        for (int i = 1; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int id2 = i * width + j;
                integral[id2] += integral[id2 - width];
                ptsIntegral[id2] += ptsIntegral[id2 - width];
            }
        }

        int wnd;
        double dWnd = 2;
        while (dWnd > 1)
        {
            wnd = (int)dWnd;
            dWnd /= 2;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    int id2 = i * width + j;
                    int left = Math.Max(0, j - wnd - 1);
                    int right = Math.Min(j + wnd, width - 1);
                    int top = Math.Max(0, i - wnd - 1);
                    int bot = Math.Min(i + wnd, height - 1);
                    int dx = right - left;
                    int dy = (bot - top) * width;
                    int idLeftTop = top * width + left;
                    int idRightTop = idLeftTop + dx;
                    int idLeftBot = idLeftTop + dy;
                    int idRightBot = idLeftBot + dx;
                    int ptsCnt = ptsIntegral[idRightBot] + ptsIntegral[idLeftTop] - (ptsIntegral[idLeftBot] + ptsIntegral[idRightTop]);
                    double sumGray = integral[idRightBot] + integral[idLeftTop] - (integral[idLeftBot] + integral[idRightTop]);

                    if (ptsCnt <= 0)
                    {
                        continue;
                    }
                    data[id2] = (byte)(sumGray / ptsCnt);
                }
            }
        }

        // 将更新后的数据写回depth矩阵
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int id2 = i * width + j;
                depth.At<byte>(i, j) = data[id2];
            }
        }

        return depth;
    }
    public Mat OptimizeDepthMapVisuals(Mat depthMap)
    {

        // 1. 归一化深度图到0-255范围
        Mat normalizedDepth = depthMap;
        Cv2.Normalize(depthMap, normalizedDepth, 0, 255, NormTypes.MinMax, MatType.CV_8UC1);

        // 2. 应用高斯滤波来减少噪声
        Mat filteredDepth = normalizedDepth;
        Cv2.GaussianBlur(normalizedDepth, filteredDepth, new Size(5, 5), 0);

        // 3. 对比度增强（直方图均衡化）
        Mat enhancedDepth = filteredDepth;
        //Cv2.EqualizeHist(filteredDepth, enhancedDepth);

        // 4. 可选：应用锐化滤波来增强细节
        Mat sharpenedDepth = enhancedDepth;
        //Cv2.AddWeighted(enhancedDepth, 1.5, new Mat(), 0, -0.5, sharpenedDepth);

        return sharpenedDepth;
    }
}