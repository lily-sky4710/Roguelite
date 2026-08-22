using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Core.MasterData;
using TPSRoguelite.InGame.Player;
using System;

namespace TPSRoguelite.InGame.Manager
{
    [Serializable]
    public class SkillButtonUI
    {
        public Button button;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dectText;
    }

    public class LevelUpManager : MonoBehaviour
    {
        public static LevelUpManager Instance {  get; private set; }

        void Start()
        {

        }

        public void OnLevelUp(PlayerInputActions currentInput, PlayerController player)
        {

        }

        private void OnSkillSelected(SkillDataRecord selectSkill)
        {

        }
    }
}