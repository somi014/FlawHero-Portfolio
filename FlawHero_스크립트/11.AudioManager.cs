using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]       //커스텀 클래스는 인스펙터 창에 나타나지 않음
public class Sound
{
    public string           name;           //사운드 이름

    public  AudioClip       clip;           //사운드 파일
    private AudioSource     source;         //사운드 플레이어

    public float            Volumn;
    public bool             loop;

    public void SetSource(AudioSource _source)
    {
        source = _source;
        source.clip = clip;
        source.volume = Volumn;
        source.loop = loop;
    }

    public void SetVolumn()
    {
        source.volume = Volumn;
    }

    public void Play()
    {
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }

    public void SetLoop()
    {
        source.loop = true;
    }

    public void SetLoopCancel()
    {
        source.loop = false;
    }
}

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    public Sound[] sounds;
    public Slider BGMSlider;
    public Slider effectSlider;

   
    void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            GameObject soundObject = new GameObject("사운드 파일 이름 : " + i + " = " + sounds[i].name);
            sounds[i].SetSource(soundObject.AddComponent<AudioSource>());
            soundObject.transform.SetParent(this.transform);
        }
    }

    public void Play(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (_name == sounds[i].name)
            {
                sounds[i].Play();
                return;
            }
        }
    }

    public void Stop(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (_name == sounds[i].name)
            {
                sounds[i].Stop();
                return;
            }
        }
    }

    public void SetLoop(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (_name == sounds[i].name)
            {
                sounds[i].SetLoop();
                return;
            }
        }
    }

    public void SetLoopCancel(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (_name == sounds[i].name)
            {
                sounds[i].SetLoopCancel();
                return;
            }
        }
    }

    public void SetVolumn(float _Volumn)
    {
        _Volumn = effectSlider.value;
        for (int i = 0; i < sounds.Length; i++)
        {
            if (!sounds[i].name.Contains("Background"))
            {
                sounds[i].Volumn = _Volumn;
                sounds[i].SetVolumn();
            }
        }
    }

    public void SetBGMVolumn(float _Volumn)
    {
        _Volumn = BGMSlider.value;
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name.Contains("Background"))
            {
                sounds[i].Volumn = _Volumn;
                sounds[i].SetVolumn();
            }
        }
    }
}
