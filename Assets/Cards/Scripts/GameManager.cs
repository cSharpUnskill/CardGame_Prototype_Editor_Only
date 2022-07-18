using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cards.ScriptableObjects;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;

namespace Cards
{
    public class GameManager : MonoBehaviour
    {
        private Material _baseMat;

        private List<CardPropertiesData> _allCards;

        [SerializeField]
        private Card[] _player1Deck;

        private Card[] _player2Deck;

        [SerializeField]
        private Card _cardPrefab;

        [SerializeField]
        private CardPackConfiguration[] _packs;

        [SerializeField, Space, Range(5f, 100f)]
        private int _cardDeckCount = 30;

        [SerializeField]
        private PlayerHand _playerHand1;

        [SerializeField]
        private PlayerHand _playerHand2;

        public PlayerDesk _player1Desk;

        public PlayerDesk _player2Desk;

        [SerializeField]
        private Transform _player1DeckParent;

        [SerializeField]
        private Transform _player2DeckParent;

        public Player _currentTurnPlayer;

        private bool _mouseEnter;

        private bool _mouseExit;

        public Card _selectedCard;

        private RaycastHit _hit;

        [HideInInspector]
        public Transform[] _currentPlayerDeskPositions;

        private Transform _target;

        private bool _stop = false;

        private bool _stop2 = false;

        [SerializeField]
        private Collider _player1DeskCollider;

        [SerializeField]
        private Collider _player2DeskCollider;

        [SerializeField]
        private GameObject _startTable;

        private List<Card> _cardsPlayer1 = new List<Card>();

        private List<Card> _cardsPlayer2 = new List<Card>();

        [SerializeField]
        private CanvasManager _canvasManager;

        [SerializeField]
        private Button _turnButton;

        [SerializeField]
        private Button _continueButton;

        [SerializeField]
        private Transform _player1hp;

        [SerializeField]
        private Transform _player2hp;

        [SerializeField]
        private Transform _player1mana;

        [SerializeField]
        private Transform _player2mana;

        [SerializeField]
        private Transform _player1portrait;

        [SerializeField]
        private Transform _player2portrait;

        [SerializeField]
        public Player _player1;

        [SerializeField]
        public Player _player2;

        private Coroutine _cor;

        [SerializeField]
        private Transform cam;

        [SerializeField]
        private Vector3 camRotPlayer1;

        [SerializeField]
        private Vector3 camRotPlayer2;

        void Awake()
        {
            IEnumerable<CardPropertiesData> array = new List<CardPropertiesData>();

            foreach (var packs in _packs) array = packs.UnionProperties(array);

            _allCards = new List<CardPropertiesData>(array);

            _baseMat = new Material(Shader.Find("TextMeshPro/Sprite"))
            {
                renderQueue = 2996
            };
        }

        void Start()
        {
            _turnButton.interactable = false;
            _currentTurnPlayer = _player1;

            _player1Deck = CreateDeck(_player1DeckParent);
            _player2Deck = CreateDeck(_player2DeckParent);
            
            foreach (var card in _player1Deck)
            {
                _cardsPlayer1.Add(card);
            }

            foreach (var card in _player2Deck)
            {
                _cardsPlayer2.Add(card);
            }
        }

        void Update()
        {
            CardInteraction();

            var toKill = new List<Card>();

            foreach (var card in _player1.cardsOnTable)
            {
                if (card.Health <= 0 || card.Health >= 11)
                {
                    toKill.Add(card);
                }
            }

            foreach (var card in _player2.cardsOnTable)
            {
                if (card.Health <= 0 || card.Health >= 11)
                {
                    toKill.Add(card);
                }
            }

            foreach (var card in toKill)
            {
                if (card.playerProperty == PlayerNum.PlayerOne) _player1.cardsOnTable.Remove(card);
                if (card.playerProperty == PlayerNum.PlayerTwo) _player2.cardsOnTable.Remove(card);
                Destroy(card.gameObject); 
            }

            if (_player1.health <= 0)
            {
#if UNITY_EDITOR

                print("Game Over player 2 Wins");
                EditorApplication.isPlaying = false;
#endif
            }

            if (_player2.health <= 0)
            {
#if UNITY_EDITOR
                print("Game Over player 1 Wins");
                EditorApplication.isPlaying = false;
#endif
            }
        }

