using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PolygonType
{
    normalBlock,
    gainBallItem,
}

public enum BallType
{
    normalBall,
    explosiveBall,
}

public abstract class BallPolygon
{
    protected int uniqueId;
    protected GameObject go;
    protected PolygonType type;

    public BallPolygon(int uniqueId, GameObject go, PolygonType type)
    {
        this.uniqueId = uniqueId;
        this.go = go;
        this.type = type;
    }

    public abstract void ShiftPosition(float move);
    public abstract float CheckCollision(Vector2 ballPosition, Vector2 ballVelocity, float ballRadius, out Vector2 newPosition, out Vector2 newVelocity);
    public abstract void OnCollision(Ball ball);

    public PolygonType GetType() { return type; }
}

public class NormalBlock : BallPolygon
{
    private int vertexCount;
    private Vector2[] vertex;
    private Vector2[] line;
    private Vector2[] normal;

    private int remainHp;
    private TextMesh hpText;

    public NormalBlock(int uniqueId, GameObject go, Vector2[] origin, int remain) : base(uniqueId, go, PolygonType.normalBlock)
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

        remainHp = remain;
        hpText = go.transform.Find("hp").GetComponent<TextMesh>();
        hpText.text = remainHp.ToString();
    }

    public override void ShiftPosition(float move)
    {
        for (int i = 0; i < vertexCount; i++)
        {
            vertex[i].y -= move;
            line[i].y -= move;
        }
        var goPos = this.go.transform.position;
        goPos.y -= move;
        this.go.transform.position = goPos;
    }

    public override float CheckCollision(Vector2 ballPosition, Vector2 ballVelocity, float ballRadius, out Vector2 newPosition, out Vector2 newVelocity)
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

    public override void OnCollision(Ball ball)
    {
        remainHp -= ball.Damage;
        hpText.text = remainHp.ToString();
        if (remainHp <= 0)
        {
            this.go.SetActive(false);
            BallManager.Instance.PolygonMap.Remove(this.uniqueId);
        }
    }
}

public class GainBallItem : BallPolygon
{
    private Vector2 itemPostion;
    private float itemRadius;

    public GainBallItem(int uniqueId, GameObject go) : base(uniqueId, go, PolygonType.gainBallItem)
    {
        itemPostion = new Vector2(go.transform.position.x, go.transform.position.y);
        itemRadius = go.transform.localScale.x / 2;
    }

    public override void ShiftPosition(float move)
    {
        itemPostion.y -= move;
        var goPos = this.go.transform.position;
        goPos.y -= move;
        this.go.transform.position = goPos;
    }

    public override float CheckCollision(Vector2 ballPosition, Vector2 ballVelocity, float ballRadius, out Vector2 newPosition, out Vector2 newVelocity)
    {
        float time = KineLib.CalculateCollisionCircle(ballPosition, ballVelocity, ballRadius, itemPostion, itemRadius, true, out newPosition, out newVelocity);
        if (time < 0)
            time = KineLib.MAXMAP;
        return time;
    }

    public override void OnCollision(Ball ball)
    {
        this.go.SetActive(false);
        BallManager.Instance.PolygonMap.Remove(this.uniqueId);
        BallManager.Instance.AddNewBall();
    }
}

public class Ball
{
    private readonly float mRadius;
    private Vector2 mPosition;
    private Vector2 mVelocity;

    private Transform mTransform;

    private float moveTime;
    private float predictTime;
    private Vector2 predictPos;
    private Vector2 predictVel;
    private bool over;
    private int lastCollisionId;

    public BallType Type;
    public int Damage;
    public bool Static;

    public Ball(float r, Transform trs, BallType type, int dmg)
    {
        mRadius = r;
        mTransform = trs;
        Type = type;
        Damage = dmg;
        Static = true;

        mPosition = new Vector2(trs.position.x, trs.position.y);
        mVelocity = Vector2.zero;
        moveTime = 0;
        predictTime = 0;
        predictPos = Vector2.zero;
        predictVel = Vector2.zero;
        over = true;
        lastCollisionId = -1;
    }

