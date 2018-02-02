using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.IO;
using UnityEditor;

public class RoomLoader : MonoBehaviour {

    public string currentGame;
    public GameObject backgroundImage;
    public GameObject hintVideo;
    public GameObject enemy;
    public GameObject iris;
    public GameObject alertPanel;
    public int phraseLength = 3;
    public Vector3 irisStartPosition;
    public Vector3 irisWaitPosition;
    public Vector3 irisEndPosition;
    public GameObject irisRunObject;
    public GameObject irisMeowObject;
    public GameObject irisAttackObject;
    public GameObject irisAttentionObject;
    public GameObject confusionObject;
    public float alertTimerLength = 0.25f;
    private string gestureDataPath;
    private GameObject irisGO;
    private GameObject irisMagic;
    public GameObject burnAnim;
    private GameObject confusionAnim;
    //List of phrases
    public string[,] phrases = { { "snake_under_chair", "spider_under_wagon", "spider_under_bed", "spider_on_wall", "spider_on_chair", "spider_in_box", "snake_under_bed", "snake_on_box", "snake_in_flowers", "snake_behind_wall", "cat_under_chair", "cat_on_wall", "cat_behind_flowers", "cat_behind_box", "cat_behind_bed", "alligator_on_bed", "alligator_in_wagon", "alligator_in_box", "alligator_behind_wall", "alligator_behind_chair" }, { "spider_under_blue_chair", "spider_on_white_wall", "spider_in_orange_flowers", "spider_in_green_box", "spider_in_blue_box", "snake_under_blue_flowers", "snake_under_blue_chair", "cat_under_orange_chair", "snake_under_black_chair", "snake_in_green_wagon", "cat_under_blue_bed", "cat_on_green_wall", "cat_on_blue_bed", "cat_behind_orange_bed", "alligator_under_green_bed", "alligator_on_blue_wall", "alligator_in_orange_flowers", "alligator_behind_orange_wagon", "alligator_behind_blue_wagon", "alligator_behind_black_wall" }, { "blue_spider_on_green_box", "green_spider_under_orange_chair", "green_alligator_under_blue_flowers", "black_spider_in_white_flowers", "black_cat_on_green_bed", "black_cat_in_blue_wagon", "black_cat_behind_green_bed", "orange_snake_under_blue_flowers", "orange_spider_under_green_flowers", "orange_alligator_in_green_flowers", "white_cat_in_green_box", "white_cat_on_orange_wall", "white_alligator_on_blue_wall", "white_snake_in_blue_flowers", "orange_spider_in_green_box", "black_alligator_behind_orange_wagon", "green_snake_under_blue_chair", "blue_alligator_on_green_wall", "black_snake_under_blue_chair", "" } };

    public string currentPhrase;
    private bool firstUpdated;
    static List<int>[] donePhrases = new List<int>[3];
    static int score = 0;
    private int frameCount = 0;
    private float alertTimer = 0.0f;
    private float burnTimer = 0.0f;
    private bool uploadNeeded = false;
    //private bool alerting = false;
    //private bool attacking = false;
    //private bool confused = false;
    //private bool irisExiting = false;
    //private string jointData = "";

    private enum LevelState
    {
        Entering,
        WaitingForAction,
        Listening,
        Confused,
        Thinking,
        Attacking,
        EnemyBurning,
        Exiting
    };

    LevelState levelState;