        private Card[] CreateDeck(Transform parent)
        {
            var deck = new Card[_cardDeckCount];

            var offset = Vector3.zero;

            for (int i = 0; i < _cardDeckCount; i++)
            {
                deck[i] = Instantiate(_cardPrefab, parent);

                deck[i].transform.localPosition = offset;

                deck[i].posInDeck = deck[i].GetComponentInParent<Transform>().position;

                offset += new Vector3(0f, 1f, 0f);

                deck[i].SwitchEnable();

                var random = _allCards[Random.Range(0, _allCards.Count)];

                var newMat = new Material(_baseMat)
                {
                    mainTexture = random.Texture
                };

                deck[i].Configuration(random, newMat, CardUtility.GetDescriptionById(random.Id));
            }

            return deck;
        }

        public void StarGame_EditorEvent()
        {
            StartCoroutine(StartGameCor(true));
        }

        void AddCard(PlayerHand player, Card newCard, bool isStart, bool isChanged)
        {
            player.SetNewCard(newCard, isStart, isChanged);
        }

        private void StartGame(bool isPlayer1)
        {
            if (!_stop && isPlayer1)
            {
                _stop = true;

                StartCoroutine(ColorLerp(_startTable.GetComponent<MeshRenderer>(), new Color32(0, 0, 0, 228), 1f, true));

                for (int i = 0; i < 3; i++)
                {
                    AddCard(_playerHand1, _cardsPlayer1[i], true, false);
                    _cardsPlayer1.Remove(_cardsPlayer1[i]);
                }

                StartCoroutine(_canvasManager.ScaleLerp(_canvasManager.continueButton.transform, Vector3.one, 0.3f, false));
            }

            if (!_stop && !isPlayer1)
            {
                _stop = true;

                StartCoroutine(ColorLerp(_startTable.GetComponent<MeshRenderer>(), new Color32(0, 0, 0, 228), 1f, true));

                for (int i = 0; i < 3; i++)
                {
                    AddCard(_playerHand2, _cardsPlayer2[i], true, false);
                    _cardsPlayer2.Remove(_cardsPlayer2[i]);
                }

                StartCoroutine(_canvasManager.ScaleLerp(_canvasManager.continueButton.transform, Vector3.one, 0.3f, false));
            }
        }

        private void ContinueGame(bool isPlayer1)
        {
            if (isPlayer1)
            {
                foreach (var c in _playerHand1.startPositions)
                {
                    if (c.childCount == 0) return;
                    var r = c.GetChild(0).GetComponent<Card>();
                    AddCard(_playerHand1, r, false, r.isChanged);
                }
                StartCoroutine(ColorLerp(_startTable.GetComponent<MeshRenderer>(), new Color32(0, 0, 0, 0), 1f, true));
                _player1.AddMana(true);
            }

            if (!isPlayer1)
            {
                foreach (var c in _playerHand2.startPositions)
                {
                    if (c.childCount == 0) return;
                    var r = c.GetChild(0).GetComponent<Card>();
                    AddCard(_playerHand2, r, false, r.isChanged);
                }
                StartCoroutine(ColorLerp(_startTable.GetComponent<MeshRenderer>(), new Color32(0, 0, 0, 0), 1f, false));

                Invoke(nameof(GameMove), 0.3f);
            }
        }

        private void GameMove()
        {
            var currPlayer = _currentTurnPlayer == _player1 ? _player1 : _player2;

            var currHand = currPlayer == _player1 ? _playerHand1 : _playerHand2;

            var currCardPlayer = currPlayer == _player1 ? _cardsPlayer1 : _cardsPlayer2;

            if (currCardPlayer.Count > 0)
            {
                AddCard(currHand, currCardPlayer[0], false, false);

                if (currPlayer == _player1) _cardsPlayer1[0].SwitchEnable();

                if (currPlayer == _player2) _cardsPlayer2[0].SwitchEnable();

                currCardPlayer.Remove(currCardPlayer[0]);
            }
        }

