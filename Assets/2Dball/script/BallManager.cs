using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

    [SerializeField]
    private Transform ballTrs;
    [SerializeField]
    private Transform ballVelocityTrs;
    [SerializeField]
    private List<GameObject> blockTemplate;

    //地图边长
    private int lofSide = 14;
    //每个格子尝试生成砖块概率
    private int probility = 25;

    private Ball mainBall;
    private List<BallPolygon> blockList = new List<BallPolygon>();

    private float moveTime;
    private float predictTime;
    private Vector2 predictPos;
    private Vector2 predictVel;
    private bool over;
    private int collCount;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector2 ballPos = new Vector2(ballTrs.position.x, ballTrs.position.y);
        Vector2 ballVel = new Vector2(ballVelocityTrs.position.x - ballTrs.position.x, ballVelocityTrs.position.y - ballTrs.position.y);

        mainBall = new Ball(ballTrs.localScale.x / 2, ballTrs);
        mainBall.Position = ballPos;
        mainBall.Velocity = ballVel;
        moveTime = 0;
        predictTime = -1;
        over = false;
        predictPos = ballPos;
        predictVel = ballVel * 5;
        collCount = 0;

        //debugMap();
        proceduralGeneration(14);
        proceduralGeneration(13);
        proceduralGeneration(12);
        proceduralGeneration(11);
        proceduralGeneration(10);
    }

    // Update is called once per frame
    void Update()
    {
        if (moveTime < predictTime)
        {
            moveTime += Time.deltaTime;
            mainBall.Position += mainBall.Velocity * Time.deltaTime;
            mainBall.MoveUpdate();
        }
        else if (over)
        {
            if (Input.GetMouseButtonDown(0))
            {
                predictVel = new Vector2(ballVelocityTrs.position.x - ballTrs.position.x, ballVelocityTrs.position.y - ballTrs.position.y);
                over = false;
            }
        }
        else
        {
            mainBall.Position = predictPos;
            mainBall.Velocity = predictVel;
            mainBall.MoveUpdate();
            moveTime = 0;
            predictTime = mainBall.PredictCollision(blockList, lofSide, out predictPos, out predictVel, out over);
            collCount++;
            UnityEngine.Debug.Log(predictTime + " // " + collCount);
        }
    }

    //high上边界
    private void proceduralGeneration(float high)
    {
        for(int i = 0; i < lofSide; i++)
        {
            int p = Random.Range(0, 100);
            if (p < probility)
            {
                p = Random.Range(0, (i == lofSide - 1 ? 5 : 7));
                Vector2[] ori = {};
                switch (p)
                {
                    case 0:
                        ori = new Vector2[4];
                        ori[0] = new Vector2(i, high);
                        ori[1] = new Vector2(i + 1, high);
                        ori[2] = new Vector2(i + 1, high - 1);
                        ori[3] = new Vector2(i, high - 1);
                        break;
                    case 1:
                        ori = new Vector2[3];
                        ori[0] = new Vector2(i, high);
                        ori[1] = new Vector2(i + 1, high - 1);
                        ori[2] = new Vector2(i, high - 1);
                        break;
                    case 2:
                        ori = new Vector2[3];
                        ori[0] = new Vector2(i + 1, high);
                        ori[1] = new Vector2(i + 1, high - 1);
                        ori[2] = new Vector2(i, high - 1);
                        break;
                    case 3:
                        ori = new Vector2[3];
                        ori[0] = new Vector2(i, high);
                        ori[1] = new Vector2(i + 1, high);
                        ori[2] = new Vector2(i, high - 1);
                        break;
                    case 4:
                        ori = new Vector2[3];
                        ori[0] = new Vector2(i, high);
                        ori[1] = new Vector2(i + 1, high);
                        ori[2] = new Vector2(i + 1, high - 1);
                        break;
                    case 5:
                        ori = new Vector2[4];
                        ori[0] = new Vector2(i, high);
                        ori[1] = new Vector2(i + 2, high);
                        ori[2] = new Vector2(i + 2, high - 1);
                        ori[3] = new Vector2(i, high - 1);
                        break;
                    case 6:
                        ori = new Vector2[4];
                        ori[0] = new Vector2(i, high - 0.5f);
                        ori[1] = new Vector2(i + 1, high);
                        ori[2] = new Vector2(i + 2, high - 0.5f);
                        ori[3] = new Vector2(i + 1, high - 1);
                        break;
                    default:
                        break;
                }
                GameObject go = GameObject.Instantiate(blockTemplate[p]);
                go.transform.position = new Vector3(i, high, 4);
                blockList.Add(new BallPolygon(ori, go));

                if (p == 5 || p == 6) i++;
            }
        }
    }

    private void debugMap()
    {
        debugBlock(14, 9, 4);
        debugBlock(13, 8, 5);
        debugBlock(12, 9, 0);
        debugBlock(11, 8, 5);
        debugBlock(10, 9, 0);
        debugBlock(9, 9, 3);

        debugBlock(9, 8, 0);
        debugBlock(9, 7, 0);
        debugBlock(9, 6, 0);
        debugBlock(9, 5, 0);
        debugBlock(9, 4, 1);

        debugBlock(10, 4, 0);
        debugBlock(11, 4, 5);
        debugBlock(12, 4, 0);
        debugBlock(13, 4, 3);

        debugBlock(12, 6, 6);
    }

    private void debugBlock(int high, int i, int p)
    {
        Vector2[] ori = { };
        switch (p)
        {
            case 0:
                ori = new Vector2[4];
                ori[0] = new Vector2(i, high);
                ori[1] = new Vector2(i + 1, high);
                ori[2] = new Vector2(i + 1, high - 1);
                ori[3] = new Vector2(i, high - 1);
                break;
            case 1:
                ori = new Vector2[3];
                ori[0] = new Vector2(i, high);
                ori[1] = new Vector2(i + 1, high - 1);
                ori[2] = new Vector2(i, high - 1);
                break;
            case 2:
                ori = new Vector2[3];
                ori[0] = new Vector2(i + 1, high);
                ori[1] = new Vector2(i + 1, high - 1);
                ori[2] = new Vector2(i, high - 1);
                break;
            case 3:
                ori = new Vector2[3];
                ori[0] = new Vector2(i, high);
                ori[1] = new Vector2(i + 1, high);
                ori[2] = new Vector2(i, high - 1);
                break;
            case 4:
                ori = new Vector2[3];
                ori[0] = new Vector2(i, high);
                ori[1] = new Vector2(i + 1, high);
                ori[2] = new Vector2(i + 1, high - 1);
                break;
            case 5:
                ori = new Vector2[4];
                ori[0] = new Vector2(i, high);
                ori[1] = new Vector2(i + 2, high);
                ori[2] = new Vector2(i + 2, high - 1);
                ori[3] = new Vector2(i, high - 1);
                break;
            case 6:
                ori = new Vector2[4];
                ori[0] = new Vector2(i, high - 0.5f);
                ori[1] = new Vector2(i + 1, high);
                ori[2] = new Vector2(i + 2, high - 0.5f);
                ori[3] = new Vector2(i + 1, high - 1);
                break;
            default:
                break;
        }
        GameObject go = GameObject.Instantiate(blockTemplate[p]);
        go.transform.position = new Vector3(i, high, 4);
        blockList.Add(new BallPolygon(ori, go));
    }
}
