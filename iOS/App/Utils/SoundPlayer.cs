using System;
using System.Diagnostics;
using System.IO;
using AVFoundation;
using Foundation;
using Trace.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(SoundPlayer))]

namespace Trace.iOS {

	public class SoundPlayer : ISoundPlayer {

		private static AVAudioPlayer player;
		private static bool isActive;

		private string prevSound = "silence.mp3";
		private float musicVolume = 0.5f;


		public void PlaySound(string sound, int loops = -1) {
			if(!App.IsInBackground) {
				prevSound = sound ?? prevSound;
				return;
			}

			// Play previous track if none is provided.
			if(string.IsNullOrEmpty(sound))
				sound = prevSound;

			// Any existing player?
			if(player != null) {
				player.Stop();
				player.Dispose();
			}

			// Initialize player
			var songPath = "sounds/" + sound;
			string fileExtension = Path.GetExtension(sound).Substring(1); // returns 'wav' or 'mp3'
			NSError err;
			try {
				player = new AVAudioPlayer(NSUrl.FromFilename(songPath), fileExtension, out err);
			}
			catch(Exception e) {
				Debug.WriteLine(e); return;
			}
			Debug.WriteLine("Playing sound: " + sound);
			player.Volume = musicVolume;
			player.FinishedPlaying += delegate {
				Debug.WriteLine("StartAudioSession().FinishedPlaying ->" + sound);
				player = null;
			};
			player.NumberOfLoops = loops;
			player.Play();
			prevSound = sound;
		}


		public void StopSound() {
			if(player != null) {
				player.Stop();
				player.Dispose();
			}
		}


		public void PlayShortSound(string newSound, int loops = 0) {
			PlaySound(newSound, loops);

			// Restart the player using the appropriate background sound in order to prevent app suspension.
			player.FinishedPlaying += (sender, e) => {
				if(isActive) {
					PlaySound(null);
				}
			};
		}


		public void AdjustVolume(float volume) {
			musicVolume = volume;
			if(player != null) { player.Volume = volume; }
		}


		public bool IsPlaying() {
			return player?.Playing ?? false;
		}


		public void ActivateAudioSession() {
			// Initialize Audio
			var session = AVAudioSession.SharedInstance();

			// If session type is still the default one, change it to 'Playback' mode, which allows background audio.
			if(session.Category.ToString().Equals("AVAudioSessionCategorySoloAmbient"))
				session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers);
			//AVAudioSession.Notifications.ObserveInterruption((sender, e) => { Debug.WriteLine("Audio Interruption: " + e.Notification); });
			session.SetActive(true);
			isActive = true;
		}


		public void DeactivateAudioSession() {
			var session = AVAudioSession.SharedInstance();
			session.SetActive(false);
			isActive = false;
		}
	}
}
