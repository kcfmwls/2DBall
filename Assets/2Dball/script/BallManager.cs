using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

    [SerializeField]
    private Transform ballVelocityTrs;
    [SerializeField]
    private GameObject ballTemplate;
    [SerializeField]
    private GameObject buffTemplate;
    [SerializeField]
    private List<GameObject> blockTemplate;

    //每个格子尝试生成砖块概率
    private int probility = 25;
    private int buffProbility = 10;

    private float shootTime = 0;
    private int shootIdx = 0;
    private float intervalTime = 0.25f;
    private Vector2 shootVel = Vector2.zero;

    private enum GameState
    {
        Motion,
        Shoot,
        Static,
    }
    private GameState state = GameState.Static;

    //地图边长
    public int LofSide = 14;

    //本次循环内发射的小球数量，总是发射列表的前n个小球
    private int shootBallCount = 0;
    //本次循环内已经停止运动的小球数量
    private int stopBallCount = 0;
    public List<Ball> BallList = new List<Ball>();

    //砖块唯一标识ID，每生成一个砖块递增，不重复
    private int uniqueBlockID = 0;
    public Dictionary<int, BallPolygon> PolygonMap = new Dictionary<int, BallPolygon>();

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
        AddNewBall();
        AddNewBall();
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
        if (state == GameState.Static)
        {
            if (Input.GetMouseButtonDown(0))
            {
                shootBallCount = BallList.Count;
                shootIdx = 0;
                shootTime = intervalTime;
                stopBallCount = 0;
                shootVel = new Vector2(ballVelocityTrs.position.x - 7, ballVelocityTrs.position.y - 0.5f);
                state = GameState.Shoot;
                Debug.Log("发射" + BallList.Count + "个球");
            }
        }
        else if (state == GameState.Shoot)
        {
            if (shootTime < intervalTime)
            {
                shootTime += Time.deltaTime;
            }
            else
            {
                BallList[shootIdx].ActiveBall(shootVel, null);
                shootIdx++;
                shootTime = 0;
            }

            for (int i = 0; i < shootIdx; i++)
            {
                BallList[i].KinematicsUpdate();
            }

            if (shootIdx == shootBallCount)
                state = GameState.Motion;
        }
        else if (state == GameState.Motion)
        {
            for (int i = 0; i < shootBallCount; i++)
            {
                BallList[i].KinematicsUpdate();
            }
        }
    }

    //由小球撞到道具时触发。增加一个静止小球到起点
    public void AddNewBall()
    {
        GameObject go = GameObject.Instantiate(ballTemplate);
        go.transform.position = new Vector3(7, 0.5f, 4.3f);
        BallList.Add(new Ball(go.transform.localScale.x / 2, go.transform, BallType.normalBall, 1));
        Debug.Log("共有" + BallList.Count + "个球");
    }

    //由小球停止后触发
    public void StopABall()
    {
        stopBallCount++;
        if (stopBallCount == shootBallCount)
        {
            foreach (var id in PolygonMap.Keys)
            {
                PolygonMap[id].ShiftPosition(1);
            }
            proceduralGeneration(14);
            state = GameState.Static;
        }
    }

    //high上边界
    private void proceduralGeneration(float high)
    {
        for(int i = 0; i < LofSide; i++)
        {
            int p = Random.Range(0, 100);
            if (p >= probility && p < probility + buffProbility)
            {
                GameObject go = GameObject.Instantiate(buffTemplate);
                go.transform.position = new Vector3(i + 0.5f, high - 0.5f, 4);
                go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                uniqueBlockID++;
                PolygonMap.Add(uniqueBlockID, new GainBallItem(uniqueBlockID, go));
            }
            else if (p < probility)
            {
                int hp = p;
                p = Random.Range(0, (i == LofSide - 1 ? 5 : 7));
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
                uniqueBlockID++;
                PolygonMap.Add(uniqueBlockID, new NormalBlock(uniqueBlockID, go, ori, hp));

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
        uniqueBlockID++;
        PolygonMap.Add(uniqueBlockID, new NormalBlock(uniqueBlockID, go, ori, 15));
    }
}
