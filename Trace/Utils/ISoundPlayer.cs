namespace Trace {

	/// <summary>
	/// This class represents the media player that plays sounds for the user in the background while
	/// the user is cycling.
	/// </summary>
	public interface ISoundPlayer {

		void ActivateAudioSession();
		void DeactivateAudioSession();
		void PlaySound(string sound, int loops = -1);
		void StopSound();
		void PlayShortSound(string newSound, int loops = 0);
		void AdjustVolume(float volume); // 0 to 1
		bool IsPlaying();
	}
}