        public void ChangeTurn(bool addCard, bool isFirstTime, bool isSentByContinue)
        {
            _canvasManager.endTurn.interactable = true;

            var destination = _currentTurnPlayer == _player1 ? camRotPlayer2 : camRotPlayer1;

            if (!isFirstTime && !isSentByContinue)
            {
                var addManaTo = _currentTurnPlayer != _player1 ? _player1 : _player2;

                addManaTo.AddMana(addManaTo == _player1 ? true : false);
            }

            StartCoroutine(TurnLerp(cam, destination, 1f, addCard, isFirstTime));

            foreach (var card in _player1.cardsInHand)
            {
                card.SwitchEnable();
            }

            foreach (var card in _player2.cardsInHand)
            {
                card.SwitchEnable();
            }

            if (_currentTurnPlayer == _player1)
            {
                _currentTurnPlayer = _player2;
                foreach (var card in _player1.cardsOnTable)
                {
                    card.canAttack = true;
                }
            }
            else
            {
                _currentTurnPlayer = _player1;
                foreach (var card in _player2.cardsOnTable)
                {
                    card.canAttack = true;
                }
            }
        }

        private void CardInteraction()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out _hit))
            {
                var card = _hit.transform.GetComponentInParent<Card>();

                if (card)
                {
                    if (card.State == CardStateType.InDeck) return;

                    if (card.playerProperty != _currentTurnPlayer.playerNum) return;

                    if (!_mouseEnter) OnMouseEnter(card);

                    if (card != _selectedCard) if (!_mouseExit) OnMouseExit();

                    if (_selectedCard == null) return;

                    if (Input.GetMouseButton(0))
                    {
                        if (card.State == CardStateType.OnTable && card.canAttack) OnDrag();
                        if (card.State == CardStateType.InHand) OnDrag();
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (card.State == CardStateType.OnStart) OnMouseClick(card);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        if (card.State == CardStateType.OnStart) return;
                        OnEndDrag();
                    }
                }
                else
                {
                    if (!_mouseExit) OnMouseExit();
                }
            }
        }

        private void OnMouseEnter(Card card)
        {
            _mouseEnter = true;
            _mouseExit = false;

            if (_cor != null) StopCoroutine(_cor);

            if (_currentTurnPlayer == _player1)
            {
                if (card.isInterectable)
                {
                    _selectedCard = card;
                    _cor = StartCoroutine(ScaleLerp(_selectedCard, _selectedCard.maxScale, 0.1f, true));
                }
            }

            else
            {
                if (card.isInterectable)
                {
                    _selectedCard = card;
                    _cor = StartCoroutine(ScaleLerp(card, card.maxScale, 0.1f, true));
                }
            }
        }

        private void OnMouseExit()
        {
            if (_cor != null) StopCoroutine(_cor);

            _mouseEnter = false;
            _mouseExit = true;

            if (_selectedCard != null) StartCoroutine(ScaleLerp(_selectedCard, _selectedCard.minScale, 0.1f, false));
        }

        private void OnMouseClick(Card card)
        {
            if (card.isInterectable)
            {
                if (!card.isChanged && card.playerProperty == PlayerNum.PlayerOne)
                {
                    card.isChanged = true;
                    card.transform.parent = null;
                    
                    _cardsPlayer1.Add(card);

                    StartCoroutine(LerpCor(card.transform, card.posInDeck, 1f, true));

                    AddCard(_playerHand1, _cardsPlayer1[0], true, true);

                    _cardsPlayer1.Remove(_cardsPlayer1[0]);
                }

                if (!card.isChanged && card.playerProperty == PlayerNum.PlayerTwo)
                {
                    card.isChanged = true;
                    card.transform.parent = null;

                    _cardsPlayer2.Add(card);

                    StartCoroutine(LerpCor(card.transform, card.posInDeck, 1f, true));

                    AddCard(_playerHand2, _cardsPlayer2[0], true, true);

                    _cardsPlayer2.Remove(_cardsPlayer2[0]);
                }
            }
        }

        private void OnDrag()
        {
            var y = Mathf.Clamp(_selectedCard.transform.position.y, 20f, 20f);

            _selectedCard.transform.position = new Vector3(_hit.point.x, y, _hit.point.z);
        }

        private void OnEndDrag()
        {
            _selectedCard.isInterectable = false;

            var collider = _currentTurnPlayer == _player1 ? _player1DeskCollider : _player2DeskCollider;

            if (_currentTurnPlayer == _player1) collider.gameObject.SetActive(true);

            if (_currentTurnPlayer == _player2) collider.gameObject.SetActive(true);

            Ray rayDown = new Ray(_selectedCard.transform.position, Vector3.down);

            _currentPlayerDeskPositions = _currentTurnPlayer == _player1 ? _player1Desk.positions : _player2Desk.positions;

            var dic = new Dictionary<Transform, float>();

            foreach (var pos in _currentPlayerDeskPositions)
            {
                if (pos.childCount > 0) continue;

                var distance = Vector3.Distance(transform.position, pos.position);

                dic.Add(pos, distance);
            }

            var destination = _selectedCard.State == CardStateType.OnTable ? _selectedCard.posOnTable : _selectedCard.posInHand;

            if (Physics.Raycast(rayDown, out RaycastHit hit, 100f))
            {
                var enemyCardOwner = _currentTurnPlayer == _player1 ? _player2 : _player1;

                if (_selectedCard.State == CardStateType.OnTable && _selectedCard.canAttack) //CardAttack
                {
                    var player = hit.transform.GetComponentInParent<Player>();

                    var card = hit.transform.GetComponentInParent<Card>();

                    if (player && _currentTurnPlayer != player)
                    {
                        if (enemyCardOwner.cardsOnTable.Any(cardx => cardx.haveTaunt))
                        {
                            StartCoroutine(LerpCor(_selectedCard.transform, destination, 0.3f, false));
                        }

                        else
                        {
                            StartCoroutine(DamageCor(_selectedCard, player.transform, 0.1f, true));
                            _selectedCard.canAttack = false;
                        }
                    }

                    else if (card && card.playerProperty != _currentTurnPlayer.playerNum && card.State == CardStateType.OnTable)
                    {
                        if (!card.haveTaunt)
                        {
                            if (enemyCardOwner.cardsOnTable.Any(cardx => cardx.haveTaunt))
                            {
                                StartCoroutine(LerpCor(_selectedCard.transform, destination, 0.3f, false));
                            }
                            else
                            {
                                StartCoroutine(DamageCor(_selectedCard, card.transform, 0.1f, false));
                                _selectedCard.canAttack = false;
                            }
                        }

                        if (card.haveTaunt)
                        {
                            StartCoroutine(DamageCor(_selectedCard, card.transform, 0.1f, false));
                            _selectedCard.canAttack = false;
                        }
                    }

                    else StartCoroutine(LerpCor(_selectedCard.transform, destination, 0.3f, false));
                }

                else if (_currentTurnPlayer.mana >= _selectedCard.Cost && hit.collider == collider) //CardPlace
                {
                    if (_selectedCard.State != CardStateType.OnTable)
                    {
                        if (dic.Count > 0)
                        {
                            _target = dic.OrderByDescending(x => x.Value).Last().Key;

                            StartCoroutine(LerpCor(_selectedCard.transform, _target.position, 0.3f, false));

                            _selectedCard.transform.SetParent(_target);

                            if (_selectedCard.State != CardStateType.OnTable) _currentTurnPlayer.EraseMana(_selectedCard.Cost, _currentTurnPlayer == _player1);

                            _selectedCard.State = CardStateType.OnTable;

                            ParseAbility(_selectedCard, _selectedCard.DetermineAbility());

                            _currentTurnPlayer.cardsOnTable.Add(_selectedCard);

                            _currentTurnPlayer.cardsInHand.Remove(_selectedCard);
                        }

                        if (dic.Count == 0) _target = null;

                        if (_target == null) StartCoroutine(LerpCor(_selectedCard.transform, destination, 0.1f, false));
                    }
                }

                else StartCoroutine(LerpCor(_selectedCard.transform, destination, 0.3f, false));
            }

            _player1DeskCollider.gameObject.SetActive(false);
            _player2DeskCollider.gameObject.SetActive(false);
        }

        private void ParseAbility(Card card, string ability)
        {
            if (ability == null) return;

            if (ability.Contains("Draw a card"))
            {
                GameMove();
            }

            if (ability.Contains("Restore 2 Health"))
            {
                card.Health += 2;
            }

            if (ability.Contains("Restore 2 Health to all friendly characters"))
            {
                foreach (var cardx in _currentTurnPlayer.cardsOnTable)
                {
                    if (cardx == _selectedCard) continue;

                    cardx.Health += 2;
                }
            }

            if (ability.Contains("Gain +1/+1 for each other friendly minion on the battlefield"))
            {
                foreach (var cardx in _currentTurnPlayer.cardsOnTable)
                {
                    if (cardx.Type == CardUnitType.Murloc)
                    {
                        cardx.Health += 1;
                        cardx.Attack += 1;
                    }
                }
            }

            if (ability.Contains("Give a friendly miniom +1/+1"))
            {
                if (_currentTurnPlayer.cardsOnTable.Count > 0)
                {
                    var miniom = _currentTurnPlayer.cardsOnTable.FirstOrDefault(x => x.Type == CardUnitType.Murloc);

                    if (miniom != null && miniom != _selectedCard)
                    {
                        miniom.Health += 1;
                        miniom.Attack += 1;
                    }
                }
            }
        }

        private IEnumerator LerpCor(Transform obj, Vector3 target, float travelTime, bool isTravelToDeck)
        {
            obj.GetComponent<Card>().isInterectable = false;

            Vector3 startPosition = obj.position;
            var startRot = obj.transform.eulerAngles;
            var endRot = new Vector3(-90f, 0f, -90f);
            float t = 0;

            while (t < 1)
            {
                if (isTravelToDeck)
                {
                    obj.eulerAngles = Vector3.Lerp(startRot, endRot, t * t);
                }
                obj.position = Vector3.Lerp(startPosition, target, t * t);
                t += Time.deltaTime / travelTime;
                yield return null;
            }
            obj.position = target;

            var card = obj.GetComponent<Card>();

            if (card.State == CardStateType.OnTable && !card.isTablePosSet)
            {
                card.isTablePosSet = true;
                card.posOnTable = card.transform.position;
            }

            if (isTravelToDeck)
            {
                obj.transform.eulerAngles = endRot;
                obj.GetComponent<Card>().SwitchEnable();
            }
            obj.GetComponent<Card>().isInterectable = true;
        }

        private IEnumerator ScaleLerp(Card card, Vector3 target, float TravelTime, bool IsInHand)
        {
            Vector3 startScale = card.transform.localScale;
            Vector3 startPos = card.transform.position;

            float z = _currentTurnPlayer == _player1 ? 50f : -50f;

            Vector3 endPos = new Vector3(card.transform.position.x, card.transform.position.y + 10f, card.transform.position.z + z);

            card.isInterectable = false;

            var destination = IsInHand ? endPos : card.posInHand;

            float t = 0;

            if (card.State == CardStateType.InHand)
            {
                while (t < 1)
                {
                    card.transform.position = Vector3.Lerp(startPos, destination, t * t);
                    card.transform.localScale = Vector3.Lerp(startScale, target, t * t);
                    t += Time.deltaTime / TravelTime;
                    yield return null;
                }
                card.transform.position = destination;
                card.transform.localScale = target;
                card.isInterectable = true;
            }

            if (card.State == CardStateType.OnTable || card.State == CardStateType.OnStart)
            {
                while (t < 1)
                {
                    card.transform.localScale = Vector3.Lerp(startScale, target, t * t);
                    t += Time.deltaTime / TravelTime;
                    yield return null;
                }
                card.transform.localScale = target;
                card.isInterectable = true;
            }
        }

        private IEnumerator ColorLerp(MeshRenderer mesh, Color target, float travelTime, bool isStart)
        {
            float t = 0;

            while (t < 1)
            {
                mesh.material.color = Color.Lerp(mesh.material.color, target, t * t);
                t += Time.deltaTime / travelTime;
                yield return null;
            }
            mesh.material.color = target;
            if (!isStart) mesh.gameObject.SetActive(false);
        }

        private IEnumerator DamageCor(Card card, Transform target, float travelTime, bool isPlayer)
        {
            var startPos = card.transform.position;
            var time = 0f;

            while (time < 1)
            {
                card.transform.position = Vector3.Lerp(startPos, target.position, time * time);
                time += Time.deltaTime / travelTime;
                yield return null;
            }
            StartCoroutine(ScaleLerp(card, new Vector3(10f, 1f, 40f), 0.1f, false));

            yield return new WaitForSeconds(0.2f);

            StartCoroutine(ScaleLerp(card, card.minScale, 0.1f, false));

            yield return new WaitForSeconds(0.01f);

            StartCoroutine(LerpCor(card.transform, card.posOnTable, 0.3f, false));

            yield return new WaitForSeconds(0.4f);

            card.DoDamage(target.transform, isPlayer);
        }

        private IEnumerator TurnLerp(Transform cam, Vector3 target, float travelTime, bool addCard, bool isVeryFirst)
        {
            var time = 0f;

            var startPos = cam.eulerAngles;

            Vector3 cardLerp1 = new Vector3(0f, 180f, 180f);

            Vector3 cardLerp2 = new Vector3(0f, 0f, 180f);

            var canvasTarget = _currentTurnPlayer == _player1 ? new Vector3(0f, 0f, 180f) : Vector3.zero;

            var player1LerpTarget = _currentTurnPlayer == _player1 ? cardLerp1 : cardLerp2;

            var player2LerpTarget = _currentTurnPlayer != _player1 ? cardLerp1 : cardLerp2;

            var player1Cards = new List<Card>();
            var player2Cards = new List<Card>();

            foreach (var card in _player1.cardsOnTable)
            {
                player1Cards.Add(card);
            }

            foreach (var card in _player2.cardsOnTable)
            {
                player2Cards.Add(card);
            }

            while (time < 1)
            {
                foreach (var card in player1Cards)
                {
                    card.transform.localEulerAngles = Vector3.Lerp(card.transform.localEulerAngles, player1LerpTarget, time * time);
                }

                foreach (var card in player2Cards)
                {
                    card.transform.localEulerAngles = Vector3.Lerp(card.transform.localEulerAngles, player2LerpTarget, time * time);
                }

                _player1portrait.transform.localEulerAngles = Vector3.Lerp(_player1portrait.transform.localEulerAngles, player1LerpTarget, time * time);
                _player2portrait.transform.localEulerAngles = Vector3.Lerp(_player2portrait.transform.localEulerAngles, player1LerpTarget, time * time);

                _player1hp.transform.localEulerAngles = Vector3.Lerp(_player1hp.transform.localEulerAngles, canvasTarget, time * time);
                _player2hp.transform.localEulerAngles = Vector3.Lerp(_player2hp.transform.localEulerAngles, canvasTarget, time * time);

                _player1mana.transform.localEulerAngles = Vector3.Lerp(_player1mana.transform.localEulerAngles, canvasTarget, time * time);
                _player2mana.transform.localEulerAngles = Vector3.Lerp(_player2mana.transform.localEulerAngles, canvasTarget, time * time);

                _turnButton.transform.localEulerAngles = Vector3.Lerp(_canvasManager.endTurn.transform.localEulerAngles, canvasTarget, time * time);
                _continueButton.transform.localEulerAngles = Vector3.Lerp(_continueButton.transform.localEulerAngles, canvasTarget, time * time);

                cam.eulerAngles = Vector3.Lerp(startPos, target, time * time);
                time += Time.deltaTime / travelTime;
                yield return null;
            }
            foreach (var card in player1Cards)
            {
                card.transform.localEulerAngles = player1LerpTarget;
            }

            foreach (var card in player2Cards)
            {
                card.transform.localEulerAngles = player2LerpTarget;
            }

            _player1hp.transform.localEulerAngles = canvasTarget;
            _player2hp.transform.localEulerAngles = canvasTarget;

            _player1mana.transform.localEulerAngles = canvasTarget;
            _player2mana.transform.localEulerAngles = canvasTarget;

            _player1portrait.transform.localEulerAngles = player1LerpTarget;
            _player2portrait.transform.localEulerAngles = player1LerpTarget;

            _turnButton.interactable = !isVeryFirst;
            _turnButton.transform.localEulerAngles = canvasTarget;
            _continueButton.transform.localEulerAngles = canvasTarget;

            cam.eulerAngles = target;

            yield return new WaitForSeconds(0.3f);
            if (addCard) GameMove();
        }

        private IEnumerator StartGameCor(bool isPlayer1)
        {
            yield return new WaitForSeconds(1);
            StartGame(isPlayer1);
        }

        public void CanvasManager_OnContinue()
        {
            ContinueGame(_stop2 == true ? false : true);

            _stop = false;

            ChangeTurn(false, _stop2 == false ? true : false, true);

            if (!_stop2)
            {
                _stop2 = true;

                StartCoroutine(StartGameCor(false));
            }
        }
    }
}