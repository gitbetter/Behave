using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static List<T> Shuffle<T>(this List<T> list) {
        List<T> copy = new List<T>(list);
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n);
            T val = copy[k];
            copy[k] = copy[n];
            copy[n] = val;
        }
        return copy;
    }
}
