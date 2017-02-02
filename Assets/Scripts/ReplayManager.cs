/* IN-GAME REPLAY - @madebyfeno - <feno@ironequal.com>
 * You can use it in commercial projects (and non-commercial project of course), modify it and share it.
 * Do not resell the resources of this project as-is or even modified. 
 * TL;DR: Do what the fuck you want but don't re-sell it
 * 
 * ironequal.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Replay
{
	public class ReplayManager : MonoBehaviour
	{
		public int recordRate = 120;
		public bool isRecording = false;
		public bool isPlaying = false;
		public static ReplayManager Singleton;
		public Action<float> OnReplayTimeChange;
		public Action OnReplayStart;

		private bool wasPlaying = true;

		private bool replayReplayAvailable = false;

		#region UI

		public Slider _slide;
		public Image _play;
		public Image _replay;
		public Image _pause;
		public Text _timestamp;
		public GameObject _replayCanvas;

		#endregion

		#region Time

		private float _startTime;
		private float _endTime;

		#endregion


		void Awake ()
		{
			if (ReplayManager.Singleton == null) {
				ReplayManager.Singleton = this;
			} else {
				Destroy (gameObject);
			}
		}


		public float GetCurrentTime ()
		{
			return Time.time - _startTime;
		}

		void StartReplay ()
		{
			_endTime = Time.time;
			_replayCanvas.SetActive (true);
			isPlaying = false;
			_replayCanvas.GetComponent<CanvasGroup> ().alpha = 1;
			_slide.maxValue = _endTime - _startTime;
			OnReplayTimeChange (0);
			RefreshTimer ();

			if (OnReplayStart != null) {
				// You can remove this log if you don't care
				#if UNITY_EDITOR
				Debug.Log ("There's " + OnReplayStart.GetInvocationList ().Length + " objects affected by the replay.");
				#endif

				OnReplayStart ();
			}
		}
		// Use this for initialization
		void Start ()
		{
			// This line call the replay to start after 3 seconds. You can remove this line and call StartReplay when you want.
			Invoke ("StartReplay", 3f);

			isRecording = true;
			_startTime = Time.time;

			_slide = _replayCanvas.GetComponentInChildren<Slider> ();


			_play.GetComponent<Button> ().onClick.AddListener (() => Play ());
			_pause.GetComponent<Button> ().onClick.AddListener (() => Pause ());
			_replay.GetComponent<Button> ().onClick.AddListener (() => ReplayReplay ());
			_slide.GetComponent<Slider> ().onValueChanged.AddListener ((Single v) => SetCursor (v));


			EventTrigger trigger = _slide.GetComponent<EventTrigger> ();
			{
				EventTrigger.Entry entry = new EventTrigger.Entry ();
				entry.eventID = EventTriggerType.PointerDown;
				entry.callback.AddListener ((eventData) => {
					wasPlaying = isPlaying;
					Pause ();
				});
				trigger.triggers.Add (entry);
			}
			{
				EventTrigger.Entry entry = new EventTrigger.Entry ();
				entry.eventID = EventTriggerType.PointerUp;
				entry.callback.AddListener ((eventData) => {
					if (wasPlaying)
						Play ();
				});
				trigger.triggers.Add (entry);
			}

			trigger = _slide.transform.parent.GetComponent<EventTrigger> ();
			{
				EventTrigger.Entry entry = new EventTrigger.Entry ();
				entry.eventID = EventTriggerType.PointerExit;
				entry.callback.AddListener ((eventData) => {
					_slide.handleRect.transform.localScale = Vector3.zero;
				});
				trigger.triggers.Add (entry);
			}
			{
				EventTrigger.Entry entry = new EventTrigger.Entry ();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback.AddListener ((eventData) => {
					_slide.handleRect.transform.localScale = Vector3.one;
				});
				trigger.triggers.Add (entry);
			}
		}

	
		// Update is called once per frame
		void Update ()
		{
			if (isPlaying) {
				_slide.value += Time.deltaTime * Time.timeScale;

				OnReplayTimeChange (_slide.value);
			}


			// You can remove/modify this if you use Space for something else
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (isPlaying) {
					Pause ();
				} else {
					Play ();
				}
			}
			// ------
		}

		public void Play ()
		{
			_slide.Select ();
			if (!isPlaying && _slide.value != _endTime - _startTime) {
				isPlaying = true;

				Swap (_play.gameObject, _pause.gameObject);

				if (_play.transform.GetSiblingIndex () > _pause.transform.GetSiblingIndex ()) {
					_play.transform.SetSiblingIndex (_pause.transform.GetSiblingIndex ());
				}
			}
		}

		void Swap (GameObject _out, GameObject _in = null, float delay = 0f)
		{
		
			if (_in != null) {
				_in.SetActive (true);
			}

			_out.SetActive (false);
		}

		public void Pause ()
		{
			_slide.Select ();
			if (isPlaying) {
				isPlaying = false;

				Swap (_pause.gameObject, _play.gameObject);

				if (_pause.transform.GetSiblingIndex () > _play.transform.GetSiblingIndex ()) {
					_pause.transform.SetSiblingIndex (_play.transform.GetSiblingIndex ());
				}
			}
		}

		public void ReplayReplay ()
		{
			_slide.value = 0;
			replayReplayAvailable = false;
			Swap (_replay.gameObject);
			Play ();

		}

		public void SetCursor (Single value)
		{
			RefreshTimer ();

			if (replayReplayAvailable) {
				replayReplayAvailable = false;
				Swap (_replay.gameObject, _play.gameObject);
			}

			if (_slide.value == _endTime - _startTime) {
				Pause ();

				replayReplayAvailable = true;
				Swap (_play.gameObject, _replay.gameObject, .2f);
			}

			if (OnReplayTimeChange != null) {
				OnReplayTimeChange (value + _startTime);
			}
		}

		void RefreshTimer ()
		{
			float current = _slide.value;
			float total = (_endTime - _startTime);

			string currentMinutes = Mathf.Floor (current / 60).ToString ("00");
			string currentSeconds = (current % 60).ToString ("00");

			string totalMinutes = Mathf.Floor (total / 60).ToString ("00");
			string totalSeconds = (total % 60).ToString ("00");

			_timestamp.text = currentMinutes + ":" + currentSeconds + " / " + totalMinutes + ":" + totalSeconds;
		}

		#if UNITY_EDITOR
		void OnDestroy ()
		{
			Debug.LogWarning (gameObject.name + " destroyed.");
		}
		#endif
	}
}