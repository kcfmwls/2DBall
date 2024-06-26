using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    #region ����
    public static BallManager Instance;
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
    #endregion

    #region ģ���
    [SerializeField]
    private Transform ballVelocityTrs;
    [SerializeField]
    private GameObject ballTemplate;
    [SerializeField]
    private GameObject buffTemplate;
    [SerializeField]
    private List<GameObject> blockTemplate;
    #endregion

    #region ��ͼ����
    //ÿ�����ӳ�������ש�����
    private int probility = 25;
    private int buffProbility = 10;

    //��ͼ�߳�
    public int LofSide = 14;
    #endregion

    #region �����߼�ִ��
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

    //����ѭ���ڵ�һ�����ص����С��λ�ã�������һ����ʼλ��
    public Vector2 ResetPos = new Vector2(7, 0.5f);
    //����ѭ���ڷ����С�����������Ƿ����б��ǰn��С��
    private int shootBallCount = 0;
    //����ѭ�����Ѿ�ֹͣ�˶���С������
    private int stopBallCount = 0;
    public List<Ball> BallList = new List<Ball>();

    //ש��Ψһ��ʶID��ÿ����һ��ש����������ظ�
    private int uniqueBlockID = 0;
    public Dictionary<int, BallPolygon> PolygonMap = new Dictionary<int, BallPolygon>();
    #endregion

    #region ��Ϸ��UI
    [SerializeField]
    private BallUIView uiView;

    public bool GMDoubleSpeed = false;
    public bool GMPause = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = GameObject.Instantiate(ballTemplate);
        go.transform.position = new Vector3(7, 0.5f, 4.3f);
        BallList.Add(new Ball(go.transform.localScale.x / 2, go.transform, BallType.normalBall, 1));
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
        if (GMPause)
            return;

        float dt = GMDoubleSpeed ? Time.deltaTime * 2 : Time.deltaTime;
        if (state == GameState.Static)
        {
            
        }
        else if (state == GameState.Shoot)
        {
            if (shootTime < intervalTime)
            {
                shootTime += dt;
            }
            else
            {
                BallList[shootIdx].ActiveBall(shootVel, null);
                shootIdx++;
                shootTime = 0;

                uiView.SetUIMoveCountText(shootIdx - stopBallCount);
            }

            for (int i = 0; i < shootIdx; i++)
            {
                BallList[i].KinematicsUpdate(dt);
            }

            if (shootIdx == shootBallCount)
                state = GameState.Motion;
        }
        else if (state == GameState.Motion)
        {
            for (int i = 0; i < shootBallCount; i++)
            {
                BallList[i].KinematicsUpdate(dt);
            }
        }
    }

    //��ҿ��ƣ�����С��
    public void ShootBall()
    {
        if (state == GameState.Static)
        {
            shootBallCount = BallList.Count;
            shootIdx = 0;
            shootTime = intervalTime;
            stopBallCount = 0;
            shootVel = new Vector2(ballVelocityTrs.position.x - ResetPos.x, ballVelocityTrs.position.y - ResetPos.y);
            state = GameState.Shoot;
        }
    }

    //��ҿ��ƣ��ж�����С��
    public void BreakBall()
    {
        if (state != GameState.Static)
        {
            state = GameState.Static;
            for (int i = 0; i < BallList.Count; i++)
            {
                BallList[i].ResetBall();
            }
            foreach (var id in PolygonMap.Keys)
            {
                PolygonMap[id].ShiftPosition(1);
            }
            proceduralGeneration(14);

            uiView.SetUIMoveCountText(0);
        }
    }

    //��С��ײ������ʱ����������һ����ֹС�����
    public void AddNewBall()
    {
        var newIdx = BallList.Count - shootBallCount;
        GameObject go = GameObject.Instantiate(ballTemplate);
        go.transform.position = new Vector3(newIdx - 0.5f, -0.5f, 4.3f);
        BallList.Add(new Ball(go.transform.localScale.x / 2, go.transform, BallType.normalBall, 1));
        Debug.Log("����" + BallList.Count + "����");

        uiView.SetUIBallCountText(BallList.Count);
    }

    //��С��ֹͣ�󴥷�
    public void StopABall(Vector2 ballPos)
    {
        stopBallCount++;
        if (stopBallCount == 1)
            ResetPos = ballPos;
        if (stopBallCount == shootBallCount)
        {
            state = GameState.Static;
            for (int i = shootBallCount; i < BallList.Count; i++)
            {
                BallList[i].ResetBall();
            }
            foreach (var id in PolygonMap.Keys)
            {
                PolygonMap[id].ShiftPosition(1);
            }
            proceduralGeneration(14);
        }

        uiView.SetUIMoveCountText(shootIdx - stopBallCount);
    }

    //high�ϱ߽�
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
        debugBlock(8, 9, 6);
        debugBlock(7, 9, 4);
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
