using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPolygon
{
    private int vertexCount;
    private Vector2[] vertex;
    private Vector2[] line;
    private Vector2[] normal;

    private GameObject go;

    public BallPolygon(Vector2[] origin, GameObject go)
    {
        vertexCount = origin.Length;
        vertex = new Vector2[vertexCount];
        line = new Vector2[vertexCount];
        normal = new Vector2[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertex[i] = new Vector2(origin[i].x, origin[i].y);
            line[i] = new Vector2(origin[(i + 1) % vertexCount].x - origin[i].x, origin[(i + 1) % vertexCount].y - origin[i].y);
            normal[i] = new Vector2(-line[i].y, line[i].x).normalized;
        }

        this.go = go;
    }

    public void ShiftPosition(float move)
    {
        for (int i = 0;i < vertexCount; i++)
        {
            vertex[i].y -= move;
            line[i].y -= move;
        }
    }

    public float CheckCollision(Vector2 ballPosition, Vector2 ballVelocity, float ballRadius, out Vector2 newPosition, out Vector2 newVelocity)
    {
        float time = KineLib.MAXMAP;
        newPosition = Vector2.zero;
        newVelocity = Vector2.zero;
        for (int i = 0; i < vertexCount; i++)
        {
            var vs = vertex[i];
            var ve = vertex[(i + 1) % vertexCount];
            var n = normal[i];
            var resPos = Vector2.zero;
            var resVel = Vector2.zero;
            var t = KineLib.CalculateCollision2(ballPosition, ballVelocity, ballRadius, vs, ve, n, out resPos, out resVel);
            if (t > 0 && t < time)
            {
                time = t;
                newPosition = resPos;
                newVelocity = resVel;
            }
        }
        return time;
    }
}

public class Ball
{
    public readonly float Radius;
    public Vector2 Position;
    public Vector2 Velocity;

    private Transform trs;

    private int lastCollisionIdx = -1;

    public Ball(float r, Transform trs)
    {
        Radius = r;
        this.trs = trs;
    }

    public void MoveUpdate()
    {
        trs.position = Position;
    }

    public float PredictCollision(List<BallPolygon> blockList, float boundary, out Vector2 newPosition, out Vector2 newVelocity, out bool over)
    {
        float time = KineLib.MAXMAP;
        newPosition = Vector2.zero;
        newVelocity = Vector2.zero;
        over = false;

        var lastID = -1;
        for (int i = 0; i < blockList.Count; i++)
        {
            if (lastCollisionIdx == i) continue;
            var resPos = Vector2.zero;
            var resVel = Vector2.zero;
            var t = blockList[i].CheckCollision(Position, Velocity, Radius, out resPos, out resVel);
            if (t < time)
            {
                time = t;
                newPosition = resPos;
                newVelocity = resVel;
                lastID = i;
            }
        }
        lastCollisionIdx = lastID;

        var lTime = (Velocity.x < -Vector2.kEpsilon) ? (Radius - Position.x) / Velocity.x : KineLib.MAXMAP;
        if (lTime < time)
        {
            time = lTime;
            newPosition = Position + time * Velocity;
            newVelocity = new Vector2(-Velocity.x, Velocity.y);
        }
        var uTime = (Velocity.y > Vector2.kEpsilon) ? (boundary - Radius - Position.y) / Velocity.y : KineLib.MAXMAP;
        if (uTime < time)
        {
            time = uTime;
            newPosition = Position + time * Velocity;
            newVelocity = new Vector2(Velocity.x, -Velocity.y);
        }
        var rTime = (Velocity.x > Vector2.kEpsilon) ? (boundary - Radius - Position.x) / Velocity.x : KineLib.MAXMAP;
        if (rTime < time)
        {
            time = rTime;
            newPosition = Position + time * Velocity;
            newVelocity = new Vector2(-Velocity.x, Velocity.y);
        }
        var bTime = (Velocity.y < -Vector2.kEpsilon) ? (Radius - Position.y) / Velocity.y : KineLib.MAXMAP;
        if (bTime < time)
        {
            time = bTime;
            newPosition = Position + time * Velocity;
            newVelocity = Vector2.zero;
            over = true;
        }

        return time;
    }
}
