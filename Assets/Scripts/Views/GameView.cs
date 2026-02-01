using Generator.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Generator.UI
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] private Button _generateMapButton;
        [SerializeField] private Button _generateSeedButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TMP_InputField _seedInput;

        //NOTE(): Keeping in same file to not create too many files for review
        [SerializeField] private CanvasGroup _winScreen;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _restartCloseButton;

        public event Action OnGenerateMapClickedEvent;

        private SeedController _seedController;


        public void Init(SeedController seedController)
        {
            _seedController = seedController;
            _generateMapButton.onClick.AddListener(OnGenerateMapClicked);
            _generateSeedButton.onClick.AddListener(OnGenerateRandomSeedClicked);
            _restartButton.onClick.AddListener(OnRestartButton);
            _restartCloseButton.onClick.AddListener(OnCloseButton);
            _exitButton.onClick.AddListener(CloseGame);
            _seedInput.text = _seedController.Seed.ToString();
        }

        private void OnDestroy()
        {
            _generateMapButton.onClick.RemoveListener(OnGenerateMapClicked);
            _generateSeedButton.onClick.RemoveListener(OnGenerateRandomSeedClicked);
            _restartButton.onClick.RemoveListener(OnRestartButton);
            _restartCloseButton.onClick.RemoveListener(OnCloseButton);
            _exitButton.onClick.RemoveListener(CloseGame);
        }


        private void OnGenerateMapClicked()
        {
            OnGenerateMapClickedEvent?.Invoke();
        }

        private void OnGenerateRandomSeedClicked()
        {
            _seedController.SetRandomSeed();
            _seedInput.text = _seedController.Seed.ToString();
        }

        private void CloseGame()
        {
            Application.Quit();
        }

        #region WIN SCREEN Logic

        public void ShowWinScreen()
        {
            _winScreen.alpha = 1.0f;
            _winScreen.interactable = true;
            _winScreen.blocksRaycasts = true;
        }

        private void OnRestartButton()
        {
            OnGenerateRandomSeedClicked();
            OnGenerateMapClicked();
            HideWinScreen();
        }

        private void OnCloseButton()
        {
            HideWinScreen();
        }

        private void HideWinScreen()
        {
            _winScreen.alpha = 0.0f;
            _winScreen.interactable = false;
            _winScreen.blocksRaycasts = false;
        }    
        #endregion


    }
}
