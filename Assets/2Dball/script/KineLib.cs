using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class KineLib
{
    public const float MAXMAP = 196000;

    /// <summary>
    /// lineStart != lineEnd
    /// ����
    /// </summary>
    public static float CalculateCollision(Vector2 ballPos, Vector2 ballVelocity, float ballRadius, Vector2 lineStart, Vector2 lineEnd, Vector2 lineNormal, out Vector2 resPos, out Vector2 resVel)
    {
        resPos = ballPos;
        resVel = ballVelocity;
        //�ٶȺͷ���ͬ���ֱ
        if (Vector2.Dot(ballVelocity, lineNormal) >= -Vector2.kEpsilon)
            return -1;

        float time;
        Vector2 line = lineEnd - lineStart;
        Vector2 collisionStart = lineStart + ballRadius * lineNormal;
        Vector2 collisionEnd = lineEnd + ballRadius * lineNormal;
        Vector2 posToCS = collisionStart - ballPos;
        Vector2 posToCE = collisionEnd - ballPos;
        //�ٶȷ�������Ǽн�֮�ڣ����ײ���߶���
        if (v2Cross(ballVelocity, posToCS) * v2Cross(ballVelocity, posToCE) <= 0)
        {
            time = (v2Cross(collisionStart, line) + v2Cross(line, ballPos)) / v2Cross(ballVelocity, line);
            //�ٶ��ڷ�����Ǽн�֮�ڣ��Ҵ�ʱ�Ѳ�����ײ���߶ζ˵�
            if (time < 0)
                return -1;

            //С���˶�����ײ�㣬�ٶ��ط��߷���ķ�������
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
            float difference = mSqrt(ballRadius * ballRadius - near.sqrMagnitude);
            //������������жϣ���ǳ���Ե�Ĳ��������Ϊû��ײ��ש��
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
            float difference = mSqrt(ballRadius * ballRadius - near.sqrMagnitude);
            //������������жϣ���ǳ���Ե�Ĳ��������Ϊû��ײ��ש��
            //if (difference > Vector2.kEpsilon)
            if (difference > 0)
            {
                LCE = pathCE.magnitude - difference;
            }
        }

        //С��δײ���߶ζ˵�
        if (LCS >= MAXMAP && LCE >= MAXMAP)
            return -1;

        float minLength = Mathf.Min(LCS, LCE);
        time = minLength / ballVelocity.magnitude;
        //--------------------------------�·���������ײ����ֹ�򸡵����������⵼�µ��˶�Խ�紩ģ
        resPos = ballPos + ballVelocity * Mathf.Max(time - 0.01f, 0);
        var newNormal = LCS < LCE ? (resPos - lineStart) : (resPos - lineEnd);
        resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, newNormal)) / Vector2.Dot(newNormal, newNormal) * newNormal;
        return time;
    }

    /// <summary>
    /// lineStart != lineEnd
    /// ��������
    /// </summary>
    public static float CalculateCollision2(Vector2 ballPos, Vector2 ballVelocity, float ballRadius, Vector2 lineStart, Vector2 lineEnd, Vector2 lineNormal, out Vector2 resPos, out Vector2 resVel)
    {
        resPos = ballPos;
        resVel = ballVelocity;
        //�ٶȺͷ���ͬ���ֱ
        if (Vector2.Dot(ballVelocity, lineNormal) >= -Vector2.kEpsilon)
            return -1;

        float time = -1;
        Vector2 line = lineEnd - lineStart;
        Vector2 collisionStart = lineStart + ballRadius * lineNormal;
        Vector2 collisionEnd = lineEnd + ballRadius * lineNormal;
        Vector2 posToCS = collisionStart - ballPos;
        Vector2 posToCE = collisionEnd - ballPos;
        //�ٶȷ�������Ǽн�֮�ڣ����ײ���߶���
        if (v2Cross(ballVelocity, posToCS) * v2Cross(ballVelocity, posToCE) <= 0)
        {
            time = (v2Cross(collisionStart, line) + v2Cross(line, ballPos)) / v2Cross(ballVelocity, line);
            //�ٶ��ڷ�����Ǽн�֮�ڣ��Ҵ�ʱ�Ѳ�����ײ���߶ζ˵�
            if (time < 0)
                return -1;

            //С���˶�����ײ�㣬�ٶ��ط��߷���ķ�������
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
            var t1 = mSqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            var t2 = -mSqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
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
            var t1 = mSqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
            var t2 = -mSqrt(detla / ballVelocity.sqrMagnitude / ballVelocity.sqrMagnitude) - mu;
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

        //--------------------------------�·���������ײ����ֹ�򸡵����������⵼�µ��˶�Խ�紩ģ
        resPos = ballPos + ballVelocity * Mathf.Max(time - 0.01f, 0);
        var newNormal = timeS < timeE ? (resPos - lineStart) : (resPos - lineEnd);
        resVel = ballVelocity - 2 * (Vector2.Dot(ballVelocity, newNormal)) / Vector2.Dot(newNormal, newNormal) * newNormal;
        return time;
    }

    private static float v2Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.y - lhs.y * rhs.x;
    }

    private static float mSqrt(float x)
    {
        float xf = 0.5f * x;
        int i;
        unsafe
        {
            i = *(int*)&x;
            i = 0x5f375a86 - (i >> 1);
            x = *(float*)&i;
        }
        x = x * (1.5f - xf * x * x);
        return 1 / x;
    }
}
