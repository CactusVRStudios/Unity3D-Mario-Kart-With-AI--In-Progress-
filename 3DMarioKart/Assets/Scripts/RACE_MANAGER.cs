using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RACE_MANAGER : MonoBehaviour
{
    public GameObject FrontCam;
    public GameObject FrontFPCam;
    public GameObject BackCam;
    bool FPCam = false; //first person cam

    public AudioSource music;
    public AudioSource musicFast;
    bool lastLap;

    public GameObject CountDownTimer;
    public GameObject CoinCounter;
    public GameObject LapCounter;
    public GameObject MiniMap;
    public GameObject PositionCounter;
    public GameObject CourseNameUI;


    public List<LapCounter> lapCounters = new List<LapCounter>();
    public List<LapCounter> sortedRacers = new List<LapCounter>();


    private float sortTime = 0;

    public static bool RACE_STARTED = false;
    public static bool RACE_COMPLETED = false;
    public static bool raceFinishStuff = false;

    private int lastPos;

    public GameObject spectatorSounds;
    public GameObject itemSystem;
    public ResultsUI resultsUI;
    public GameObject finishUI;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       

        if (RACE_STARTED && !RACE_COMPLETED)
        {
            if (!music.isPlaying && !lastLap)
            {
                music.Play();
            }
            sortTime += Time.deltaTime;

            if(sortTime > 0.1f)
            {
                calculateRacerPosition();
                sortTime = 0;
            }

        }
        //camera stuff
        if (Input.GetKeyDown(KeyCode.Alpha1) && RACE_STARTED && !RACE_COMPLETED) //if pressed 1 and back cam is not enabled, disable front cam and enable FP cam
        {
            if (!Input.GetKey(KeyCode.B))
            {
                FPCam = true;
                FrontFPCam.SetActive(true);
                FrontCam.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && RACE_STARTED && !RACE_COMPLETED) //if pressed 1 and back cam is not enabled, disable FrontFP cam and enable regular front cam
        {
            if (!Input.GetKey(KeyCode.B))
            {
                FPCam = false;
                FrontFPCam.SetActive(false);
                FrontCam.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.B) && RACE_STARTED && !RACE_COMPLETED)
        {
            if (FPCam)
            {
                BackCam.SetActive(true);
                FrontFPCam.SetActive(false);
            }
            else
            {
                BackCam.SetActive(true);
                FrontCam.SetActive(false);
            }
            
        }
        if (Input.GetKeyUp(KeyCode.B) && RACE_STARTED && !RACE_COMPLETED)
        {
            if (FPCam)
            {
                FrontFPCam.SetActive(true);
                BackCam.SetActive(false);
            }
            else
            {
                BackCam.SetActive(false);
                FrontCam.SetActive(true);
            }
            
        }

        if (RACE_COMPLETED && !raceFinishStuff)
        {
            raceFinishStuff = true;
            StartCoroutine(FinishRace());
        }

        if(GameObject.FindGameObjectWithTag("Player").GetComponent<LapCounter>().LAPCOUNT == 3 && !lastLap)
        {
            lastLap = true;
            music.Stop();
            musicFast.Play();
        }
    }

    public IEnumerator CountDownTImerPlay()
    {
        GameObject.Find("FadeInOut").GetComponent<Animator>().SetTrigger("FadeIn");//fade in anim
        CoinCounter.SetActive(true);
        LapCounter.SetActive(true);
        MiniMap.SetActive(true);

        //make the main camera active, start it's entry animation, wait a bit before playing the audio, and then disable the sceneEntry camera, and start the countdown in 4.5 seconds
        FrontCam.SetActive(true);                                                                   
        FrontCam.GetComponent<Animator>().SetTrigger("Entry");
        yield return new WaitForSeconds(0.5f);
        FrontCam.GetComponent<AudioSource>().Play();
        GameObject.Find("SceneEntryCamera").GetComponent<Camera>().enabled = false;
        yield return new WaitForSeconds(4.5f);
        CountDownTimer.GetComponent<Animator>().SetTrigger("Timer");
    }

    public void calculateRacerPosition()
    {

        sortedRacers = new List<LapCounter>(lapCounters);
        sortedRacers.Sort(SortByScore);


        for(int i = 0; i < sortedRacers.Count; i++)
        {
            sortedRacers[i].Position = i + 1;
        }

    }

    int SortByScore(LapCounter p1, LapCounter p2)
    {
        return -p1.totalCheckpointVal.CompareTo(p2.totalCheckpointVal);
    }

    IEnumerator FinishRace()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().effectSounds[14].Play();

        spectatorSounds.SetActive(false);
        itemSystem.SetActive(false);
        FrontCam.transform.parent.parent.GetComponent<AudioSource>().Stop();
        finishUI.GetComponent<Animator>().SetBool("Finish", true);
        BackCam.SetActive(false);
        FrontCam.SetActive(true);

        if(GameObject.FindGameObjectWithTag("Player").GetComponent<LapCounter>().Position == 1)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Driver.SetBool("FirstPlace", true);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().marioFirstPlace.Play();
            yield return new WaitForSeconds(1);

            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().effectSounds[15].Play();
            yield return new WaitForSeconds(2.5f);
            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);
            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().effectSounds[17].Play();

        }
        else if (GameObject.FindGameObjectWithTag("Player").GetComponent<LapCounter>().Position < 6)
        {
            //for now it is the same thing
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Driver.SetBool("FirstPlace", true);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().marioFirstPlace.Play();
            yield return new WaitForSeconds(1);

            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().effectSounds[16].Play(); //except for this
            yield return new WaitForSeconds(2.5f);
            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);
            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().effectSounds[17].Play();
        } 

    }
}
