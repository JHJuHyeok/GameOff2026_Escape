using TMPro;
using UnityEngine;

namespace SH.Project.UI.Checklist
{
    [DisallowMultipleComponent]
    public sealed class ChecklistRuleView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _numberText;
        [SerializeField] private TMP_Text _descriptionText;

        #region Public API

        // 전달받은 번호와 규칙 문장을 표시합니다.
        public void SetContent(int number, string description)
        {
            if (_numberText != null) _numberText.text = $"{number:D2}.";
            if (_descriptionText != null) _descriptionText.text = description ?? string.Empty;
        }

        #endregion
    }
}