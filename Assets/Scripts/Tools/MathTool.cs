using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.Mathematics
{
    public static class MathTool
    {
        public static Rect GetBottomRect(this Bounds bounds)
        {
            var boundsMin = bounds.min;
            var boundsSize = bounds.size;
            return new Rect(boundsMin.x, bounds.min.z, boundsSize.x, boundsSize.z);
        }
        public static bool DoRectAndRayIntersect(Rect rect, Vector2 point, Vector2 dir, out Vector2 intersection)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 line1, line2;
                switch (i)
                {
                    default:
                        line1 = new Vector2(rect.xMin, rect.yMin);
                        line2 = new Vector2(rect.xMin, rect.yMax);
                        break;
                    case 1:
                        line1 = new Vector2(rect.xMin, rect.yMax);
                        line2 = new Vector2(rect.xMax, rect.yMax);
                        break;
                    case 2:
                        line1 = new Vector2(rect.xMax, rect.yMax);
                        line2 = new Vector2(rect.xMax, rect.yMin);
                        break;
                    case 3:
                        line1 = new Vector2(rect.xMax, rect.yMin);
                        line2 = new Vector2(rect.xMin, rect.yMin);
                        break;
                }
                if (DoLineAndRayIntersect(line1, line2, point, dir, out intersection))
                {
                    return true;
                }
            }
            intersection = Vector2.zero;
            return false;
        }
        public static bool DoLinesIntersect(Vector2 lineA1, Vector2 lineA2, Vector2 lineB1, Vector2 lineB2)
        {
            return DoLinesIntersect(lineA1, lineA2, lineB1, lineB2, out _);
        }
        public static bool DoLinesIntersect(Vector2 lineA1, Vector2 lineA2, Vector2 lineB1, Vector2 lineB2, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            Vector2 A1ToB1 = lineA1 - lineB1;
            Vector2 A2ToB1 = lineA2 - lineB1;
            Vector2 A1ToB2 = lineA1 - lineB2;
            Vector2 A2ToB2 = lineA2 - lineB2;
            // 三角形abc 面积的2倍  
            float area_abc = A1ToB1.Cross(A2ToB1);

            // 三角形abd 面积的2倍  
            float area_abd = A1ToB2.Cross(A2ToB2);

            // 面积符号相同则两点在线段同侧,不相交 (对点在线段上的情况,本例当作不相交处理);  
            if (area_abc * area_abd >= 0)
            {
                return false;
            }

            Vector2 B1ToA1 = lineB1 - lineA1;
            Vector2 B2ToA1 = lineB2 - lineA1;
            // 三角形cda 面积的2倍  
            var area_cda = B1ToA1.Cross(B2ToA1);
            // 三角形cdb 面积的2倍  
            // 注意: 这里有一个小优化.不需要再用公式计算面积,而是通过已知的三个面积加减得出.  
            var area_cdb = area_cda + area_abc - area_abd;
            if (area_cda * area_cdb >= 0)
            {
                return false;
            }

            //计算交点坐标  
            float distanceToA1 = area_cda / (area_abd - area_abc);
            Vector2 d = distanceToA1 * (lineA2 - lineA1);
            intersection = lineA1 + d;
            return true;

        }

        public static bool DoRectsOverlap(Vector2 rectCenter1, Vector2 rectSize1, Vector2 rectCenter2, Vector2 rectSize2)
        {
            var rect1ExtentX = rectSize1.x * 0.5f;
            var rect1ExtentY = rectSize1.y * 0.5f;
            var rect2ExtentX = rectSize2.x * 0.5f;
            var rect2ExtentY = rectSize2.y * 0.5f;
            if (!DoRangesIntersect(rectCenter1.x - rect1ExtentX, rectCenter1.x + rect1ExtentX, rectCenter2.x - rect2ExtentX, rectCenter2.x + rect2ExtentX))
                return false;
            if (!DoRangesIntersect(rectCenter1.y - rect1ExtentY, rectCenter1.y + rect1ExtentY, rectCenter2.y - rect2ExtentY, rectCenter2.y + rect2ExtentY))
                return false;
            return true;
        }

        private static bool DoLineAndRayIntersect(Vector2 line1, Vector2 line2, Vector2 point, Vector2 dir, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            // 判断A1和A2是否在射线的两端
            Vector2 line1ToPoint = line1 - point;
            Vector2 line2ToPoint = line2 - point;
            Vector2 lineDir = line2 - line1;
            float line1CrossRay = line1ToPoint.Cross(dir);
            float line2CrossRay = line2ToPoint.Cross(dir);
            float lineCrossPoint = lineDir.Cross(line2ToPoint);
            float lineCrossPointRay = lineDir.Cross(line2 - (point + dir));

            // A1和A2在射线的同一侧，不相交
            if (line1CrossRay * line2CrossRay > 0)
            {
                return false;
            }
            // 线段A在射线的另一侧
            if (Mathf.Abs(lineCrossPointRay) > Mathf.Abs(lineCrossPoint))
            {
                return false;
            }

            // 如果line是竖线
            if (lineDir.x == 0)
            {
                // 如果都是竖线
                if (dir.x == 0)
                {
                    return false;
                }
                // 如果只有line是竖线
                else
                {
                    float raySlope = dir.y / dir.x;
                    float rayOffset = point.y - point.x * raySlope;

                    float x = line1.x;
                    float y = raySlope * line1.x + rayOffset;
                    intersection = new Vector2(x, y);
                    return true;
                }
            }
            else
            {
                // 如果只有dir是竖线
                if (dir.x == 0)
                {
                    float lineSlope = lineDir.y / lineDir.x;
                    float lineOffset = line1.y - line1.x * lineSlope;

                    float x = point.x;
                    float y = lineSlope * point.x + lineOffset;
                    intersection = new Vector2(x, y);
                    return true;
                }
                // 如果两个都不是竖线
                else
                {
                    // ax1 + b = y1
                    // ax2 + b = y2
                    // a = (y2 - y1) / (x2 - x1)
                    // b = y - ax;
                    float lineSlope = lineDir.y / lineDir.x;
                    float lineOffset = line1.y - line1.x * lineSlope;

                    float raySlope = dir.y / dir.x;
                    float rayOffset = point.y - point.x * raySlope;

                    // ax + b = y;
                    // cx + d = y;
                    // (a - c)x + (b - d) = 0;
                    // x = (d - b) / (a - c);
                    float x = (rayOffset - lineOffset) / (lineSlope - raySlope);
                    float y = lineSlope * x + lineOffset;
                    intersection = new Vector2(x, y);
                    return true;
                }
            }
        }
        private static bool DoLinesIntersectNormal(Vector2 lineA1, Vector2 lineA2, Vector2 lineB1, Vector2 lineB2, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            //线段A的法线NormalA  
            Vector2 normalA = new Vector2(lineA2.y - lineA1.y, lineA1.x - lineA2.x);

            //线段B的法线NormalB  
            Vector2 normalB = new Vector2(lineB2.y - lineB1.y, lineB1.x - lineB2.x);

            //两条法线做叉乘, 如果结果为0, 说明线段A和线段B平行或共线,不相交  
            var denominator = normalA.Cross(normalB);
            if (denominator == 0)
            {
                return false;
            }

            //在法线NormalB上的投影  
            var distB1_NB = Vector2.Dot(normalB, lineB1);
            var distA1_NB = Vector2.Dot(normalB, lineA1) - distB1_NB;
            var distA2_NB = Vector2.Dot(normalB, lineA2) - distB1_NB;

            // 点A1投影和点A2投影在点B1投影同侧 (对点在线段上的情况,本例当作不相交处理);  
            if (distA1_NB * distA2_NB >= 0)
            {
                return false;
            }

            //  
            //判断点B1点B2和线段A的关系, 原理同上  
            //  
            //在法线N1上的投影  
            var distA1_NA = Vector2.Dot(normalA, lineA1);
            var distB1_NA = Vector2.Dot(normalA, lineB1) - distA1_NA;
            var distB2_NA = Vector2.Dot(normalA, lineB2) - distA1_NA;
            if (distB1_NA * distB2_NA >= 0)
            {
                return false;
            }

            //计算交点坐标  
            var fraction = distA1_NB / denominator;
            Vector2 distance = new Vector2(fraction * normalA.y, -fraction * normalA.x);
            intersection = lineA1 + distance;
            return true;

        }

        public static float Cross(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.x * vector2.y - vector1.y * vector2.x;
        }

        public static bool DoRangesIntersect(float start1, float end1, float start2, float end2)
        {
            return Mathf.Max(start1, end1) >= Mathf.Min(start2, end2) && Mathf.Min(start1, end1) <= Mathf.Max(start2, end2);
        }
        /// <summary>
        /// 以抛物线形式进行Lerp。
        /// </summary>
        /// <param name="a">起始值。</param>
        /// <param name="b">结束值。</param>
        /// <param name="highestHeight">最大值。</param>
        /// <param name="t">插值。</param>
        /// <param name="isInner">结束值是否在上升阶段。</param>
        /// <returns>最终值。</returns>
        public static float LerpParabolla(float a, float b, float highestHeight, float t, bool isInner = false)
        {
            return GetParabollaY(0, 1, a, b, highestHeight, t, isInner);
        }

        /// <summary>
        /// 获取经过两个点的抛物线，并返回x值为variable时的y值。
        /// </summary>
        /// <param name="startX">点1的x值</param>
        /// <param name="endX">点2的x值</param>
        /// <param name="startY">点1的y值</param>
        /// <param name="endY">点2的y值</param>
        /// <param name="highestHeight">最大高度。</param>
        /// <param name="variable">当前x值。</param>
        /// <param name="isInner">点2是否在点1与对称轴之间。</param>
        /// <returns>抛物线在x值为variable时的y值。</returns>
        public static float GetParabollaY(float startX, float endX, float startY, float endY, float highestHeight, float variable, bool isInner = false)
        {
            float y = endY - startY;
            float x = endX - startX;

            // 当x = 0时，有无数个结果，故返回0
            if (x == 0)
            {
                //Debug.LogError("抛物线两点x轴距离为0。");
                return 0;
            }

            float k = highestHeight;

            if (k > 0)
            {
                if (y > k)
                {
                    Debug.LogError("最大高度过小，无法达到y值。");
                    return 0;
                }
            }
            else if (k < 0)
            {
                if (y < k)
                {
                    Debug.LogError("最大高度过大，无法达到y值。");
                    return 0;
                }
            }
            else
            {
                Debug.LogError("最大高度为0，无法进行运算。");
                return 0;
            }

            float h;

            if (y == 0)
            {
                h = x / 2;
            }
            else
            {
                h = (k * x - Mathf.Sqrt(k * x * x * (k - y)) * (isInner ? -1 : 1)) / y;
            }

            float a = -k / h / h;
            return a * Mathf.Pow((variable - startX) - h, 2) + k + startY;
        }

        public static bool CollideBetweenCubeAndCylinder(Cylinder cylinder, Bounds cube)
        {
            Rect cylinderRectSide;
            Rect cubeRectSide;
            Vector2 cylinderCenterTop;
            Rect cubeRectTop;
            switch (cylinder.axis)
            {
                case Axis.X:
                    {
                        cylinderRectSide = new Rect(cylinder.center.x - cylinder.length * 0.5f, cylinder.center.y - cylinder.radius, cylinder.length, cylinder.radius * 2);
                        cubeRectSide = new Rect(cube.min.x, cube.min.y, cube.size.x, cube.size.y);

                        cylinderCenterTop = new Vector2(cylinder.center.z, cylinder.center.y);
                        cubeRectTop = new Rect(cube.min.z, cube.min.y, cube.size.z, cube.size.y);
                    }
                    break;
                case Axis.Y:
                    {
                        cylinderRectSide = new Rect(cylinder.center.x - cylinder.radius, cylinder.center.y - cylinder.length * 0.5f, cylinder.radius * 2, cylinder.length);
                        cubeRectSide = new Rect(cube.min.x, cube.min.y, cube.size.x, cube.size.y);

                        cylinderCenterTop = new Vector2(cylinder.center.x, cylinder.center.z);
                        cubeRectTop = new Rect(cube.min.x, cube.min.z, cube.size.x, cube.size.z);
                    }
                    break;
                case Axis.Z:
                    {
                        cylinderRectSide = new Rect(cylinder.center.x - cylinder.radius, cylinder.center.z - cylinder.length * 0.5f, cylinder.radius * 2, cylinder.length);
                        cubeRectSide = new Rect(cube.min.x, cube.min.z, cube.size.x, cube.size.z);

                        cylinderCenterTop = new Vector2(cylinder.center.x, cylinder.center.y);
                        cubeRectTop = new Rect(cube.min.x, cube.min.y, cube.size.x, cube.size.y);
                    }
                    break;
                default:
                    return false;
            }

            // 检测圆柱体侧面1是否与方块侧面相交。
            if (!cubeRectSide.Overlaps(cylinderRectSide))
                return false;

            return CollideBetweenRectangleAndCircle(cylinderCenterTop, cylinder.radius, cubeRectTop.center, cubeRectTop.size);
        }
        public static bool CollideBetweenCubeAndRoundCube(RoundCube roundCube, Bounds cube)
        {
            return CollideBetweenCubeAndRoundCube(roundCube.center, roundCube.size, roundCube.radius, cube.center, cube.size);
        }
        public static bool CollideBetweenCubeAndRoundCube(Vector3 roundCubeCenter, Vector3 roundCubeSize, float roundCubeRadius, Vector3 cubeCenter, Vector3 cubeSize)
        {
            // 检测x-y平面和x-z平面的投影相交。
            for (int i = 0; i < 2; i++)
            {
                var comp1 = 0;
                var comp2 = 1;
                if (i == 1)
                {
                    comp2 = 2;
                }
                var roundCenter = new Vector2(roundCubeCenter[comp1], roundCubeCenter[comp2]);
                var roundSize = new Vector2(roundCubeSize[comp1], roundCubeSize[comp2]);

                var rectCenter = new Vector2(cubeCenter[comp1], cubeCenter[comp2]);
                var rectSize = new Vector2(cubeSize[comp1], cubeSize[comp2]);
                if (CollideBetweenRectangleAndRoundRectangle(roundCenter, roundSize, roundCubeRadius, rectCenter, rectSize))
                    return true;
            }
            return false;
        }
        public static bool CollideBetweenCubeAndSphere(Vector3 sphereCenter, float sphereRadius, Vector3 cubeCenter, Vector3 cubeScale)
        {
            // 将目标球的坐标(world Potition)转换为立方体的 localPosition
            Vector3 spherePos = sphereCenter - cubeCenter;

            // 使用上面方法计算的 localPos 会受到 立方体 transform.localScale 影响
            // 当立方体 transform.localScale = Vector3.one 时计算结果为 (x, y, z)
            // 当立方体 trnasofrm.localScale = new Vector3(a, b, c) 时, 计算所得 localPos = (x / a, y / b, z / c)
            // 通过下面计算将结果转换
            // spherePos.x *= transform.localScale.x;
            // spherePos.y *= transform.localScale.y;
            // spherePos.z *= transform.localScale.z;

            // 将 localPos x、y、z 分别于 max、min 的 x、y、z 做比较
            //  x 取值范围 (min.x, max.x)
            //  y 取值范围 (min.y, max.y)
            //  z 取值范围 (min.z, max.z)
            float x = spherePos.x;
            x = Mathf.Clamp(x, -cubeScale.x * 0.5f, cubeScale.x * 0.5f);

            float y = spherePos.y;
            y = Mathf.Clamp(y, -cubeScale.y * 0.5f, cubeScale.y * 0.5f);

            float z = spherePos.z;
            z = Mathf.Clamp(z, -cubeScale.z * 0.5f, cubeScale.z * 0.5f);

            // x、y、z 重新取值后得到新坐标
            Vector3 pos = new Vector3(x, y, z);

            // 求新坐标 pos 与 localPos 的距离
            float distance = (spherePos - pos).magnitude;
            // 距离大于半径则不相交
            return distance <= sphereRadius;
        }
        public static bool CollideBetweenRectangleAndCircle(Vector2 circleCenter, float circleRadius, Vector2 rectCenter, Vector2 rectScale)
        {
            // 将目标圆的坐标(world Potition)转换为矩形的 localPosition
            Vector2 circlePos = circleCenter - rectCenter;

            // 使用上面方法计算的 circlePos 会受到 矩形 rectScale 影响
            // 当矩形 rectScale = Vector2.one 时计算结果为 (x, y)
            // 当矩形 rectScale = new Vector2(a, b) 时, 计算所得 localPos = (x / a, y / b)
            // 通过下面计算将结果转换
            // circlePos.x *= rectScale.x;
            // circlePos.y *= rectScale.y;

            // 将 circlePos x、y 分别于 max、min 的 x、y 做比较
            //  x 取值范围 (min.x, max.x)
            //  y 取值范围 (min.y, max.y)
            float x = circlePos.x;
            x = Mathf.Clamp(x, -rectScale.x * 0.5f, rectScale.x * 0.5f);

            float y = circlePos.y;
            y = Mathf.Clamp(y, -rectScale.y * 0.5f, rectScale.y * 0.5f);

            // x、y 重新取值后得到新坐标
            Vector2 pos = new Vector2(x, y);

            // 求新坐标 pos 与 circlePos 的距离
            float distance = (circlePos - pos).magnitude;
            // 距离大于半径则不相交
            return distance <= circleRadius;
        }
        public static bool CollideBetweenRectangleAndRoundRectangle(RoundRect roundRect, Rect rectangle)
        {
            var roundRectCenter = new Rect(roundRect.center - roundRect.size * 0.5f, roundRect.size);
            if (roundRect.radius <= 0)
                return roundRectCenter.Overlaps(rectangle);

            // 水平相交
            var roundSizeHori = roundRect.size + Vector2.right * roundRect.radius;
            var roundRectHori = new Rect(roundRect.center - roundSizeHori * 0.5f, roundSizeHori);
            if (rectangle.Overlaps(roundRectHori))
                return true;

            // 垂直相交
            var roundSizeVert = roundRect.size + Vector2.up * roundRect.radius;
            var roundRectVert = new Rect(roundRect.center - roundSizeVert * 0.5f, roundSizeVert);
            if (rectangle.Overlaps(roundRectVert))
                return true;

            // 圆角相交
            for (int i = 0; i < 4; i++)
            {
                var x = (i & 1) == 1 ? roundRectCenter.xMax : roundRectCenter.xMin;
                var y = (i & 2) == 2 ? roundRectCenter.yMax : roundRectCenter.yMin;
                var corner = new Vector2(x, y);
                if (CollideBetweenRectangleAndCircle(corner, roundRect.radius, rectangle.center, rectangle.size))
                    return true;
            }
            return false;
        }
        public static bool CollideBetweenRectangleAndRoundRectangle(Vector2 roundRectCenter, Vector2 roundRectSize, float roundRectRadius, Vector2 rectangleCenter, Vector2 rectangleSize)
        {
            if (roundRectRadius <= 0)
                return DoRectsOverlap(roundRectCenter, roundRectSize, rectangleCenter, rectangleSize);

            // 水平相交
            var roundSizeHori = roundRectSize + Vector2.right * roundRectRadius * 2;
            if (DoRectsOverlap(roundRectCenter, roundSizeHori, rectangleCenter, rectangleSize))
                return true;

            // 垂直相交
            var roundSizeVert = roundRectSize + Vector2.up * roundRectRadius * 2;
            if (DoRectsOverlap(roundRectCenter, roundSizeVert, rectangleCenter, rectangleSize))
                return true;

            // 圆角相交
            for (int i = 0; i < 4; i++)
            {
                var x = roundRectCenter.x + roundRectSize.x * ((i & 1) == 1 ? -0.5f : 0.5f);
                var y = roundRectCenter.y + roundRectSize.y * ((i & 2) == 2 ? -0.5f : 0.5f);
                var corner = new Vector2(x, y);
                if (CollideBetweenRectangleAndCircle(corner, roundRectRadius, rectangleCenter, rectangleSize))
                    return true;
            }
            return false;
        }
        public static float EaseIn(float x)
        {
            x = Mathf.Clamp01(x);
            return Mathf.Pow(x, 2);
        }
        public static float EaseOut(float x)
        {
            x = Mathf.Clamp01(x);
            return 1 - Mathf.Pow(x - 1, 2);
        }
        public static float EaseInAndOut(float x)
        {
            x = Mathf.Clamp01(x);
            if (x < 0.5f)
            {
                return EaseIn(2 * x) * 0.5f;
            }
            else
            {
                return EaseOut((2 * x - 1)) * 0.5f + 0.5f;
            }
        }

        public static bool AABBSweep(Vector3 Ea, Vector3 Eb, Vector3 A1, Vector3 B1, Vector3 prevA, Vector3 prevB, out float firstTime, out float lastTime)
        {
            Bounds prevBoxA = new Bounds(prevA, Ea);//previous state of AABB A
            Bounds prevBoxB = new Bounds(prevB, Eb);//previous state of AABB B
            Vector3 displacementA = A1 - prevA;//displacement of A
            Vector3 displacementB = B1 - prevB;//displacement of B
            //the problem is solved in A's frame of reference

            Vector3 relativeDisplacement = displacementB - displacementA;
            //relative velocity (in normalized time)

            Vector3 firstTimeAxises = new Vector3(0, 0, 0);
            //first times of overlap along each axis

            Vector3 lastTimeAxises = new Vector3(1, 1, 1);
            //last times of overlap along each axis

            //check if they were overlapping
            // on the previous frame
            if (prevBoxA.Intersects(prevBoxB))
            {
                firstTime = 0;
                lastTime = 0;
                return true;

            }

            //find the possible first and last times
            //of overlap along each axis
            for (int i = 0; i < 3; i++)
            {
                var minA = prevBoxA.min[i];
                var maxA = prevBoxA.max[i];
                var minB = prevBoxB.min[i];
                var maxB = prevBoxB.max[i];
                var displacement = relativeDisplacement[i];
                if (maxA < minB && displacement < 0)
                {
                    firstTimeAxises[i] = (maxA - minB) / displacement;
                }
                else if (maxB < minA && displacement > 0)
                {
                    firstTimeAxises[i] = (minA - maxB) / displacement;
                }

                if (maxB > minA && displacement < 0)
                {
                    lastTimeAxises[i] = (minA - maxB) / displacement;
                }
                else if (maxA > minB && displacement > 0)
                {
                    lastTimeAxises[i] = (maxA - minB) / displacement;
                }
            }

            //possible first time of overlap
            firstTime = Mathf.Max(firstTimeAxises.x, firstTimeAxises.y, firstTimeAxises.z);

            //possible last time of overlap
            lastTime = Mathf.Min(lastTimeAxises.x, lastTimeAxises.y, lastTimeAxises.z);

            //they could have only collided if
            //the first time of overlap occurred
            //before the last time of overlap
            return firstTime <= lastTime;
        }
        public static bool Intersects(this Rect rect, Rect target)
        {
            if (rect.yMin > target.yMax)
            {
                return false;
            }
            if (rect.yMax < target.yMin)
            {
                return false;
            }
            if (rect.xMax < target.xMin)
            {
                return false;
            }
            if (rect.xMin > target.xMax)
            {
                return false;
            }
            return true;
        }
        public static bool CollideBetweenCubeAndCapsule(Capsule capsule, Vector3 cubeCenter, Vector3 cubeSize)
        {
            // 扩展立方体
            var expandedSize = cubeSize + Vector3.one * capsule.radius * 2;
            var capsuleStart = capsule.GetPoint1();
            var capsuleEnd = capsule.GetPoint2();

            // 检测线段与扩展后的AABB是否相交
            return CollideBetweenCubeAndLine(capsuleStart, capsuleEnd, cubeCenter, expandedSize);
        }
        public static bool CollideBetweenCubeAndLine(Vector3 lineStart, Vector3 lineEnd, Vector3 cubeCenter, Vector3 cubeSize)
        {
            var t_min = 0f;
            var t_max = 1f;
            var cubeMin = cubeCenter - cubeSize * 0.5f;
            var cubeMax = cubeCenter + cubeSize * 0.5f;

            for (int axis = 0; axis < 3; axis++)
            {
                var a = lineStart[axis];
                var b = lineEnd[axis];
                var minAxis = cubeMin[axis];
                var maxAxis = cubeMax[axis];
        
                if (Mathf.Abs(b - a) < 1e-6)  // 线段平行于当前轴
                {
                    if (a < minAxis || a > maxAxis)
                        return false;
                }
                else
                {
                    var inverseDistance = 1f / (b - a);
                    var t1 = (minAxis - a) * inverseDistance;
                    var t2 = (maxAxis - a) * inverseDistance;
                    if (t1 > t2)
                    {
                        var swap = t1;
                        t1 = t2;
                        t2 = swap;
                    }
                    t_min = Mathf.Max(t_min, t1);
                    t_max = Mathf.Min(t_max, t2);
                    if (t_min > t_max)
                        return false;
                }
            }

            return (t_min <= 1) && (t_max >= 0);
        }
    }
    public enum Axis
    {
        X,
        Y,
        Z
    }
    public struct Cylinder
    {
        public Cylinder(Axis axis, Vector3 center, float length, float radius)
        {
            this.axis = axis;
            this.center = center;
            this.length = length;
            this.radius = radius;
        }
        public Axis axis;
        public Vector3 center;
        public float length;
        public float radius;
    }
    public struct Capsule
    {
        public Capsule(Axis axis, Vector3 center, float length, float radius)
        {
            this.axis = axis;
            this.center = center;
            this.length = length;
            this.radius = radius;
        }
        public Vector3 GetDirection()
        {
            switch (axis)
            {
                case Axis.X:
                    return Vector3.right;
                case Axis.Y:
                    return Vector3.up;
                case Axis.Z:
                    return Vector3.forward;
                default:
                    return Vector3.zero;
            }
        }
        public Vector3 GetPoint1()
        {
            return center + GetDirection() * length * 0.5f;
        }
        public Vector3 GetPoint2()
        {
            return center - GetDirection() * length * 0.5f;
        }
        public Axis axis;
        public Vector3 center;
        public float length;
        public float radius;
    }
    public struct RoundRect
    {
        public RoundRect(Vector2 center, Vector2 size, float radius)
        {
            this.center = center;
            this.size = size;
            this.radius = radius;
        }
        public Vector2 center;
        public Vector2 size;
        public float radius;
    }
    public struct RoundCube
    {
        public RoundCube(Vector3 center, Vector3 size, float radius)
        {
            this.center = center;
            this.size = size;
            this.radius = radius;
        }
        public Vector3 center;
        public Vector3 size;
        public float radius;
    }
}