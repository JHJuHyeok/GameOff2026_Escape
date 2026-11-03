using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace SoundPlayer
{
    public class BgmAudioPlayer : MonoBehaviour
    {
        private AudioSource _sourceA, _sourceB, _activeSource;

        private Coroutine _fadeCoroutine;
        private float _targetVolume = 1f;

        public SoundId CurrentBgmId { get; private set; } = SoundId.None;
        public AudioClip CurrentClip => _activeSource != null ? _activeSource.clip : null;
        public bool IsPlaying => _activeSource != null && _activeSource.isPlaying;

        private void Awake() => EnsureSources();

        private void EnsureSources()
        {
            if (_sourceA != null) return;
            _sourceA = CreateSource();
            _sourceB = CreateSource();
        }

        private AudioSource CreateSource()
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.loop = true;
            s.playOnAwake = false;
            return s;
        }

        /// <summary>오디오 플레이어에 정보 주입 후 플레이</summary>
        public void Play(SoundId id, AudioClip clip, AudioMixerGroup mixerGroup, 
            float targetVolume, float fadeDuration = 1.0f, bool restart = false)
        {
            if (clip == null) return;
            EnsureSources();

            bool sameTrack = id != SoundId.None ? CurrentBgmId == id : CurrentClip == clip;
            if (!restart && sameTrack && IsPlaying) return;

            CurrentBgmId = id;
            _targetVolume = targetVolume;

            // 교체할 새 Source 지정
            AudioSource from = _activeSource;
            AudioSource to = (from == _sourceA) ? _sourceB : _sourceA;
            _activeSource = to;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            to.Stop();
            to.clip = clip;
            to.outputAudioMixerGroup = mixerGroup;
            to.volume = 0f;
            to.Play();

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(CrossfadeRoutine(from, to, targetVolume, fadeDuration));
        }

        public void StopBgm(float fadeDuration = 1.0f)
        {
            if (_sourceA == null) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutAllRoutine(fadeDuration));
            CurrentBgmId = SoundId.None;
        }

        /// <summary>믹서를 쓰지 않을 때 볼륨 슬라이더 값 반영</summary>
        public void SetTargetVolume(float volume)
        {
            _targetVolume = volume;
            if (_fadeCoroutine == null && _activeSource != null)
                _activeSource.volume = volume;
        }

        private IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, float targetVolume, float duration)
        {
            float fromStart = from != null ? from.volume : 0f;

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = t / duration;
                to.volume = Mathf.Lerp(0f, targetVolume, k);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
                yield return null;
            }

            to.volume = _targetVolume;
            if (from != null)
            {
                from.Stop();
                from.clip = null;
                from.volume = 0f;
            }
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutAllRoutine(float duration)
        {
            float startA = _sourceA.volume;
            float startB = _sourceB.volume;

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = t / duration;
                _sourceA.volume = Mathf.Lerp(startA, 0f, k);
                _sourceB.volume = Mathf.Lerp(startB, 0f, k);
                yield return null;
            }

            foreach (AudioSource s in new[] { _sourceA, _sourceB })
            {
                s.Stop();
                s.clip = null;
                s.volume = 0f;
            }
            _activeSource = null;
            _fadeCoroutine = null;
        }
    }
}