using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Core.MasterData;
using TPSRoguelite.InGame.Player;
using System;
using Unity.VisualScripting;

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

        [Header("UIê›íË")]
        [SerializeField] private GameObject skillSelectPanel;
        [SerializeField] private SkillButtonUI[] skillButtons = new SkillButtonUI[3];

        private PlayerInputActions inputActions;
        private PlayerController playerController;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            Time.timeScale = 1.0f;

            if(skillSelectPanel != null)
            {
                skillSelectPanel.SetActive(false);
            }
        }

        public void OnLevelUp(PlayerInputActions currentInput, PlayerController player)
        {
            inputActions = currentInput;
            playerController = player;

            var allSkills = MasterDataAccessor.Instance.GetAll<SkillDataRecord>();
            var chosenSkills = allSkills.OrderBy(v => System.Guid.NewGuid()).Take(3).ToList();

            for(int i = 0; i < 3; i++)
            {
                var skill = chosenSkills[i];
                var ui = skillButtons[i];

                ui.nameText.text = skill.SkillName;
                ui.dectText.text = skill.Description;

                ui.button.onClick.RemoveAllListeners();
                ui.button.onClick.AddListener(() => OnSkillSelected(skill));
            }

            if(skillSelectPanel != null)
            {
                skillSelectPanel.SetActive(true);
            }

            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if(inputActions != null)
            {
                inputActions.Player.Disable();
            }
        }

        private void OnSkillSelected(SkillDataRecord selectSkill)
        {
            if(playerController != null)
            {
                playerController.ApplySkill(selectSkill);
            }

            if(skillSelectPanel != null)
            {
                skillSelectPanel.SetActive(false);
            }

            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if(inputActions != null)
            {
                inputActions.Player.Enable();
            }
        }
    }
}