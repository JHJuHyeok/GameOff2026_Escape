using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SoundPlayer;

namespace MainSystem
{
    public enum VolumeChannel { Master = 0, Bgm = 1, Sfx = 2 }

    /// <summary>
    /// BGM(크로스페이드) + SFX(풀링) + 볼륨 저장.
    /// AudioMixer는 선택 사항:
    ///  - 할당하면: 노출된 파라미터(MasterVolume / BGMVolume / SFXVolume)에 dB로 반영
    ///  - 비워 두면: AudioListener.volume과 AudioSource.volume으로 대체 동작
    ///
    /// 사용 예:
    ///   AudioManager.Instance.PlayBGM(bgmClip, 1f, SoundId.BgmMenu);
    ///   AudioManager.Instance.PlaySFX(hitClip, 1f, 0.1f, SoundId.Hit);
    ///   AudioManager.Instance.PlaySFX(boomClip, position: explosionPos);      // 3D 위치 기반 재생
    ///   AudioManager.Instance.StopSfx(SoundId.Hit);
    ///   AudioManager.Instance.SetVolume(VolumeChannel.BGM, slider.value);
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Mixer (선택)")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        private const string MasterParam = "MasterVolume";
        private const string BgmParam = "BGMVolume";
        private const string SfxParam = "SFXVolume";

        [Header("SFX")]
        [Tooltip("동시 재생 가능한 SFX 최대 개수. 초과 시 가장 오래된 소리 끊고 재사용")]
        [SerializeField, Min(1)] private int maxSfxVoices = 24;
        [Tooltip("같은 SoundId의 동시 재생 상한")]
        [SerializeField, Min(0)] private int maxSamePerId = 4;
        [SerializeField, Min(0)] private int sfxPrewarm = 8;

        [Header("저장된 값이 없을 때의 기본 볼륨")]
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;

        private BgmAudioPlayer _bgm;
        private SfxAudioPlayer _sfxTemplate;
        private Transform _sfxRoot;
        private readonly List<SfxAudioPlayer> _activeSfx = new();
        private readonly float[] _volumes = new float[3];

        private bool UseMixer => mixer != null;
        private float BgmTargetVolume => UseMixer ? 1f : _volumes[(int)VolumeChannel.Bgm];
        private float SfxScale => UseMixer ? 1f : _volumes[(int)VolumeChannel.Sfx];

        public SoundId CurrentBgmId => _bgm != null ? _bgm.CurrentBgmId : SoundId.None;
        public AudioClip CurrentBgmClip => _bgm != null ? _bgm.CurrentClip : null;

        protected override void Init()
        {
            for (int i = 0; i < _volumes.Length; i++)
                _volumes[i] = PlayerPrefs.GetFloat(PrefKey((VolumeChannel)i), defaultVolume);

            // BGM 플레이어
            GameObject bgmObj = new GameObject("BGM");
            bgmObj.transform.SetParent(transform, false);
            _bgm = bgmObj.AddComponent<BgmAudioPlayer>();

            // SFX 풀 루트
            GameObject sfxRootObj = new GameObject("SFX");
            sfxRootObj.transform.SetParent(transform, false);
            _sfxRoot = sfxRootObj.transform;

            // SFX 템플릿: 비활성 상태로 만들어 두면 PoolManager가 복제하는 인스턴스도 비활성화
            GameObject tpl = new GameObject("SfxAudioPlayer (Template)");
            tpl.SetActive(false);
            tpl.transform.SetParent(transform, false);
            AudioSource src = tpl.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _sfxTemplate = tpl.AddComponent<SfxAudioPlayer>();

            if (sfxPrewarm > 0 && PoolManager.Instance != null)
                PoolManager.Instance.Prewarm(tpl, sfxPrewarm);
        }

        // AudioMixer.SetFloat는 Awake 시점에 무시되는 경우가 있어 Start에서 적용
        private void Start() => ApplyAllVolumes();

        // 종료 시각이 지난 SFX 풀로 반환
        private void Update()
        {
            if (_activeSfx.Count == 0) return;

            for (int i = _activeSfx.Count - 1; i >= 0; i--)
            {
                SfxAudioPlayer p = _activeSfx[i];
                if (p == null) { _activeSfx.RemoveAt(i); continue; }
                if (p.IsFinished) ReleaseSfx(p);
            }
        }

        // ---------------- BGM ----------------

        public void PlayBgm(AudioClip clip, float fadeDuration = 1f, SoundId id = SoundId.None, bool restart = false)
            => _bgm.Play(id, clip, bgmGroup, BgmTargetVolume, fadeDuration, restart);

        public void StopBgm(float fadeDuration = 1f) => _bgm.StopBgm(fadeDuration);

        // ---------------- SFX ----------------

