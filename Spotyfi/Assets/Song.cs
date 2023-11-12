using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Song : MonoBehaviour
{
    public SongManager manager;
    public int songIndex = 0;

    public void PlaySong()
    {
        manager.PlaySong(songIndex);
    }
}
