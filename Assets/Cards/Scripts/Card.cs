using TMPro;
using UnityEngine;
using System;

namespace Cards
{
    public class Card : MonoBehaviour
    {
        [SerializeField]
        private GameObject _frontCard;

        [Space, SerializeField]
        private MeshRenderer _picture;

        [SerializeField]
        public TextMeshPro _cost;

        [SerializeField]
        private TextMeshPro _name;

        [SerializeField]
        private TextMeshPro _discription;

        [SerializeField]
        private TextMeshPro _attack;

        [SerializeField]
        public TextMeshPro _health;

        [SerializeField]
        private TextMeshPro _type;

        [HideInInspector]
        public Vector3 posInHand;

        [HideInInspector]
        public Vector3 posOnTable;

        [HideInInspector]
        public Vector3 posInDeck;

        [HideInInspector]
        public bool isTablePosSet = false;

        [HideInInspector]
        public Vector3 minScale;

        [HideInInspector]
        public Vector3 maxScale;

        [HideInInspector]
        public bool isInterectable = true;

        [HideInInspector]
        public bool isChanged = false;
        public bool IsFrontCard => _frontCard.activeSelf;
        public CardStateType State { get; set; } = CardStateType.InDeck;

        public bool canAttack;

        public bool haveTaunt;

        private CardPropertiesData _data;

        public PlayerNum playerProperty;

        public int Cost { get => _data.Cost; }

        public int Attack { get => _data.Attack; set { _data.Attack = (ushort)value; var r = (Int32)value; _attack.text = r.ToString(); } }

        public int Health { get => _data.Health; set { _data.Health = (ushort)value; var r = (Int32)value; _health.text = r.ToString(); } }

        public CardUnitType Type { get => _data.Type; }

        public string Descript { get => _discription.text; }

        public void Configuration(CardPropertiesData data, Material picture, string description)
        {
            _picture.material = picture;
            _cost.text = data.Cost.ToString();
            _name.text = data.Name.ToString();
            _discription.text = description;
            _attack.text = data.Attack.ToString();
            _health.text = data.Health.ToString();
            _type.text = data.Type == CardUnitType.None ? "" : data.Type.ToString();
            _data = data;
            if (_discription.text.Contains("Taunt")) haveTaunt = true;
            if (_discription.text.Contains("Charge")) canAttack = true;
            DetermineAbility();
        }

        void Start()
        {
            minScale = new Vector3(70f, 1f, 100f);
            maxScale = new Vector3(130f, 1f, 160f);
        }

        public string DetermineAbility()
        {
            if (_discription.text.Contains("Restore 2 Health to all friendly characters"))
            {
                return "Restore 2 Health to all friendly characters";
            }

            if (_discription.text.Contains("Gain +1/+1 for each other friendly minion on the battlefield"))
            {
                return "Gain +1/+1 for each other friendly minion on the battlefield";
            }

            if (_discription.text.Contains("Restore 2 Health"))
            {
                return "Restore 2 Health";
            }

            if (_discription.text.Contains("Give a friendly miniom +1/+1"))
            {
                return "Give a friendly miniom +1/+1";
            }

            if (_discription.text.Contains("Draw a card"))
            {
                return "Draw a card";
            }

            return null;
        }

        public void DoDamage(Transform target, bool isPlayer)
        {
            if (isPlayer) target.GetComponent<Player>().health -= Attack;

            if (!isPlayer)
            {
                var card = target.GetComponent<Card>();
                card.Health -= Attack;
                Health -= card.Attack;
            }
        }

        [ContextMenu("Switch Enable")]
        public void SwitchEnable()
        {
            var boolean = !IsFrontCard;
            _frontCard.SetActive(boolean);
            _picture.enabled = boolean;
        }
    }
}