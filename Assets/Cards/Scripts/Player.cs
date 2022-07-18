using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace Cards
{
    public class Player : MonoBehaviour
    {
        public int health;

        public int mana = 0;

        public int maxMana = 0;

        [SerializeField]
        private TextMeshPro healthTMP;

        [SerializeField]
        private TextMeshPro manaTMP;

        [SerializeField]
        private List<GameObject> _manaObjects = new List<GameObject>();

        private List<GameObject> _disabledMana = new List<GameObject>();

        [HideInInspector]
        public List<Card> cardsOnTable = new List<Card>();

        [HideInInspector]
        public List<Card> cardsInHand = new List<Card>();

        public PlayerNum playerNum;

        private int i = 0;

        private void Update()
        {
            healthTMP.text = health.ToString();
            manaTMP.text = $"{mana}/10";
        }

        public void AddMana(bool isPlayer1)
        {
            if (maxMana == 10 || mana == 10) return;

            maxMana++;
            mana = maxMana;

            if (!isPlayer1) return;

            _manaObjects[i].SetActive(true);

            foreach (var obj in _disabledMana)
            {
                obj.SetActive(true);
            }

            _disabledMana.Clear();

            i++;
        }

        public void EraseMana(int mana, bool isPlayer1)
        {
            this.mana -= mana;

            for (int j = mana; j > 0; j--)
            {
                if (!isPlayer1) return;

                var manaObj = _manaObjects.Where(x => x.activeSelf == true).Reverse().First();

                manaObj.SetActive(false);

                _disabledMana.Add(manaObj);
            }
        }
    }
}