        /// <param name="pitchVariation">0.1이면 피치 0.9 ~ 1.1 랜덤</param>
        /// <param name="id">지정하면 StopSfx(id)와 같은 ID 동시 재생 제한이 적용됨</param>
        /// <param name="position">지정하면 해당 위치에서 3D로 재생, 생략하면 2D</param>
        /// <returns>재생에 사용된 플레이어 (실패 시 null). 보관하지 말고 즉시 쓸 때만 사용</returns>
        public SfxAudioPlayer PlaySfx(AudioClip clip, float volume = 1f, float pitchVariation = 0f,
                                      SoundId id = SoundId.None, float delay = 0f, Vector3? position = null)
        {
            PoolManager pool = PoolManager.Instance;
            if (clip == null || pool == null) return null;

            // 1) 같은 ID 동시 재생 상한: 넘으면 같은 ID 중 가장 오래된 것 종료
            if (id != SoundId.None && maxSamePerId > 0)
            {
                int count = 0;
                SfxAudioPlayer oldest = null;
                foreach (SfxAudioPlayer p in _activeSfx)
                {
                    if (p == null || p.CurrentSoundId != id) continue;
                    if (oldest == null) oldest = p;
                    count++;
                }
                if (count >= maxSamePerId) ReleaseSfx(oldest);
            }

            // 2) 전체 상한: 넘으면 가장 오래된 소리부터 회수
            while (_activeSfx.Count >= maxSfxVoices)
                ReleaseSfx(_activeSfx[0]);

            Vector3 pos = position ?? Vector3.zero;
            SfxAudioPlayer player = pool.Spawn(_sfxTemplate, pos, Quaternion.identity, _sfxRoot);
            if (player == null) return null;

            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            player.Play(id, clip, sfxGroup, volume * SfxScale, pitch, delay, position.HasValue ? 1f : 0f);
            _activeSfx.Add(player);
            return player;
        }

        public SfxAudioPlayer PlaySFXRandom(AudioClip[] clips, float volume = 1f, float pitchVariation = 0f,
                                            SoundId id = SoundId.None)
        {
            if (clips == null || clips.Length == 0) return null;
            return PlaySfx(clips[Random.Range(0, clips.Length)], volume, pitchVariation, id);
        }

        /// <summary>해당 ID의 SFX를 모두 중지</summary>
        public void StopSfx(SoundId id)
        {
            for (int i = _activeSfx.Count - 1; i >= 0; i--)
            {
                SfxAudioPlayer p = _activeSfx[i];
                if (p != null && p.CurrentSoundId == id) ReleaseSfx(p);
            }
        }

        public void StopAllSfx()
        {
            for (int i = _activeSfx.Count - 1; i >= 0; i--)
                ReleaseSfx(_activeSfx[i]);
        }

        private void ReleaseSfx(SfxAudioPlayer player)
        {
            _activeSfx.Remove(player);

            PoolManager pool = PoolManager.Instance; // 종료 중이면 null
            if (pool != null && player != null)
                pool.Despawn(player.gameObject);
        }

        // ---------------- 볼륨 ----------------

        public float GetVolume(VolumeChannel channel) => _volumes[(int)channel];

        /// <param name="linear">0 ~ 1 (슬라이더 값 그대로)</param>
        public void SetVolume(VolumeChannel channel, float linear)
        {
            linear = Mathf.Clamp01(linear);
            _volumes[(int)channel] = linear;
            PlayerPrefs.SetFloat(PrefKey(channel), linear);
            ApplyVolume(channel);
        }

        /// <summary>슬라이더를 놓았을 때(onPointerUp 등) 호출. 드래그 중 매번 호출할 필요 없음</summary>
        public void SaveVolumes() => PlayerPrefs.Save();

        private void ApplyAllVolumes()
        {
            for (int i = 0; i < _volumes.Length; i++) ApplyVolume((VolumeChannel)i);
        }

        private void ApplyVolume(VolumeChannel channel)
        {
            float v = _volumes[(int)channel];

            if (UseMixer)
            {
                string param = ParamName(channel);
                if (!mixer.SetFloat(param, LinearToDb(v)))
                    Debug.LogWarning($"[AudioManager] AudioMixer에 노출된 파라미터가 없습니다: {param}");
            }
            else
            {
                if (channel == VolumeChannel.Master) AudioListener.volume = v;
                else if (channel == VolumeChannel.Bgm) _bgm.SetTargetVolume(v);
            }
        }

        private string ParamName(VolumeChannel channel) => channel switch
        {
            VolumeChannel.Master => MasterParam,
            VolumeChannel.Bgm => BgmParam,
            _ => SfxParam
        };

        private static string PrefKey(VolumeChannel channel) => $"Volume_{channel}";

        private static float LinearToDb(float linear) => Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
    }
}
