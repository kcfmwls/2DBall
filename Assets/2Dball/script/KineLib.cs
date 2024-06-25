using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KineLib
{
    public const float MAXMAP = 196000;

    /// <summary>
    /// lineStart != lineEnd
    /// 多边形碰撞
    /// 几何
    /// </summary>
    public static float CalculateCollision(Vector2 ballPos, Vector2 ballVelocity, float ballRadius, Vector2 lineStart, Vector2 lineEnd, Vector2 lineNormal, out Vector2 resPos, out Vector2 resVel)
    {
        resPos = ballPos;
        resVel = ballVelocity;
        //速度和法线同向或垂直
        if (Vector2.Dot(ballVelocity, lineNormal) >= -Vector2.kEpsilon)
            return -1;

        float time;
        Vector2 line = lineEnd - lineStart;
        Vector2 collisionStart = lineStart + ballRadius * lineNormal;
        Vector2 collisionEnd = lineEnd + ballRadius * lineNormal;
        Vector2 posToCS = collisionStart - ballPos;
        Vector2 posToCE = collisionEnd - ballPos;
        //速度方向在锐角夹角之内，其会撞到线段内
        if (V2Cross(ballVelocity, posToCS) * V2Cross(ballVelocity, posToCE) <= 0)
        {
            time = (V2Cross(collisionStart, line) + V2Cross(line, ballPos)) / V2Cross(ballVelocity, line);
            //速度在反向锐角夹角之内，且此时已不可能撞到线段端点
            if (time < 0)
                return -1;

            //小球运动到碰撞点，速度沿法线方向的分量反向
            resPos = ballPos + ballVelocity * time;
            resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, lineNormal)) / lineNormal.sqrMagnitude * lineNormal;
            return time;
        }

        posToCS = lineStart - ballPos;
        float LCS = MAXMAP;
        if (Vector2.Dot(posToCS, ballVelocity) > 0)
        {
            Vector2 pathCS = Vector2.Dot(posToCS, ballVelocity) / ballVelocity.sqrMagnitude * ballVelocity;
            Vector2 near = posToCS - pathCS;
            float difference = Mathf.Sqrt(ballRadius * ballRadius - near.sqrMagnitude);
            //如果采用这种判断，则非常边缘的擦边球会认为没有撞到砖块
            //if (difference > Vector2.kEpsilon)
            if (difference > 0)
            {
                LCS = pathCS.magnitude - difference;
            }
        }
        posToCE = lineEnd - ballPos;
        float LCE = MAXMAP;
        if (Vector2.Dot(posToCE, ballVelocity) > 0)
        {
            Vector2 pathCE = Vector2.Dot(posToCE, ballVelocity) / ballVelocity.sqrMagnitude * ballVelocity;
            Vector2 near = posToCE - pathCE;
            float difference = Mathf.Sqrt(ballRadius * ballRadius - near.sqrMagnitude);
            //如果采用这种判断，则非常边缘的擦边球会认为没有撞到砖块
            //if (difference > Vector2.kEpsilon)
            if (difference > 0)
            {
                LCE = pathCE.magnitude - difference;
            }
        }

        //小球未撞到线段端点
        if (LCS >= MAXMAP && LCE >= MAXMAP)
            return -1;

        float minLength = Mathf.Min(LCS, LCE);
        time = minLength / ballVelocity.magnitude;
        resPos = ballPos + ballVelocity * time;
        var newNormal = LCS < LCE ? (resPos - lineStart) : (resPos - lineEnd);
        resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, newNormal)) / Vector2.Dot(newNormal, newNormal) * newNormal;
        return time;
    }

    /// <summary>
    /// lineStart != lineEnd
    /// 多边形碰撞
    /// 参数方程
    /// </summary>
    public static float CalculateCollision2(Vector2 ballPos, Vector2 ballVelocity, float ballRadius, Vector2 lineStart, Vector2 lineEnd, Vector2 lineNormal, out Vector2 resPos, out Vector2 resVel)
    {
        resPos = ballPos;
        resVel = ballVelocity;
        //速度和法线同向或垂直
        if (Vector2.Dot(ballVelocity, lineNormal) >= -Vector2.kEpsilon)
            return -1;

        float time = -1;
        Vector2 line = lineEnd - lineStart;
        Vector2 collisionStart = lineStart + ballRadius * lineNormal;
        Vector2 collisionEnd = lineEnd + ballRadius * lineNormal;
        Vector2 posToCS = collisionStart - ballPos;
        Vector2 posToCE = collisionEnd - ballPos;
        //速度方向在锐角夹角之内，其会撞到线段内
        if (V2Cross(ballVelocity, posToCS) * V2Cross(ballVelocity, posToCE) <= 0)
        {
            time = (V2Cross(collisionStart, line) + V2Cross(line, ballPos)) / V2Cross(ballVelocity, line);
            //速度在反向锐角夹角之内，且此时已不可能撞到线段端点
            if (time < 0)
                return -1;

            //小球运动到碰撞点，速度沿法线方向的分量反向
            resPos = ballPos + ballVelocity * time;
            resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, lineNormal)) / lineNormal.sqrMagnitude * lineNormal;
            return time;
        }

        float timeS = MAXMAP;
        float dx = ballPos.x - lineStart.x;
        float dy = ballPos.y - lineStart.y;
        float detla = (ballVelocity.x * dx + ballVelocity.y * dy) * (ballVelocity.x * dx + ballVelocity.y * dy) - (dx * dx + dy * dy - ballRadius * ballRadius) * ballVelocity.sqrMagnitude;
        if (detla > 0)
        {
            var mu = (ballVelocity.x * dx + ballVelocity.y * dy) / ballVelocity.sqrMagnitude;
            var t1 = Mathf.Sqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            var t2 = -Mathf.Sqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            if (t1 > 0 && t2 > 0)
                timeS = Mathf.Min(t1, t2);
            else if (t1 > 0)
                timeS = t1;
            else if (t2 > 0)
                timeS = t2;
        }

        float timeE = MAXMAP;
        dx = ballPos.x - lineEnd.x;
        dy = ballPos.y - lineEnd.y;
        detla = (ballVelocity.x * dx + ballVelocity.y * dy) * (ballVelocity.x * dx + ballVelocity.y * dy) - (dx * dx + dy * dy - ballRadius * ballRadius) * ballVelocity.sqrMagnitude;
        if (detla > 0)
        {
            var mu = (ballVelocity.x * dx + ballVelocity.y * dy) / ballVelocity.sqrMagnitude;
            var t1 = Mathf.Sqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            var t2 = -Mathf.Sqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            if (t1 > 0 && t2 > 0)
                timeE = Mathf.Min(t1, t2);
            else if (t1 > 0)
                timeE = t1;
            else if (t2 > 0)
                timeE = t2;
        }
        
        time = Mathf.Min(timeS, timeE);
        if (time >= MAXMAP)
            return -1;

        resPos = ballPos + ballVelocity * time;
        var newNormal = timeS < timeE ? (resPos - lineStart) : (resPos - lineEnd);
        resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, newNormal)) / Vector2.Dot(newNormal, newNormal) * newNormal;
        return time;
    }

    /// <summary>
    /// 圆形碰撞
    /// 参数方程
    /// isTrigger : 为真代表小球会穿过这个圆，速度不变。为假会完全弹性碰撞
    /// </summary>
    public static float CalculateCollisionCircle(Vector2 ballPos, Vector2 ballVelocity, float ballRadius, Vector2 cirPos, float cirRadius, bool isTrigger, out Vector2 resPos, out Vector2 resVel)
    {
        resPos = ballPos;
        resVel = ballVelocity;
        float time = MAXMAP;
        float dx = ballPos.x - cirPos.x;
        float dy = ballPos.y - cirPos.y;
        float sumRadius = ballRadius + cirRadius;
        float detla = (ballVelocity.x * dx + ballVelocity.y * dy) * (ballVelocity.x * dx + ballVelocity.y * dy) - (dx * dx + dy * dy - sumRadius * sumRadius) * ballVelocity.sqrMagnitude;
        if (detla > 0)
        {
            var mu = (ballVelocity.x * dx + ballVelocity.y * dy) / ballVelocity.sqrMagnitude;
            var t1 = Mathf.Sqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            var t2 = -Mathf.Sqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            if (t1 > 0 && t2 > 0)
                time = Mathf.Min(t1, t2);
            else if (t1 > 0)
                time = t1;
            else if (t2 > 0)
                time = t2;
        }

        if (time >= MAXMAP)
            return -1;
        resPos = ballPos + ballVelocity * time;
        if (!isTrigger)
        {
            var newNormal = resPos - cirPos;
            resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, newNormal)) / Vector2.Dot(newNormal, newNormal) * newNormal;
        }
        return time;
    }

    private static float V2Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.y - lhs.y * rhs.x;
    }
}
