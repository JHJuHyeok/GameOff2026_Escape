using System;
using System.IO;
using UnityEngine;

namespace MainSystem
{
    /// <summary>
    /// 저장/불러오기.
    ///  1) PlayerPrefs 래퍼: 설정값, 하이스코어 등 단순 값
    ///  2) JSON 저장: [Serializable] 클래스 통째로 저장 (진행도 등)
    ///     - PC/에디터: persistentDataPath/{slot}.json 파일
    ///     - WebGL 빌드: 파일 대신 PlayerPrefs에 저장 (브라우저 저장소가 더 안정적)
    ///
    /// 사용 예:
    ///   [Serializable] public class SaveData { public int stage; public int coins; }
    ///   SaveManager.Instance.Save("progress", new SaveData { stage = 3 });
    ///   SaveData data = SaveManager.Instance.Load("progress", new SaveData());
    ///   SaveManager.Instance.TrySetHighScore(score);
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        private const string DefaultHighScoreKey = "HighScore";

        // ---------------- PlayerPrefs ----------------

        public int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public void SavePrefs() => PlayerPrefs.Save();

        // ---------------- 하이스코어 ----------------

        public int GetHighScore(string key = DefaultHighScoreKey) => GetInt(key, 0);

        /// <summary>기록을 경신했으면 저장하고 true 반환</summary>
        public bool TrySetHighScore(int score, string key = DefaultHighScoreKey)
        {
            if (score <= GetInt(key, 0)) return false;
            SetInt(key, score);
            PlayerPrefs.Save();
            return true;
        }

        // ---------------- JSON 저장 ----------------

        public bool Save<T>(string slot, T data)
        {
            try
            {
                WriteRaw(slot, JsonUtility.ToJson(data, true));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 저장 실패 ({slot}): {e.Message}");
                return false;
            }
        }

        public bool TryLoad<T>(string slot, out T data)
        {
            data = default;
            try
            {
                if (!ReadRaw(slot, out string json)) return false;
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 불러오기 실패 ({slot}): {e.Message}");
                return false;
            }
        }

        /// <summary>저장 데이터가 없거나 실패하면 defaultValue 반환</summary>
        public T Load<T>(string slot, T defaultValue = default)
            => TryLoad(slot, out T data) ? data : defaultValue;

        public bool Exists(string slot) => ExistsRaw(slot);
        public void Delete(string slot) => DeleteRaw(slot);

        // ---------------- 저장소 구현 ----------------
#if UNITY_WEBGL && !UNITY_EDITOR
        private static string WebKey(string slot) => "save_" + slot;

        private static void WriteRaw(string slot, string json)
        {
            PlayerPrefs.SetString(WebKey(slot), json);
            PlayerPrefs.Save();
        }

        private static bool ReadRaw(string slot, out string json)
        {
            json = null;
            if (!PlayerPrefs.HasKey(WebKey(slot))) return false;
            json = PlayerPrefs.GetString(WebKey(slot));
            return true;
        }

        private static bool ExistsRaw(string slot) => PlayerPrefs.HasKey(WebKey(slot));

        private static void DeleteRaw(string slot)
        {
            PlayerPrefs.DeleteKey(WebKey(slot));
            PlayerPrefs.Save();
        }
#else
        private static string PathOf(string slot) => Path.Combine(Application.persistentDataPath, slot + ".json");

        private static void WriteRaw(string slot, string json) => File.WriteAllText(PathOf(slot), json);

        private static bool ReadRaw(string slot, out string json)
        {
            json = null;
            string path = PathOf(slot);
            if (!File.Exists(path)) return false;
            json = File.ReadAllText(path);
            return true;
        }

        private static bool ExistsRaw(string slot) => File.Exists(PathOf(slot));

        private static void DeleteRaw(string slot)
        {
            string path = PathOf(slot);
            if (File.Exists(path)) File.Delete(path);
        }
#endif
    }
}
