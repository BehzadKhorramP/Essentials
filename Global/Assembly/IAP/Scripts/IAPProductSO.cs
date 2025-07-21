using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEH.IAP
{
    [CreateAssetMenu(fileName = "IAPProductSO", menuName = "IAP/IAPProductSO")]

    public class IAPProductSO : ScriptableObject
    {
        public string m_ProductID;
    }


    public static class IAPProductExention
    {
        public static bool IsValid(this IAPProductSO iAPProductSO)
        {
            return iAPProductSO != null && !string.IsNullOrEmpty(iAPProductSO.m_ProductID);
        }

        public static bool IsValidAndEquals(this IAPProductSO iAPProductSO, string id)
        {
            return iAPProductSO.IsValid() && iAPProductSO.m_ProductID.Equals(id);
        }
    }
}