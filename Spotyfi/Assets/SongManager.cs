using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SongManager : MonoBehaviour
{
    [SerializeField] private GameObject songPrefab;

    [SerializeField] private string directoryPath; // The path to the directory containing the MP3s.
    private List<AudioClip> clips = new List<AudioClip>(); // The list of AudioClips.

    [SerializeField] private AudioSource audioSource;
    
    private int currentSongIndex = 0;

    [Header("References")]
    [SerializeField] private Sprite[] volumeSprites;
    [SerializeField] private Image volumeImage;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text currentTimeText;
    [SerializeField] private TMP_Text maxTimeText;
    [SerializeField] private TMP_Text songNameText;
    [SerializeField] private TMP_Text artistNameText;

    void Update()
    {
        //Update the volumeImage based on volumeSlider value
        if(volumeSlider.value > 0.5)
        {
            volumeImage.sprite = volumeSprites[2];
        }else if(volumeSlider.value > 0)
        {
            volumeImage.sprite = volumeSprites[1];
        }
        else
        {
            volumeImage.sprite = volumeSprites[0];
        }

        //Set source volume to volumeSlider value
        audioSource.volume = volumeSlider.value;

        //Update currentTimeText & progressSlider
        currentTimeText.text = $"{Mathf.FloorToInt(audioSource.time / 60f).ToString()}:{(((int)(audioSource.time%60)).ToString().Length == 1 ? ("0" + (int)(audioSource.time%60)) : (int)(audioSource.time%60))}";
        progressBar.value = audioSource.time/audioSource.clip.length;
    }

    public void UpdateProgress()
    {
        if (Input.GetMouseButton(0) && Input.mousePosition.x < 1400)
            audioSource.time = progressBar.value * audioSource.clip.length;
    }

    private void Start()
    {
        LoadFiles();
    }

    public void PreviousTrack()
    {
        if(currentSongIndex-1 != -1)
        {
            currentSongIndex--;
        }
        else
        {
            currentSongIndex = clips.Count-1;
        }
        UpdateTrack();
    }

    public void PlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }

    public void NextTrack()
    {
        if (currentSongIndex + 1 != clips.Count)
        {
            currentSongIndex++;
        }
        else
        {
            currentSongIndex = 0;
        }
        UpdateTrack();
    }

    public void PlaySong(int index)
    {
        currentSongIndex = index;
        UpdateTrack();
    }

    void UpdateTrack()
    {
        audioSource.Stop();
        audioSource.clip = clips[currentSongIndex];
        artistNameText.text = files[currentSongIndex].Split(" - ")[0].Remove(0, files[currentSongIndex].Split(" - ")[0].LastIndexOf('\\') + 1);
        if(Regex.Matches(files[currentSongIndex], " - ").Count == 2)
            songNameText.text = files[currentSongIndex].Split(" - ")[1] + files[currentSongIndex].Split(" - ")[2].TrimEnd(".ogg");
        else
            songNameText.text = files[currentSongIndex].Split(" - ")[1].TrimEnd(".ogg");
        maxTimeText.text = $"{Mathf.FloorToInt(audioSource.clip.length / 60f).ToString()}:{(((int)(audioSource.clip.length % 60)).ToString().Length == 1 ? ("0" + (int)(audioSource.clip.length % 60)) : (int)(audioSource.clip.length % 60))}";
        audioSource.Play();
    }

    public List<string> files = new List<string>();
    [SerializeField] private Transform scrollViewContent;
    public void LoadFiles()
    {
        //Grabs all files from FileDirectory
        files = Directory.GetFiles(directoryPath).ToList<string>();

        //Checks all files in directory and stores all OGG files into the files list.
        for (int i = 0; i < files.Count; i++)
        {
            if (files[i].EndsWith(".ogg"))
            {
                //Add song to clips list
                clips.Add(new WWW(files[i]).GetAudioClip(false, true, AudioType.OGGVORBIS));
                //Instantiate song widget prefab with a position offset on the y axis directly proportional to the value of i and store it in the "song" variable
                var song = Instantiate(songPrefab, new Vector3(960, 0 - i * 120, 0), Quaternion.identity, scrollViewContent);
                //Get all the TMP_Texts
                TMP_Text[] texts = song.GetComponentsInChildren<TMP_Text>();
                //Set the song name
                if (Regex.Matches(files[currentSongIndex], " - ").Count == 2)
                    texts[0].text = files[i].Split(" - ")[1] + files[i].Split(" - ")[2].TrimEnd(".ogg");
                else
                    texts[0].text = files[i].Split(" - ")[1].TrimEnd(".ogg");
                //Set the artist name
                texts[1].text = files[i].Split(" - ")[0].Remove(0, files[i].Split(" - ")[0].LastIndexOf('\\') + 1);
                //Set the song duration
                texts[2].text = $"{Mathf.FloorToInt(clips[i].length / 60f)}:{(((int)(clips[i].length % 60)).ToString().Length == 1 ? ("0" + (int)(clips[i].length % 60)) : (int)(clips[i].length % 60))}".ToString();
                song.GetComponent<Song>().manager = this;
                song.GetComponent<Song>().songIndex = i;
            }
        }
        //Set the y position of the content/song widgets parent to 2600 so it starts with the songs on the screen
        scrollViewContent.position = new Vector3(960, 2600, 0);

        UpdateTrack();
    }
}