using UnityEngine;

namespace UtilityTool
{
    /// <summary>
    /// Camera.main 캐싱 + 마우스/화면 좌표 관련 도우미
    /// Input System(ENABLE_INPUT_SYSTEM), 기존 Input 모두 자동 대응.
    /// </summary>
    public static class CameraUtil
    {
        private static Camera _main;

        /// <summary>캐싱된 메인 카메라 (씬 전환으로 파괴되면 자동 재탐색)</summary>
        public static Camera Main
        {
            get
            {
                if (_main == null) _main = Camera.main;
                return _main;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _main = null;

        // ---------------- 마우스 ----------------

        public static Vector2 MouseScreenPosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var mouse = UnityEngine.InputSystem.Mouse.current;
                return mouse != null ? mouse.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }

        /// <summary>2D 게임용: 마우스가 가리키는 월드 좌표 (z = 0 평면)</summary>
        public static Vector2 MouseWorld2D()
        {
            return ScreenToWorldOnPlane(MouseScreenPosition, new Plane(Vector3.forward, Vector3.zero));
        }

        /// <summary>3D 게임용: 마우스 레이가 높이 y의 수평면과 만나는 지점</summary>
        public static Vector3 MouseWorldOnGround(float groundY = 0f)
        {
            return ScreenToWorldOnPlane(MouseScreenPosition, new Plane(Vector3.up, new Vector3(0f, groundY, 0f)));
        }

        /// <summary>마우스 위치로 레이캐스트</summary>
        public static bool MouseRaycast(out RaycastHit hit, LayerMask mask, float maxDistance = 100f)
        {
            hit = default;
            if (Main == null) return false;
            Ray ray = Main.ScreenPointToRay(MouseScreenPosition);
            return Physics.Raycast(ray, out hit, maxDistance, mask);
        }

        private static Vector3 ScreenToWorldOnPlane(Vector2 screenPos, Plane plane)
        {
            if (Main == null) return Vector3.zero;
            Ray ray = Main.ScreenPointToRay(screenPos);
            return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
        }

        // ---------------- 화면 경계 ----------------

        /// <summary>Orthographic 카메라가 보고 있는 월드 영역 (2D 전용)</summary>
        public static Bounds GetOrthoBounds()
        {
            Camera cam = Main;
            if (cam == null) return new Bounds();
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            return new Bounds(cam.transform.position.WithZ(0f), new Vector3(width, height, 0f));
        }

        /// <summary>월드 좌표가 화면 안에 있는지. margin은 뷰포트 비율(0.1 = 10% 여유)</summary>
        public static bool IsVisible(Vector3 worldPos, float margin = 0f)
        {
            if (Main == null) return false;
            Vector3 v = Main.WorldToViewportPoint(worldPos);
            return v.z > 0f
                && v.x > -margin && v.x < 1f + margin
                && v.y > -margin && v.y < 1f + margin;
        }

        /// <summary>2D: 위치를 화면 경계 안으로 제한 (padding은 월드 단위)</summary>
        public static Vector2 ClampToScreen(Vector2 worldPos, float padding = 0f)
        {
            Bounds b = GetOrthoBounds();
            return new Vector2(
                Mathf.Clamp(worldPos.x, b.min.x + padding, b.max.x - padding),
                Mathf.Clamp(worldPos.y, b.min.y + padding, b.max.y - padding));
        }

        /// <summary>화면 바깥 가장자리의 랜덤 위치 (적 스폰용, 2D)</summary>
        public static Vector2 RandomOffscreenPosition(float margin = 1f)
        {
            Bounds b = GetOrthoBounds();
            int side = Random.Range(0, 4);
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            switch (side)
            {
                case 0: return new Vector2(x, b.max.y + margin); // 위
                case 1: return new Vector2(x, b.min.y - margin); // 아래
                case 2: return new Vector2(b.min.x - margin, y); // 왼쪽
                default: return new Vector2(b.max.x + margin, y); // 오른쪽
            }
        }

        public static Vector3 WorldToScreen(Vector3 worldPos)
            => Main != null ? Main.WorldToScreenPoint(worldPos) : Vector3.zero;
    }
}