    // Use this for initialization
    void Start()
    {
        if (phraseLength < 6)
        {
            //Select a phrase at random
            phraseLength = Random.Range(3, 6);
            int currNum = phraseLength < 5 ? Random.Range(0, 20) : Random.Range(0, 19);

            for (int i = 0; i < 3; ++i)
                if (donePhrases[i] == null) donePhrases[i] = new List<int>();

            while (donePhrases[phraseLength - 3].Contains(currNum))
            {
                phraseLength = Random.Range(3, 6);
                currNum = phraseLength < 5 ? Random.Range(0, 20) : Random.Range(0, 19);
            }
            donePhrases[phraseLength - 3].Add(currNum);
            currentPhrase = phrases[phraseLength - 3, currNum];

            //Terminate Level if all signs done
            if (donePhrases[0].Count == 20 && donePhrases[1].Count == 20 && donePhrases[2].Count == 19)
            {
                Application.Quit();
            }
        }
        else
        {
            currentPhrase = "alligator_above_bed";
        }

        try
        {
            string answerFilePath = "C:/Ubuntu Image/sf/live-verifier/answer.txt";
            answerFilePath.Replace("/", "\\");
            File.Delete(answerFilePath);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }

        //Load the default background
        SpriteRenderer backgroundSpriteRenderer = backgroundImage.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = Resources.Load<Sprite>(@"Games/" + currentGame + @"/images/backgrounds/room background");

        //Load Iris animation prefab and make it a child of the iris GameObject
        UpdateIrisAnimationWithGameObject(irisRunObject);
        iris.transform.localPosition = irisStartPosition;

        //Get the video corresponding to the randomly selected phrase
        hintVideo.GetComponent<VideoPlayer>().url = @"Assets\Resources\SignAssets\" + phraseLength.ToString() + @"\" + currentPhrase + @"\" + currentPhrase + ".mp4";
        hintVideo.GetComponent<VideoPlayer>().isLooping = false;

        //To show first frame of video
        firstUpdated = false;

        //Get the enemy prefab corresponding to the randomly selected phrase
        enemy = Instantiate(Resources.Load<GameObject>(@"SignAssets/" + phraseLength.ToString() + @"/" + currentPhrase + @"/" + currentPhrase), enemy.transform.position, Quaternion.identity);
        enemy.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        //Set default values 
        levelState = LevelState.Entering;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //if Iris is walking towards her target position...
        if (levelState == LevelState.Entering && iris.transform.localPosition.x < irisWaitPosition.x)
        {
            //... move Iris until her target position
            iris.transform.localPosition = Vector3.MoveTowards(iris.transform.localPosition, irisWaitPosition, Time.deltaTime);
            if (iris.transform.localPosition.x >= irisWaitPosition.x)
            {
                iris.transform.localPosition = irisWaitPosition;
                levelState = LevelState.WaitingForAction;

                //Remove the walking animation and add the meow animation
                UpdateIrisAnimationWithGameObject(irisMeowObject);
            }
        }

        //Flash the Kinect window border to alert the user
        if ( levelState == LevelState.Listening )
        {
            alertTimer -= Time.deltaTime;
            if ( alertTimer <= 0 )
            {
                alertPanel.SetActive(!alertPanel.activeInHierarchy);
                alertTimer = alertTimerLength;
            }
        }

        else
        {
            alertTimer = 0.0f;
            alertPanel.SetActive(false);
        }

        if ( levelState > LevelState.Entering && levelState <= LevelState.Thinking )
        {
            //End level using Shift
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(2))
            {
                StartAttack();
            }
        }

        if ( levelState == LevelState.Thinking )
        {
            if (uploadNeeded)
            {
                try
                {
                    gestureDataPath.Replace("/", "\\");
                    string inputFilePath = @"C:/Ubuntu Image/sf/live-verifier/data/txt/input.txt";
                    inputFilePath.Replace("/", "\\");
                    inputFilePath.Replace("\\\\", "\\");
                    string answerFilePath = "C:/Ubuntu Image/sf/live-verifier/answer.txt";
                    answerFilePath.Replace("/", "\\");
                    File.Delete(answerFilePath);
                    File.Copy(gestureDataPath, inputFilePath, true);
                    uploadNeeded = false;
                }
                catch (System.Exception e)
                {

                }
            }
            else
            {
                try
                {
                    string answerFilePath = "C:/Ubuntu Image/sf/live-verifier/answer.txt";
                    answerFilePath.Replace("/", "\\");
                    StreamReader reader = new StreamReader(answerFilePath);
                    string result = reader.ReadToEnd();
                    reader.Close();
                    File.Delete(answerFilePath);
                    Debug.Log(result);
                    if (result == "YES")
                        StartAttack();
                    else
                        StartConfusion();
                }
                catch (System.Exception e)
                {
                    //Debug.Log(e.Message);
                }
            }
        }

        if ( levelState == LevelState.Attacking )
        {
            if (irisMagic.transform.localPosition != enemy.transform.localPosition)
                irisMagic.transform.localPosition = Vector3.MoveTowards(irisMagic.transform.localPosition, enemy.transform.localPosition, 3*Time.deltaTime);
            else
            {
                Destroy(irisMagic);
                levelState = LevelState.EnemyBurning;
                Destroy(enemy);
                Vector3 enemyPos = enemy.transform.position;
                enemy = Instantiate(burnAnim, enemyPos, Quaternion.identity);
                burnTimer = 0.9f;
                UpdateIrisAnimationWithGameObject(irisMeowObject);
            }
        }