    public void ActiveBall(Vector2 velocity, Vector2? position)
    {
        mVelocity = velocity;
        mPosition = position.HasValue ? position.Value : mPosition;
        MoveUpdate();
        moveTime = 0;
        PredictCollision(BallManager.Instance.PolygonMap, BallManager.Instance.LofSide);

        Static = false;
    }

    public void ResetBall()
    {
        mPosition = BallManager.Instance.ResetPos;
        mVelocity = Vector2.zero;
        moveTime = 0;
        predictTime = 0;
        predictPos = Vector2.zero;
        predictVel = Vector2.zero;
        over = true;
        lastCollisionId = -1;
        MoveUpdate();

        Static = true;
    }

    public void KinematicsUpdate(float dt)
    {
        if (Static) return;
        if (moveTime < predictTime)
        {
            moveTime += dt;
            mPosition += mVelocity * dt;
            MoveUpdate();
        }
        else if (over)
        {
            BallManager.Instance.StopABall(predictPos);
            ResetBall();
        }
        else
        {
            mPosition = predictPos;
            MoveUpdate();

            var map = BallManager.Instance.PolygonMap;
            if (map.ContainsKey(lastCollisionId))
            {
                map[lastCollisionId].OnCollision(this);
                mVelocity = predictVel;
            }
            if (lastCollisionId == -1)
                mVelocity = predictVel;

            moveTime = 0;
            PredictCollision(map, BallManager.Instance.LofSide);
        }
    }

    private void MoveUpdate()
    {
        mTransform.position = mPosition;
    }

    private void PredictCollision(Dictionary<int, BallPolygon> blockMap, float boundary)
    {
        predictTime = KineLib.MAXMAP;
        predictPos = Vector2.zero;
        predictVel = Vector2.zero;
        over = false;

        if (mVelocity.magnitude < Vector2.kEpsilon)
        {
            predictTime = 0;
            over = true;
            return;
        }

        var lastID = -1;
        foreach (var id in blockMap.Keys)
        {
            //if (lastCollisionId == id) continue;
            var resPos = Vector2.zero;
            var resVel = Vector2.zero;
            var t = blockMap[id].CheckCollision(mPosition, mVelocity, mRadius, out resPos, out resVel);
            if (t < predictTime)
            {
                predictTime = t;
                predictPos = resPos;
                predictVel = resVel;
                lastID = id;
            }
        }
        lastCollisionId = lastID;

        var lTime = (mVelocity.x < -Vector2.kEpsilon) ? (mRadius - mPosition.x) / mVelocity.x : KineLib.MAXMAP;
        if (lTime < predictTime)
        {
            predictTime = lTime;
            predictPos = mPosition + predictTime * mVelocity;
            predictVel = new Vector2(-mVelocity.x, mVelocity.y);
        }
        var uTime = (mVelocity.y > Vector2.kEpsilon) ? (boundary - mRadius - mPosition.y) / mVelocity.y : KineLib.MAXMAP;
        if (uTime < predictTime)
        {
            predictTime = uTime;
            predictPos = mPosition + predictTime * mVelocity;
            predictVel = new Vector2(mVelocity.x, -mVelocity.y);
        }
        var rTime = (mVelocity.x > Vector2.kEpsilon) ? (boundary - mRadius - mPosition.x) / mVelocity.x : KineLib.MAXMAP;
        if (rTime < predictTime)
        {
            predictTime = rTime;
            predictPos = mPosition + predictTime * mVelocity;
            predictVel = new Vector2(-mVelocity.x, mVelocity.y);
        }
        var bTime = (mVelocity.y < -Vector2.kEpsilon) ? (mRadius - mPosition.y) / mVelocity.y : KineLib.MAXMAP;
        if (bTime < predictTime)
        {
            predictTime = bTime;
            predictPos = mPosition + predictTime * mVelocity;
            predictVel = Vector2.zero;
            over = true;
        }
    }
}
