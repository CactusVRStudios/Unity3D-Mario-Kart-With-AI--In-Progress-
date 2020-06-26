using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RACE_MANAGER : MonoBehaviour
{
    public GameObject FrontCam;
    public GameObject FrontFPCam;
    public GameObject BackCam;
    bool FPCam = false; //first person cam

    public AudioSource music;
    public GameObject CountDownTimer;
    public GameObject CoinCounter;
    public GameObject CourseNameUI;

    public static bool RACE_STARTED = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       

        if (RACE_STARTED)
        {
            if (!music.isPlaying)
            {
                music.Play();
            }
        }
        //camera stuff
        if (Input.GetKeyDown(KeyCode.Alpha1) && RACE_STARTED) //if pressed 1 and back cam is not enabled, disable front cam and enable FP cam
        {
            if (!Input.GetKey(KeyCode.B))
            {
                FPCam = true;
                FrontFPCam.SetActive(true);
                FrontCam.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && RACE_STARTED) //if pressed 1 and back cam is not enabled, disable FrontFP cam and enable regular front cam
        {
            if (!Input.GetKey(KeyCode.B))
            {
                FPCam = false;
                FrontFPCam.SetActive(false);
                FrontCam.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.B) && RACE_STARTED)
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
        if (Input.GetKeyUp(KeyCode.B) && RACE_STARTED)
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
    }

    public IEnumerator CountDownTImerPlay()
    {
        GameObject.Find("FadeInOut").GetComponent<Animator>().SetTrigger("FadeIn");//fade in anim
        CoinCounter.SetActive(true);

        //make the main camera active, start it's entry animation, wait a bit before playing the audio, and then disable the sceneEntry camera, and start the countdown in 4.5 seconds
        FrontCam.SetActive(true);                                                                   
        FrontCam.GetComponent<Animator>().SetTrigger("Entry");
        yield return new WaitForSeconds(0.5f);
        FrontCam.GetComponent<AudioSource>().Play();
        GameObject.Find("SceneEntryCamera").GetComponent<Camera>().enabled = false;
        yield return new WaitForSeconds(4.5f);
        CountDownTimer.GetComponent<Animator>().SetTrigger("Timer");
    }
}
