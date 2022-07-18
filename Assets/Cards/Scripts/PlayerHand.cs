using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField]
        private Transform[] _positions;

        [HideInInspector]
        public List<Transform> _positionsInHand = new List<Transform>();

        public Transform[] startPositions;

        private List<Transform> _startPositionsChecker = new List<Transform>();

        [SerializeField]
        private PlayerNum playerNum;

        [SerializeField]
        private GameManager _gameManager;

        public void SetNewCard(Card newCard, bool isStart, bool isChanged)
        {
            bool b = playerNum == PlayerNum.PlayerOne;

            if (b) newCard.playerProperty = PlayerNum.PlayerOne;
            else newCard.playerProperty = PlayerNum.PlayerTwo;

            if (!isStart)
            {
                foreach (var pos in _positions)
                {
                    if (pos.childCount > 0) continue;

                    _positionsInHand.Add(pos);
                }

                if (_positionsInHand.Count != 0)
                {
                    StartCoroutine(MoveUpTop(newCard, _positionsInHand[0], isStart, false));
                    newCard.transform.SetParent(_positionsInHand[0]);

                    if (newCard.playerProperty == PlayerNum.PlayerOne) _gameManager._player1.cardsInHand.Add(newCard);
                    if (newCard.playerProperty == PlayerNum.PlayerTwo) _gameManager._player2.cardsInHand.Add(newCard);

                    _positionsInHand.Clear();
                }

                else Destroy(newCard.gameObject);
            }

            if (isStart)
            {
                foreach (var pos in startPositions)
                {
                    if (pos.childCount > 0) continue;

                    _startPositionsChecker.Add(pos);
                }

                StartCoroutine(MoveUpTop(newCard, _startPositionsChecker[0], isStart, true));

                if (isChanged) newCard.isChanged = true;

                newCard.transform.SetParent(_startPositionsChecker[0]);
                _startPositionsChecker.Clear();
            }
        }

        private IEnumerator MoveUpTop(Card card, Transform parent, bool isStart, bool needUpTop)
        {
            if (needUpTop)
            {
                var time = 0f;
                var startPos = card.transform.position;
                var startRot = card.transform.localEulerAngles;
                var endPos = isStart ? parent.position : new Vector3(0f, 150f, -30f);
                var endRot = new Vector3(-180f, -180f, 0f);

                while (time < 1f)
                {
                    card.transform.position = Vector3.Lerp(startPos, endPos, time);
                    card.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, time);
                    time += Time.deltaTime;
                    yield return null;
                }
                card.transform.position = endPos;
                card.transform.localEulerAngles = endRot;

                if (!isStart)
                {
                    yield return new WaitForSeconds(.3f);
                    yield return StartCoroutine(MoveInHand(card, parent, false));
                }

                if (isStart)
                {
                    card.SwitchEnable();
                    card.State = CardStateType.OnStart;
                    card.isInterectable = true;
                }
            }

            else
            {
                yield return StartCoroutine(MoveInHand(card, parent, true));
            }
        }

        private IEnumerator MoveInHand(Card card, Transform parent, bool isRotate)
        {
            var time = 0f;
            var startPos = card.transform.position;
            var endPos = parent.transform.position;
            var startRot = card.transform.localEulerAngles;
            var endRot = new Vector3(-180f, -180f, 0f);

            while (time < 1f)
            {
                if (isRotate) card.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, time);
                card.transform.position = Vector3.Lerp(startPos, endPos, time);
                time += Time.deltaTime;
                yield return null;
            }
            if (isRotate) card.transform.localEulerAngles = endRot;
            card.transform.position = endPos;
            card.posInHand = card.transform.position;
            card.State = CardStateType.InHand;
        }
    }
}
