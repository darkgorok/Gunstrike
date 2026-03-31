using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupEnemySystem : MonoBehaviour {
    
    public enum SHOWENEMYTYPE { ShowAll, OneByOne}

    public SHOWENEMYTYPE showEnemyType;

    bool isWorked = false;
    [Header("NOTE: IF PLACE BOSS INTO THIS, NEED DISABLE 'DETECTPLAYER' object in BOSS")]
    public GroupMiniEnemy[] EnemyGroup;
    public int[] showOrderGroup;
    [Tooltip("Just in case this is boss and need avtive it")]
    public bool sendMessage = false;
    public string trySendAMessage = "Play";
    int currentGroup = 0;

    public AudioClip soundShowUp;
    public AudioClip soundWarning;
    public AudioClip soundClean;
    public AudioClip soundGetKey;
    AudioSource soundWarningScr;
    public Transform targetCameraLook;
    bool isCameraLooking = false;
    CameraFollow mainCamera;
    Vector3 mainCameraStartPoint;
    float moveCameraPercent = 0;
    bool moveCameraToTarget = true;
    public float moveCameraSpeed = 5;
    //public GameObject keyForLastEnemey;
    public enum GETKEYTYPE { KEY, NOKEY}
    public GETKEYTYPE getKeyType;

    List<MonoBehaviour> listMono;
    List<GameObject> listEnemyNeedKill;

    bool isFinishUse = false;

    GameObject lastEnemyPosition;

    // Use this for initialization
    void Start () {
        mainCamera = FindObjectOfType<CameraFollow>();
        //Invoke("Init", 1f);
        StartCoroutine(InitCo());

        soundWarningScr = gameObject.AddComponent<AudioSource>();
        soundWarningScr.clip = soundWarning;
        soundWarningScr.loop = true;
        soundWarningScr.volume = 1;
    }

    IEnumerator InitCo()
    {
        yield return null;
        //listEnemyNeedKill = new List<GameObject>(Monsters);
        listEnemyNeedKill = new List<GameObject>();
        listMono = new List<MonoBehaviour>();
        foreach (var target in EnemyGroup)
        {
            if (target != null)
            {
                foreach (var _miniE in target.miniGroup)
                {
                    if (_miniE != null)
                    {
                        MonoBehaviour[] monos = _miniE.GetComponents<MonoBehaviour>();
                        foreach (var mono in monos)
                        {
                            listMono.Add(mono);
                            mono.enabled = false;
                        }

                        if (showEnemyType == SHOWENEMYTYPE.OneByOne)
                        {
                            _miniE.SetActive(false);

                        }
                        else
                        {
                            _miniE.SetActive(true);
                            listEnemyNeedKill.Add(_miniE);
                        }
                    }
                }
            }
        }

        lastEnemyPosition = new GameObject();
        lastEnemyPosition.name = "FollowLastEnemy-GroupEnemySystem.cs";

        //ShowNextGroup();

        //show first group
        GameObject[] pickGroup = EnemyGroup[showOrderGroup[0] - 1].miniGroup;
        foreach (var enemy in pickGroup)
        {
            if (enemy != null)
                enemy.SetActive(true);
        }
    }

    //void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 10, 200, 20), "isCameraLooking: " + isCameraLooking);
    //}

    // Update is called once per frame
    void Update () {
        if (isFinishUse)
            return;


        if (isCameraLooking)
        {
            
            if (moveCameraToTarget) {
                moveCameraPercent += moveCameraSpeed * Time.deltaTime;
                mainCamera.transform.position = Vector3.Lerp(mainCameraStartPoint, targetCameraLook.position, moveCameraPercent);
                if(Vector2.Distance(mainCamera.transform.position, targetCameraLook.position) < 0.1f)
                {
                    moveCameraToTarget = false;
                    moveCameraPercent = 0;
                }
            }
            else
            {
                moveCameraPercent += moveCameraSpeed * Time.deltaTime;
                mainCamera.transform.position = Vector3.Lerp(targetCameraLook.position, mainCameraStartPoint, moveCameraPercent);
                if (Vector2.Distance(mainCameraStartPoint, mainCamera.transform.position) < 0.1f)
                {
                    isCameraLooking = false;

                }
            }
        }

        if (isWorked)
        {
            int alive = 0;
            foreach(var _target in listEnemyNeedKill)
            {
                if (_target != null && _target.activeInHierarchy)
                {
                    alive++;
                    lastEnemyPosition.transform.position = _target.transform.position;
                }
            }

            if (alive == 0)
            {
                if (showEnemyType == SHOWENEMYTYPE.ShowAll)
                    KillTheLastEnemyEvent();
                else
                    StartCoroutine(ShowNextGroup());
            }
        }
	}

    void KillTheLastEnemyEvent()
    {
        SoundManager.PlaySfx(soundClean);
        //Instantiate(keyForLastEnemey, lastEnemyPosition.transform.position + Vector3.up * 1.5f, Quaternion.identity);
        if(getKeyType == GETKEYTYPE.KEY)
        {
            GameManager.Instance.isHasKey = true;
            SoundManager.PlaySfx(soundGetKey);
        }
        GroupEnemySystemUI.Instance.ShowClean();
        isFinishUse = true;
    }

    IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (isWorked)
            yield break;

        if (other.gameObject != GameManager.Instance.Player.gameObject)
            yield break;
        
        GameManager.Instance.Player.PausePlayer(true);
        SoundManager.PlaySfx(soundShowUp);

        mainCamera.enabled = false;
        mainCameraStartPoint = mainCamera.transform.position;
        isCameraLooking = true;
        GroupEnemySystemUI.Instance.ShowWarning(true);
        SoundManager.Instance.PauseMusic(true);
        soundWarningScr.Play();

        while (isCameraLooking) { yield return null; }

        mainCamera.enabled = true;
        GroupEnemySystemUI.Instance.ShowWarning(false);
        SoundManager.Instance.PauseMusic(false);
        soundWarningScr.Stop();
        GameManager.Instance.Player.PausePlayer(false);

        if (showEnemyType == SHOWENEMYTYPE.ShowAll)
            ShowAllEnemy();
        else
            StartCoroutine(ShowNextGroup());

        isWorked = true;
    }

    void ShowAllEnemy()
    {
        foreach (var mono in listMono)
        {
            mono.enabled = true;
            if (sendMessage)
            {
                mono.SendMessage(trySendAMessage, SendMessageOptions.DontRequireReceiver);
                mono.SendMessage("IPlay", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    bool isShowingNextGroup = false;
    IEnumerator ShowNextGroup()
    {
        if (isShowingNextGroup)
            yield break;
        isShowingNextGroup = true;
        //Debug.LogError("Show " + currentGroup);
        if (currentGroup >= EnemyGroup.Length)
        {
            KillTheLastEnemyEvent();
            yield break;
        }
        else
        {
            int nextGroup = showOrderGroup[currentGroup];
            if (nextGroup > EnemyGroup.Length)
            {
                Debug.LogError("WRONG SET ORDER, MUST LOWER THAN OR EQUAL ENEMY GROUP NUMBER");
                yield break;
            }
            GameObject[] pickGroup = EnemyGroup[nextGroup - 1].miniGroup;

            foreach (var enemy in pickGroup)
            {
                enemy.SetActive(true);
            }
            foreach (var enemy in pickGroup)
            {
                //enemy.SetActive(true);
                MonoBehaviour[] monos = enemy.GetComponents<MonoBehaviour>();
                foreach (var mono in monos)
                {
                    listMono.Add(mono);
                    mono.enabled = true;
                    yield return null;
                    //Debug.LogError(mono.gameObject.name);
                    if (sendMessage)
                    {
                        mono.SendMessage(trySendAMessage, SendMessageOptions.DontRequireReceiver);
                        mono.SendMessage("IPlay", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            //listEnemyNeedKill.Clear();
            listEnemyNeedKill = new List<GameObject>(pickGroup);

            currentGroup++;
        }

        isShowingNextGroup = false;
    }

    private void OnDrawGizmos()
    {
        if (targetCameraLook)
            Gizmos.DrawLine(transform.position, targetCameraLook.position);
    }

    [System.Serializable]
    public class GroupMiniEnemy
    {
        public GameObject[] miniGroup;
    }
}
