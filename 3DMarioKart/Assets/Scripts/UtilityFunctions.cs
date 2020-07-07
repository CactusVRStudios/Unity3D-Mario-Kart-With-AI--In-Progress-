using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UtilityFunctions : MonoBehaviour
{
    private PlayerSounds playersounds;
    private Player playerscript;

    private bool drifting;
    private Vector3 offset;



    // Start is called before the first frame update
    void Start()
    {
        playersounds = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>();
        playerscript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.V))
        {
            drifting = true;
        }
        else
        {
            drifting = false;
        }

        camPos();
        moustacheAnim();
        hairAnim();
        trail();
    }

    public void cam_shake()
    {
        if(transform.parent.tag == "Player")
        {
            Camera.main.GetComponent<Animator>().SetTrigger("Shake");
        }
    }
    public void playCoinSound()
    {
        GetComponent<AudioSource>().Play();
    }
    public void playHopSound()
    {
        if(drifting)
            playersounds.effectSounds[6].Play();
    }
    public void gliderOpenflapSound()
    {
        playersounds.effectSounds[12].Play();
    }

    public void is_drifting()
    {
       GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().drifting = true;
    }

    public void camPos()
    {
        if(this.gameObject.name == "Main Camera Back")
        {
            transform.localPosition = new Vector3(0, -0.66f, 2.5f);
        }
    }

    public void moustacheAnim()
    {
        if(gameObject.name == "Bone_017")
        {
            if (playerscript.GetComponent<ItemManager>().isBullet)
            {
                gameObject.GetComponent<Animator>().SetBool("moustaceMove", false);
                gameObject.GetComponent<Animator>().enabled = false;
            }
            else if(!playerscript.GetComponent<ItemManager>().isBullet && playerscript.currentspeed > 40)
            {
                gameObject.GetComponent<Animator>().enabled = true;
                gameObject.GetComponent<Animator>().SetBool("moustaceMove", true);
            }

            if (playerscript.currentspeed > 40)
            {
                gameObject.GetComponent<Animator>().SetBool("moustaceMove", true);
            }
            else
            {
                gameObject.GetComponent<Animator>().SetBool("moustaceMove", false);
            }

            

        }
        
    }
    public void hairAnim()
    {
        if(gameObject.name == "Hair")
        {
            if(playerscript.currentspeed > 40)
            {
                gameObject.GetComponent<Animator>().SetBool("HairMove", true);
            }
            else
            {
                gameObject.GetComponent<Animator>().SetBool("HairMove", false);
            }
        }
    }



    public void countDownNoise()
    {
        GetComponent<AudioSource>().Play();
    }
    public void goSound()
    {
        transform.GetChild(3).GetComponent<AudioSource>().Play();
    }
    public void raceStarted()
    {
        RACE_MANAGER.RACE_STARTED = true;
    }

    public void fadeOut()
    {
        GameObject.Find("FadeInOut").GetComponent<Animator>().SetTrigger("FadeOut");
    }

    void trail()
    {
        if(gameObject.name == "Trail")
        {
            //ground normal rotation
            Ray ground = new Ray(transform.position, -transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ground, out hit, 4) && hit.normal.y > 0.5f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, 7.5f * Time.deltaTime);
                Debug.DrawRay(hit.point, hit.normal, Color.white, 20f);
            }
        }
    }



    public void startCountDown()
    {
        StartCoroutine(GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>().CountDownTImerPlay());
    }
    public void disableThisCam()
    {
        gameObject.GetComponent<Camera>().depth = -5;
    }
    public void disableCourseNameUI()
    {
        StartCoroutine(disableCourseNameUIFunc());
    }
    IEnumerator disableCourseNameUIFunc()
    {
        GameObject NameUI = GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>().CourseNameUI;

        //make text transparent
        for (int i = 0; i < 20; i++)
        {
            for(int j = 0; j < NameUI.transform.GetChild(0).childCount; j++)
            {
                Transform text = NameUI.transform.GetChild(0);
                text.GetChild(j).GetComponent<Text>().color -= new Color(0, 0, 0, 0.2f);
            }
            yield return new WaitForSeconds(0.001f);

        }

        //make images transparent
        for (int i = 0; i < 200; i++)
        {
            for (int j = 0; j < NameUI.transform.GetChild(1).childCount; j++)
            {
                Transform images = NameUI.transform.GetChild(1);
                images.GetChild(j).GetComponent<Image>().color -= new Color(0, 0, 0, 0.01f);
            }
            NameUI.GetComponent<Image>().color -= new Color(0, 0, 0, 0.02f);
            yield return new WaitForSeconds(0.001f);

        }


    }

    public void happy()
    {
        StartCoroutine(happyFacePlayer());
    }
    public IEnumerator happyFacePlayer()
    {
        for(int i = 0; i < 40; i++)
        {
            playerscript.MarioFace.material = playerscript.faces[3];
            yield return new WaitForSeconds(0.016f);
        }
    }




}
