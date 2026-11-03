using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour 싱글톤 베이스
/// 
/// 사용법 :
///     public class AnyManager : Singleton<AnyManager>
///     {
///         protected override void Init() { ... }      // Awake 대신 Init을 override
///     }
/// 
/// - Persistent == true(기본): 씬이 바뀌어도 유지. 중복 인스턴스는 자동 파괴.
///   -> 모든 씬에 Managers 프리팹을 넣어 두어도 하나만 살아남음. (어느 씬에서 시작해도 테스트 가능)
/// - Persistent == false: 씬 전용. 새 씬의 인스턴스가 이전 인스턴스 대체.
/// - DontDestroyOnLoad 루트 오브젝트 단위로 적용되므로,
///   매니저들은 [Managers] 루트 아래 "자식 오브젝트 하나당 매니저 하나"로 두는 것 권장.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;
    protected static bool _isQuitting;

    /// <summary>Instance 호출하지 않고 존재 여부만 확인 (해제/구독 취소 시 사용)</summary>
    public static bool HasInstance => _instance != null;

    public static T Instance
    {
        get
        {
            if (_isQuitting || !Application.isPlaying)
                return null;

            if (_instance == null)
            {
                _instance = (T)FindAnyObjectByType(typeof(T));

                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name, typeof(T));
                    _instance = obj.GetComponent<T>();
                }
            }

            return _instance;
        }
    }

    /// <summary>true면 씬 전환 시 유지, false면 씬 전용</summary>
    protected virtual bool Persistent => true;

    protected virtual void Awake()
    {
        T self = this as T;

        if (Persistent)
        {
            if (_instance != null && _instance != self)
            {
                // 중복 인스턴스: Update/Start 등 라이프 사이클이 돌지 않도록 비활성화 이후 파괴
                enabled = false;
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(transform.root.gameObject);
        }

        _instance = self;
        Init();
    }

    /// <summary>하위 클래스의 초기화. Awake 대신 이것을 override (중복 인스턴스에선 호출 불가)</summary>
    protected virtual void Init() { }

    protected virtual void OnDestroy()
    {
        T self = this as T;
        if (_instance == self)
            _instance = null;
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}