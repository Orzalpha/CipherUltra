using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WebUI;

namespace CipherUltra.Models;
public static class TransferHelper
{
    /* Convert hex string to bin vector. */
    public static BitArray HexStr2BitArr(string str)
    {
        var length = str.Length;
        var ans = new BitArray(length * 4);
        var pos = 0;
        for (var i = length - 1; i >= 0; i--)
        {
            var ch = str[i];
            var num = -1;
            if (char.IsDigit(ch))
            {
                num = Convert.ToInt32(ch);
            }
            else if (char.IsLetter(ch))
            {
                ch = char.ToLower(ch);
                if (ch >= 'a' && ch <= 'f')
                {
                    num = ch - 'a' + 10;
                }
                else
                {
                    throw new Exception("Unexpected Input String!");
                }
            }
            for (var j = 0; j < 4; j++)
            {
                ans[pos] = Convert.ToBoolean(num % 2);
                num /= 2;
                pos++;
            }
        }
        return ans;
    }

    public static BitArray BinStr2BitArr(string str)
    {
        var length = str.Length;
        var ans = new BitArray(length);
        for (var i = length - 1; i >= 0; i--)
        {
            var ch = str[i];
            if (char.IsDigit(ch))
            {
                switch (ch)
                {
                    case '0':
                        ans[i] = false; break;
                    case '1':
                        ans[i] = true; break;
                    default: throw new Exception("Unexpected Input");
                }
            }
        }
        return ans;
    }

    public static long BitArr2Int(BitArray bitArray, int entry, int length)
    {
        long ans = 0;
        for (var i = length - 1; i >= 0; i--)
        {
            ans *= 2;
            ans += bitArray[entry + i] ? 1 : 0;
        }
        return ans;
    }

    public static BitArray Int2BitArr(long num, int length)
    {
        var bitArray = new BitArray(length);
        for (var i = 0; i < bitArray.Count; i++)
        {
            bitArray[i] = Convert.ToBoolean(num % 2);
            num /= 2;
        }
        return bitArray;
    }

    public static BitArray RoundLeftShift(BitArray bitArray, int cnt)
    {
        var tmp = new BitArray(cnt);
        var length = bitArray.Count;
        for (var i = 0; i < cnt; i++)
        {
            tmp[i] = bitArray[length - cnt + i];
        } 
        bitArray.LeftShift(cnt);
        for (var i = 0; i < cnt; i++)
        {
            bitArray[i] = tmp[i];
        }
        return bitArray;
    }

}
