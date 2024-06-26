using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallUIView : MonoBehaviour
{
    /*-------------Binding UI-------------*/
    [SerializeField]
    private Button doubleSpeedBtn;

    [SerializeField]
    private GameObject doubleSpeedTagGo;

    [SerializeField]
    private Button pauseBtn;

    [SerializeField]
    private GameObject pauseTagGo;

    [SerializeField]
    private Text ballCountText;

    [SerializeField]
    private Text moveCountText;

    [SerializeField]
    private Button shootBtn;

    [SerializeField]
    private Button stopBtn;
    /*------------------------------------*/

    // Start is called before the first frame update
    void Start()
    {
        ballCountText.text = "1";
        moveCountText.text = "0";

        doubleSpeedBtn.onClick.AddListener(() =>
        {
            var tag = !BallManager.Instance.GMDoubleSpeed;
            BallManager.Instance.GMDoubleSpeed = tag;
            doubleSpeedTagGo.SetActive(tag);
        });
        pauseBtn.onClick.AddListener(() =>
        {
            var tag = !BallManager.Instance.GMPause;
            BallManager.Instance.GMPause = tag;
            pauseTagGo.SetActive(tag);
        });
        shootBtn.onClick.AddListener(() =>
        {
            BallManager.Instance.ShootBall();
        });
        stopBtn.onClick.AddListener(() =>
        {
            BallManager.Instance.BreakBall();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUIBallCountText(int num)
    {
        ballCountText.text = num.ToString();
    }

    public void SetUIMoveCountText(int num)
    {
        moveCountText.text = num.ToString();
    }
}
