using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryObject : ScriptableObject
    {
        public InventorySlot[] slots = new InventorySlot[63];
        
        [ContextMenu("Clear")]
        public void Clear()
        {
            slots = new InventorySlot[63];
        }
        
        public (int[], int) AddItem(int _id, int _amount, int _maxStack)
        {
            List<int> _updatedSlots = new List<int>();
            for (int _i = 0; _i < slots.Length; _i++)
            {
                if (slots[_i].id == _id && slots[_i].amount < _maxStack)
                {
                    _amount = slots[_i].AddAmount(_amount, _maxStack);
                    _updatedSlots.Add(_i);
                }
                
                if (_amount <= 0)
                {
                    return (_updatedSlots.ToArray(), 0);
                }
            }

            for (int _i = 0; _i < slots.Length; _i++)
            {
                if (slots[_i].id == 0)
                {
                    _amount = slots[_i].SetItem(_id, _amount, _maxStack);
                    _updatedSlots.Add(_i);
                }

                if (_amount <= 0)
                {
                    return (_updatedSlots.ToArray(), 0);
                }
            }

            return (_updatedSlots.ToArray(), _amount);
        }

        public void PlaceBlock(int _slotId)
        {
            slots[_slotId].RemoveAmount(1);
        }

        public void SwapSlots(int _fromSlotId, int _toSlotId, Func<int, int> _checkMaxStack)
        {
            InventorySlot _fromSlot = slots[_fromSlotId];
            InventorySlot _toSlot = slots[_toSlotId];
            if (_fromSlot.id < 1)
                return;
            if (_fromSlot.id == _toSlot.id)
            {
                
                int _maxStack = _checkMaxStack(_toSlot.id);
                if (_toSlot.amount < _maxStack)
                {
                    _fromSlot.SetAmount(_toSlot.AddAmount(_fromSlot.amount, _maxStack));
                }

                return;
            }
            
            InventorySlot _tempSlot = _toSlot.Copy();
            
            _toSlot.id = _fromSlot.id;
            _toSlot.amount = _fromSlot.amount;

            _fromSlot.id = _tempSlot.id;
            _fromSlot.amount = _tempSlot.amount;
        }
    }

    [Serializable]
    public class InventorySlot
    {
        public int id = 0;
        public int amount = 0;

        public InventorySlot() { }

        public InventorySlot(int _id, int _amount)
        {
            id = _id;
            amount = _amount;
        }

        public void SetAmount(int _amount)
        {
            if (_amount <= 0)
            {
                id = 0;
                amount = 0;
            }
            else
            {
                amount = _amount;
            }
        }

        public int AddAmount(int _amount, int _maxStack)
        {
            int _newAmount = amount + _amount;
            
            if (_newAmount > _maxStack)
            {
                amount = _maxStack;
                return _newAmount - _maxStack;
            }

            amount = _newAmount;
            return 0;
        }

        public int SetItem(int _id, int _amount, int _maxStack)
        {
            id = _id;

            if (_amount > _maxStack)
            {
                amount = _maxStack;
                return _amount - _maxStack;
            }

            amount = _amount;
            return 0;
        }

        public void RemoveAmount(int _amount)
        {
            amount -= _amount;
            if (amount <= 0)
            {
                id = 0;
                amount = 0;
            }
        }

        public InventorySlot Copy()
        {
            return new InventorySlot(id, amount);
        }
    }
}