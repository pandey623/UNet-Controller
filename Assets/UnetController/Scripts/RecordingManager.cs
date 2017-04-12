﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace GreenByteSoftware.UNetController {
	public class RecordingManager : MonoBehaviour {

		public string fileName = "";

		public Text buttonText;
		public Text playButtonText;
		public InputField inputField;
		public Slider tickSlider;
		public Slider speedSlider;
		public Text timeText;
		public Text speedText;

		public GameObject recordObject;
		public GameObject playbackObject;

		public GameObject playerPrefab;

		private List<PlayerData> recording;

		private uint totalTicks;
		private uint currentTick;

		private float fixedTime;

		private float tickTime = 0.05f;

		private bool updating = false;

		private float _speed = 1f;
		public float speed {
			get {
				return _speed;
			}
			set {
				_speed = value;
				if (fixedTime <= tickTime / _speed) {
					indexAddition = 1;
					_fixedUpdates =  Mathf.RoundToInt ((tickTime / _speed) / fixedTime);
				} else {
					_fixedUpdates = 1;
					indexAddition = (uint)Mathf.RoundToInt ((fixedTime / tickTime) * speed);
				}
			}
		}

		private uint indexAddition = 1;

		private int _fixedUpdates;
		public int fixedUpdates {
			get {
				return _fixedUpdates;
			}
		}
		private int _curUpdates = 0;

		private bool playing = false;

		void Start () {
			speed = 1f;
		}

		//Where playback happens
		void FixedUpdate () {
			if (!playing || recording == null)
				return;
			
			if (fixedTime != Time.fixedDeltaTime) {
				fixedTime = Time.fixedDeltaTime;
				speed = speed;
			}

			_curUpdates++;
			if (_curUpdates >= _fixedUpdates) {
				_curUpdates = 0;
				uint lastTick = currentTick;
				currentTick += indexAddition;
				if (currentTick > totalTicks) {
					currentTick = totalTicks;
					ClickPlay ();
				}
				updating = true;
				PlaybackUpdate (lastTick);
				tickSlider.value = currentTick;
				timeText.text = currentTick.ToString ();
				updating = false;
			}
		}

		void PlaybackUpdate (uint lastTick) {
			foreach (PlayerData player in recording) {
				if (currentTick >= player.startTick && currentTick < player.endTick) {
					if (!player.controller.gameObject.activeSelf)
						player.controller.gameObject.SetActive (true);
					player.controller.PlaybackSetResults (player.ticksRecord [(int) (lastTick - player.startTick)], player.ticksRecord [(int) (currentTick - player.startTick)], fixedUpdates, (currentTick == 0 || currentTick == lastTick) ? 0f : speed);
				} else {
					player.controller.gameObject.SetActive (false);
				}
			}
		}

		public void ClickOpen () {
			if (File.Exists (Path.Combine (Application.persistentDataPath, fileName + ".rec"))) {
				recording = GameManager.GetRecording (Path.Combine (Application.persistentDataPath, fileName + ".rec"), ref totalTicks, ref tickTime, playerPrefab);
				tickSlider.maxValue = totalTicks;
				playbackObject.SetActive (true);
				recordObject.SetActive (false);
				speed = speed;
				PlaybackUpdate (0);
			}
		}

		public void ClickSetTick () {
			if (updating)
				return;
			currentTick = (uint)tickSlider.value;
			PlaybackUpdate (currentTick);
		}

		public void SpeedChange () {
			speed = speedSlider.value;
			speedText.text = speed.ToString () + "x";
		}

		public void ClickPlay () {
			playing = !playing;
			if (playing)
				playButtonText.text = "Stop";
			else
				playButtonText.text = "Play";
			if (!playing)
				PlaybackUpdate (currentTick);
		}

		public void ClickRecord () {
			if (GameManager.isRecording) {
				buttonText.text = "Start Recording";
				GameManager.EndRecord ();
			} else if (fileName != string.Empty) {
				buttonText.text = "Stop Recording";
				GameManager.StartRecord (Path.Combine (Application.persistentDataPath, fileName + ".rec"));
			}
		}

		public void FileNameChange () {
			fileName = inputField.text;
		}
	}
}