using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour {
    private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start() {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer != null) {
            videoPlayer.Play();
        }
        else {
            Debug.LogError(
                "No VideoPlayer component found on the GameObject. Please attach this script to a GameObject with a VideoPlayer component.");
        }
    }

    public void PlayVideo() {
        if (videoPlayer != null) {
            videoPlayer.Play();
        }
    }

    public void PauseVideo() {
        if (videoPlayer != null) {
            videoPlayer.Pause();
        }
    }
}