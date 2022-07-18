using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cards
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField]
        private GameManager _gameManager;

        [SerializeField]
        private Button _startButton;

        [SerializeField]
        private Button _resumeButton;

        [SerializeField]
        private Button _settingsButton;

        [SerializeField]
        private Button _exitButton;

        [SerializeField]
        private GameObject _preventingClick;

        [SerializeField]
        private Material _pressedMaterial;

        [SerializeField]
        private Material _standardMaterial;

        [SerializeField]
        private Button _turnGameButton;

        [SerializeField]
        private TextMeshPro _player1Hp;

        [SerializeField]
        private TextMeshPro _player2Hp;

        [SerializeField]
        private TextMeshPro _player1Mana;

        [SerializeField]
        private TextMeshPro _player2Mana;

        private void Start()
        {
            _resumeButton.gameObject.SetActive(false);

            _preventingClick.SetActive(true);
            _player1Hp.gameObject.SetActive(false);
            _player2Hp.gameObject.SetActive(false);
            _player1Mana.gameObject.SetActive(false);
            _player2Mana.gameObject.SetActive(false);
            _turnGameButton.gameObject.SetActive(false);
            StartCoroutine(Lerp(transform, Vector3.one, 0.5f, false));
        }
        public void Start_EditorEvent()
        {
            _gameManager.StarGame_EditorEvent();
            StartCoroutine(MaterialLerp(_startButton));
            StartCoroutine(Lerp(transform, Vector3.zero, 0.3f, true));
            _preventingClick.SetActive(false);
            _player1Hp.gameObject.SetActive(true);
            _player2Hp.gameObject.SetActive(true);
            _player1Mana.gameObject.SetActive(true);
            _player2Mana.gameObject.SetActive(true);
            _turnGameButton.gameObject.SetActive(true);
        }
        public void Resume_EditorEvent()
        {
            StartCoroutine(MaterialLerp(_resumeButton));
        }
        public void Settings_EditorEvent()
        {
            StartCoroutine(MaterialLerp(_settingsButton));
        }
        public void Exit_EditorEvent()
        {
            StartCoroutine(MaterialLerp(_exitButton));
            Application.Quit();
        }
        private IEnumerator Lerp(Transform obj, Vector3 target, float TravelTime, bool turnOfStart)
        {
            //_gameManager.input 

            _startButton.interactable = false;
            _resumeButton.interactable = false; 
            _settingsButton.interactable = false;
            _exitButton.interactable = false;


            Vector3 startPosition = obj.localScale;

            float t = 0;

            while (t < 1)
            {
                obj.localScale = Vector3.Lerp(startPosition, target, t * t);

                t += Time.unscaledDeltaTime / TravelTime;

                yield return null;
            }

            obj.localScale = target;

            _startButton.interactable = true;
            _resumeButton.interactable = true;
            _settingsButton.interactable = true;
            _exitButton.interactable = true;

            yield return new WaitForSeconds(1f);
            if (turnOfStart)
            {
                _startButton.gameObject.SetActive(false);
                _resumeButton.gameObject.SetActive(true);
            }
        }

        private IEnumerator MaterialLerp(Button butt)
        {
            butt.interactable = false;
            butt.image.material = _pressedMaterial;
            yield return new WaitForSeconds(0.1f);
            butt.image.material = _standardMaterial;
            butt.interactable = true;
        }
    }
}
