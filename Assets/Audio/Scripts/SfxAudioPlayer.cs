using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using UtilityTool;
using MainSystem;

namespace SoundPlayer
{
    /// <summary>
    /// 풀링되는 SFX 재생용 컴포넌트. 스스로 반환하지 않고, 종료 시각만 알림.
    /// 반환/상한/회수는 AudioManager에서 담당.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxAudioPlayer : MonoBehaviour, IPoolable
    {
        private AudioSource _audioSource;

        private AudioSource Source
        {
            get
            {
                if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        public SoundId CurrentSoundId { get; private set; } = SoundId.None;

        // 재생 종료 예정 시각
        public double EndDspTime { get; private set; }
        public bool IsFinished => AudioSettings.dspTime >= EndDspTime;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        /// <summary>오디오 플레이어에 정보 주입 후 플레이</summary>
        public void Play(SoundId id, AudioClip clip, AudioMixerGroup mixerGroup, 
            float volume, float pitch = 1f, float delay = 0f, float spatialBlend = 0f)
        {
            AudioSource s = Source;
            s.Stop();

            CurrentSoundId = id;
            s.clip = clip;
            s.outputAudioMixerGroup = mixerGroup;
            s.volume = volume;
            s.pitch = Mathf.Max(0.01f, pitch);
            s.spatialBlend = spatialBlend;
            s.loop = false;

            double start = AudioSettings.dspTime;
            if (delay > 0f)
            {
                start += delay;
                s.PlayScheduled(start);
            }
            else
            {
                s.Play();
            }

            // 피치가 높으면 더 빨리 끝나므로 살짝 여유 포함
            EndDspTime = start + clip.length / s.pitch + 0.05;
        }

        public void Stop() => Source.Stop();

        // ---- IPoolable ----
        public void OnSpawn() { }

        public void OnDespawn()
        {
            Source.Stop();
            Source.clip = null;
            CurrentSoundId = SoundId.None;
            EndDspTime = 0d;
        }
    }
}