using System;
using Nova;
using UnityEngine;
using UnityEngine.Events;

namespace WILCommunityGame
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private int currency;
        
        public int Currency =>  currency;
        public event Action<int> OnCurrencyChanged;
        
        public bool CanReceiveCurrency(long amount) => amount > 0 && amount <= int.MaxValue - (long)currency;

        public void AddCurrency(int amount)
        {
            if (!CanReceiveCurrency(amount))
                return;
            
            currency += amount;
            OnCurrencyChanged?.Invoke(currency);
        }
    }
}