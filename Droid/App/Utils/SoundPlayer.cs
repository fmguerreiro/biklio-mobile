using System;
using System.Diagnostics;
using System.IO;
using Android.Media;
using Trace.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(SoundPlayer))]

namespace Trace.Droid {

	public class SoundPlayer : ISoundPlayer {

		private static MediaPlayer player;
		private static bool isActive;

		private float musicVolume = 0.5f;


		public void PlaySound(string sound, int loops = -1) {

			//// Play background track if none is provided.
			//if(string.IsNullOrEmpty(sound))
			//	sound = getBackgroundSound();

			//// Any existing player?
			//if(player != null) {
			//	player.Stop();
			//	player.Dispose();
			//}

			//// Initialize player
			//var songPath = "raw/" + sound;
			////string fileExtension = Path.GetExtension(sound).Substring(1); // returns 'wav' or 'mp3'
			//player = new MediaPlayer();
			//player.SetAudioStreamType(Android.Media.Stream.Music);
			//player.SetVolume(musicVolume, musicVolume);

			//player.Completion += (sender, args) => {
			//	player.Stop();
			//	if(loops == -1) {
			//		player.Reset();
			//		player.Start();
			//	}
			//	else if(loops > 0) {
			//		player.Reset();
			//		player.Start();
			//		loops--;
			//	}
			//};
			//player.Prepared += (sender, args) => player.Start();

			//try {
			//	player.SetDataSource(songPath);
			//	player.PrepareAsync();
			//}
			//catch(Exception e) {
			//	Debug.WriteLine(e); player.Dispose(); return;
			//}
			//Debug.WriteLine("Playing sound: " + sound);
		}


		public void StopSound() {
			//if(player != null) {
			//	player.Stop();
			//	player.Dispose();
			//}
		}


		public void PlayShortSound(string newSound, int loops = 0) {
			//PlaySound(newSound, loops);

			//// Restart the player using the appropriate background sound in order to prevent app suspension.
			//player.Completion += (sender, e) => {
			//	if(isActive) {
			//		PlaySound(null);
			//	}
			//};
		}


		public void AdjustVolume(float volume) {
			//musicVolume = volume;
			//if(player != null) { player.SetVolume(volume, volume); }
		}


		public bool IsPlaying() {
			//return player?.IsPlaying ?? false;
			return isActive;
		}


		public void ActivateAudioSession() {
			//// Initialize Audio
			//var session = AVAudioSession.SharedInstance();

			//// If session type is still the default one, change it to 'Playback' mode, which allows background audio.
			//if(session.Category.ToString().Equals("AVAudioSessionCategorySoloAmbient"))
			//	session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers);
			////AVAudioSession.Notifications.ObserveInterruption((sender, e) => { Debug.WriteLine("Audio Interruption: " + e.Notification); });
			//session.SetActive(true);
			isActive = true;
		}


		public void DeactivateAudioSession() {
			//var session = AVAudioSession.SharedInstance();
			//session.SetActive(false);
			isActive = false;
		}


		private string getBackgroundSound() {
			switch(RewardEligibilityManager.Instance.GetCurrentState()) {
				case State.CyclingEligible:
					return User.Instance.BycicleEligibleSoundSetting;
				default:
					return User.Instance.BackgroundIneligibleSoundSetting;
			}
		}
	}
}