        if ( levelState == LevelState.EnemyBurning )
        {
            burnTimer -= Time.deltaTime;
            if ( burnTimer <= 0 )
            {
                Destroy(enemy);
                levelState = LevelState.Exiting;
                UpdateIrisAnimationWithGameObject(irisRunObject);
            }
        }

        if (levelState == LevelState.Exiting && iris.transform.localPosition.x < irisEndPosition.x)
        {
            //... move Iris until her target position
            iris.transform.localPosition = Vector3.MoveTowards(iris.transform.localPosition, irisEndPosition, Time.deltaTime);
            if (iris.transform.localPosition.x >= irisEndPosition.x)
            {
                iris.transform.localPosition = irisEndPosition;
                SceneManager.LoadScene("SampleScene");
            }
        }
    }

    private void FixedUpdate()
    {
        if(frameCount < 10)
        {
            frameCount++;
        }
        if(frameCount == 10)
        {
            hintVideo.GetComponent<VideoPlayer>().Pause();
            frameCount++;
        }
    }

    public void PlayHint()
    {
        //Pause if already playing
        if (hintVideo.GetComponent<VideoPlayer>().isPlaying)
        {
            hintVideo.GetComponent<VideoPlayer>().Pause();
        }
        else
        {
            //rewind video to first frame and play
            hintVideo.GetComponent<VideoPlayer>().frame = 1;
            hintVideo.GetComponent<VideoPlayer>().Play();
        }
    }

    IEnumerator Upload(string jointData)
    {
        byte[] myData = System.Text.Encoding.UTF8.GetBytes(jointData);
        UnityWebRequest www = UnityWebRequest.Put("http://localhost:8000", myData);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
            StartConfusion();
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            if ( true )             // Incorrect Sign
            {
                StartConfusion();
            }
            else
            {
                StartAttack();
            }
        }
    }

    private void StartConfusion()
    {
        levelState = LevelState.Confused;
        confusionAnim = Instantiate(confusionObject, Vector3.zero, Quaternion.identity);
        confusionAnim.transform.parent = iris.transform;
        confusionAnim.transform.localPosition = new Vector3(0.0f, 1.15f, 0.0f);
    }

    public void PlayAlert()
    {
        if ( levelState != LevelState.Listening )
        {
            UpdateIrisAnimationWithGameObject(irisAttentionObject);
            if (levelState == LevelState.Confused)
            {
                Destroy(confusionAnim);
            }
            levelState = LevelState.Listening;
        }
        else
        {
            levelState = LevelState.Thinking;
            uploadNeeded = true;
            //gestureDataPath = @"C:/PhraseData/orange_spider_in_green_box/good/24/_24.txt";
            //gestureDataPath.Replace("/", "\\");
            //string inputFilePath = @"C:/Ubuntu Image/sf/live-verifier/data/txt/input.txt";
            //inputFilePath.Replace("/", "\\");
            //File.Copy(gestureDataPath, inputFilePath, true);
            ////StreamReader reader = new StreamReader(gestureDataPath);
            ////jointData = reader.ReadToEnd();
            //reader.Close();
            //StartCoroutine(Upload(reader.ReadToEnd()));
        }
    }

    private void StartAttack()
    {
        if (levelState == LevelState.Confused)
            Destroy(confusionAnim);

        levelState = LevelState.Attacking;
        UpdateIrisAnimationWithGameObject(irisAttackObject);
        irisMagic = Instantiate(Resources.Load<GameObject>(@"Games/" + currentGame + @"/images/IrisMagic/magic"), irisGO.transform.position, Quaternion.identity);
        //SceneManager.LoadScene("SampleScene");
    }

    private void UpdateIrisAnimationWithGameObject(GameObject irisAnimObject)
    {
        Destroy(irisGO);
        irisGO = Instantiate(irisAnimObject, Vector3.zero, Quaternion.identity);
        irisGO.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        irisGO.transform.parent = iris.transform;
        irisGO.transform.localPosition = Vector3.zero;
    }

    public void SetGestureDataPath(string path)
    {
        gestureDataPath = path;
        gestureDataPath.Replace("\\\\", "\\");
        Debug.Log(gestureDataPath);
    }
}
