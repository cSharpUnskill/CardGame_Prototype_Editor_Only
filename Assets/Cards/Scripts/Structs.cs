using System;
using UnityEngine;
using OneLine;

namespace Cards
{
	[System.Serializable]
	public struct CardPropertiesData
	{
		[Width(30)]
		public uint Id;
		[NonSerialized]
		public ushort Cost;
		public string Name;
		[Width(50)]
		public Texture Texture;
		[Width(40)]
		public ushort Attack;
		[Width(40)]
		public ushort Health;
		[Width(65)]
		public CardUnitType Type;

		public CardParamsData GetParams()
		{
			return new CardParamsData(Cost, Attack, Health);
		}
	}

	public struct CardParamsData
	{
		public ushort Cost;
		public ushort Attack;
		public ushort Health;

		public CardParamsData(ushort cost, ushort attack, ushort health)
		{
			Cost = cost; Attack = attack; Health = health;
		}
	}
}
