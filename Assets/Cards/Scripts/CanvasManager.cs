using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Cards
{
    public class CanvasManager : MonoBehaviour
    {
        public Button continueButton;

        public Button endTurn;

        [SerializeField]
        private GameManager _gameManager;

        public void Continue_EditorEvent()
        {
            _gameManager.CanvasManager_OnContinue();
            StartCoroutine(ScaleLerp(continueButton.transform, Vector3.zero, 0.3f, true));
        }

        public void EndTurn_EditorEvent()
        {
            _gameManager.ChangeTurn(true, false, false);
            endTurn.interactable = false;
        }

        public IEnumerator ScaleLerp(Transform obj, Vector3 target, float travelTime, bool isTurnOffOnEnd)
        {
            var time = 0f;
            var startScale = obj.localScale;
            while (time < 1f)
            {
                obj.localScale = Vector3.Lerp(startScale, target, time * time);
                time += Time.deltaTime / travelTime;
                yield return null;
            }
            obj.localScale = target;
            if (isTurnOffOnEnd) endTurn.interactable = true;
            continueButton.gameObject.SetActive(!isTurnOffOnEnd);
        }
    }
}
