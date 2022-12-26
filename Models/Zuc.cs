using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.WebUI;
using static CipherUltra.Models.TransferHelper;

namespace CipherUltra.Models;
public class Zuc
{
    public enum Mode
    {
        Encryption, Decryption
    }
    public Mode mode;
    BitArray key;
    BitArray iv1;
    BitArray iv2;
    StorageFile inputFile;
    StorageFile outputFile;
    Random random = new();

    ZUCHelper zucForEnc;
    ZUCHelper zucForMac;

    public Zuc(BitArray key, Mode mode, StorageFile inputFile, StorageFile outputFile)
    {
        this.key = key;
        this.mode = mode;
        this.inputFile = inputFile;
        this.outputFile = outputFile;
    }

    public async Task<bool> StartWork()
    {
        if (mode == Mode.Encryption)
        {
            var str = string.Empty;
            for (var i = 0; i < 8; i++)
            {
                var num = random.Next(0, 65536);
                str += num.ToString("X4");
            }
            iv1 = HexStr2BitArr(str);
            var tempbytes = new byte[16];
            iv1.CopyTo(tempbytes, 0);
            var outStream = await outputFile.OpenStreamForWriteAsync();
            var bw = new BinaryWriter(outStream);
            bw.Write(tempbytes);
            str = string.Empty;
            for (var i = 0; i < 8; i++)
            {
                var num = random.Next(0, 65536);
                str += num.ToString("X4");
            }
            iv2 = HexStr2BitArr(str);
            iv2.CopyTo(tempbytes, 0);
            //await FileIO.WriteBytesAsync(outputFile, tempbytes);
            bw.Write(tempbytes);
            zucForEnc = new ZUCHelper(key, iv1);
            zucForMac = new ZUCHelper(key, iv2);

            var inStream = await inputFile.OpenStreamForReadAsync();
            var br = new BinaryReader(inStream);
            var length = inStream.Length;
            var plain = new byte[4];
            var code = new byte[4];
            var secret = new byte[4];
            var mac = new BitArray(32);
            var mac1 = new BitArray(zucForMac.Work());
            BitArray mac2;
            mac.SetAll(false);
            BitArray CodeBitarray;
            for (var i = 0; i < length / 4; i++)
            {
                plain = br.ReadBytes(4);
                CodeBitarray = new BitArray(zucForEnc.Work());
                CodeBitarray.CopyTo(code, 0);
                mac2 = new BitArray(zucForMac.Work());
                for (var j = 0; j < 4; j++)
                {
                    secret[j] = (byte)(plain[j] ^ code[j]);
                    var plainBitarray = new BitArray(plain[j]);
                    for (var k = 0; k < plainBitarray.Length; k++)
                    {
                        if (plainBitarray[k])
                        {
                            mac.Xor(mac1);
                            mac1.LeftShift(1);
                            mac1[0] = mac1[0] ^ mac2[31];
                            mac2.LeftShift(1);
                        }
                    }

                }
                //await FileIO.WriteBytesAsync(outputFile, secret);
                bw.Write(secret);
            }
            CodeBitarray = new BitArray(zucForEnc.Work());
            CodeBitarray.CopyTo(code, 0);
            mac2 = new BitArray(zucForMac.Work());
            var tempBytesLeft = new byte[length % 4];
            for (var i = 0; i < length % 4; i++)
            {
                var tmpPlain = br.ReadByte();
                tempBytesLeft[i] = (byte)(tmpPlain ^ code[i]);
                var plainBitarray = new BitArray(tmpPlain);
                for (var j = 0; j < 8; j++)
                {
                    if (plainBitarray[j])
                    {
                        mac.Xor(mac1);
                        mac1.LeftShift(1);
                        mac1[0] = mac1[0] ^ mac2[31];
                        mac2.LeftShift(1);
                    }
                }

            }
            bw.Write(tempBytesLeft);
            mac2 = new BitArray(zucForMac.Work());
            mac.Xor(mac2);
            var macBytes = new byte[4];
            mac.CopyTo(macBytes, 0);
            bw.Write(macBytes);
            br.Close();
            bw.Close();
            return true;
        }
        else
        {
            var inStream = await inputFile.OpenStreamForReadAsync();
            var br = new BinaryReader(inStream);
            var outStream = await outputFile.OpenStreamForWriteAsync();
            var bw = new BinaryWriter(outStream);
            var length = inStream.Length;
            length = length - 16 - 16 - 4;

            var iv1Bytes = br.ReadBytes(16);
            iv1 = new BitArray(iv1Bytes);
            var iv2Bytes = br.ReadBytes(16);
            iv2 = new BitArray(iv2Bytes);
            zucForEnc = new ZUCHelper(key, iv1);
            zucForMac = new ZUCHelper(key, iv2);

            var plain = new byte[4];
            var code = new byte[4];
            var secret = new byte[4];
            var mac = new BitArray(32);
            var mac1 = new BitArray(zucForMac.Work());
            BitArray mac2;
            mac.SetAll(false);
            BitArray CodeBitarray;
            for (var i = 0; i < length / 4; i++)
            {
                secret = br.ReadBytes(4);
                CodeBitarray = new BitArray(zucForEnc.Work());
                CodeBitarray.CopyTo(code, 0);
                mac2 = new BitArray(zucForMac.Work());
                for (var j = 0; j < 4; j++)
                {
                    plain[j] = (byte)(secret[j] ^ code[j]);
                    var plainBitarray = new BitArray(plain[j]);
                    for (var k = 0; k < plainBitarray.Length; k++)
                    {
                        if (plainBitarray[k])
                        {
                            mac.Xor(mac1);
                            mac1.LeftShift(1);
                            mac1[0] = mac1[0] ^ mac2[31];
                            mac2.LeftShift(1);
                        }
                    }

                }
                bw.Write(plain);
            }
            CodeBitarray = new BitArray(zucForEnc.Work());
            CodeBitarray.CopyTo(code, 0);
            mac2 = new BitArray(zucForMac.Work());
            var tmpPlain = new byte[length % 4];
            for (var i = 0; i < length % 4; i++)
            {
                var tmpSecret = br.ReadByte();
                tmpPlain[i] = (byte)(tmpSecret ^ code[i]);
                var plainBitarray = new BitArray(tmpPlain[i]);
                for (var j = 0; j < 8; j++)
                {
                    if (plainBitarray[j])
                    {
                        mac.Xor(mac1);
                        mac1.LeftShift(1);
                        mac1[0] = mac1[0] ^ mac2[31];
                        mac2.LeftShift(1);
                    }
                }

            }
            bw.Write(tmpPlain);
            mac2 = new BitArray(zucForMac.Work());
            mac.Xor(mac2);
            var macFromFile = br.ReadBytes(4);
            var macFromFileBitArray = new BitArray(macFromFile);
            br.Close();
            bw.Close();
            if (BitArr2Int(mac, 0, mac.Length) == BitArr2Int(macFromFileBitArray, 0, macFromFileBitArray.Length))
            {
                return true;
            }
            else { return false; }

        }
    }

}
