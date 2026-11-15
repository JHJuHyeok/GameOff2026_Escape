using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultDetailNumberAnimation : MonoBehaviour
    {
        [SerializeField] private TMP_Text _valueText;
        [SerializeField, Min(0f)] private float _rollLeadTime = 0.16f;
        [SerializeField, Min(0f)] private float _digitInterval = 0.085f;
        [SerializeField, Min(0.01f)] private float _rollInterval = 0.045f;

        private readonly System.Random _random = new System.Random();

        #region Properties

        internal bool IsConfigured => _valueText != null; // 숫자를 표시할 TMP가 연결되었는지 여부

        #endregion

        #region Animation API

        // 구분 기호는 유지하고 숫자만 바꿉니다. 왼쪽부터 확정한 뒤 최종값을 표시합니다.
        internal IEnumerator Play(string value)
        {
            int digitCount = 0;
            foreach (char character in value) if (IsDigit(character)) digitCount++;

            if (digitCount > 0)
            {
                char[] displayed = value.ToCharArray();
                double startedAt = Time.unscaledTimeAsDouble;
                double nextRollAt = startedAt;
                int locked = 0;

                while (locked < digitCount)
                {
                    double now = Time.unscaledTimeAsDouble;
                    double lockAt = startedAt + _rollLeadTime + locked * _digitInterval;
                    if (now >= lockAt)
                    {
                        locked++;
                        nextRollAt = now;
                    }

                    if (now >= nextRollAt && locked < digitCount)
                    {
                        int digitIndex = 0;
                        for (int index = 0; index < displayed.Length; index++)
                        {
                            if (!IsDigit(value[index])) continue;
                            displayed[index] = digitIndex++ < locked ? value[index] : (char)('0' + _random.Next(10));
                        }
                        _valueText.SetCharArray(displayed, 0, displayed.Length);
                        nextRollAt = now + Math.Max(0.01f, _rollInterval);
                    }

                    if (locked < digitCount) yield return null;
                }
            }

            SetValue(value);
        }

        // 숫자 연출 없이 전달받은 문자열을 바로 표시합니다.
        internal void SetValue(string value)
        {
            _valueText.text = value;
        }

        #endregion

        #region Digit Check

        private static bool IsDigit(char character)
        {
            return character >= '0' && character <= '9';
        }

        #endregion
    }
}