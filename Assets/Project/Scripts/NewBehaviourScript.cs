using IFramework.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Translate
{
    protected struct ExternalKey
    {
        /// <summary>
        /// Total hours
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Token value
        /// </summary>
        public long Value { get; }

        /// <param name="time">Unix-formatted total hours</param>
        /// <param name="value">Token value</param>
        public ExternalKey(long time, long value)
        {
            Time = time;
            Value = value;
        }
    }
    internal static string GetTextBetween(string src, string from, string to, int startIndex = 0)
    {
        if (src == null)
            throw new ArgumentNullException(nameof(src));
        if (from == null)
            throw new ArgumentNullException(nameof(from));
        if (to == null)
            throw new ArgumentNullException(nameof(to));
        if (startIndex < 0 || startIndex > src.Length - 1)
            throw new ArgumentOutOfRangeException(nameof(to));

        int index = src.IndexOf(from, startIndex, StringComparison.Ordinal);
        if (index == -1)
            return null;

        int index2 = src.IndexOf(to, index, StringComparison.Ordinal);
        if (index2 == -1)
            return null;

        var result = src.Substring(index + from.Length, index2 - index - from.Length);
        return result;
    }


    internal static string ToCamelCase(string src)
    {
        string[] words = src.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        return string.Concat(words.Select(word
            => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    protected static int UnixTotalHours
    {
        get
        {
            DateTime unixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)DateTime.UtcNow.Subtract(unixTime).TotalHours;
        }
    }
    // We have use googles own api built into google Translator.
    public static IEnumerator Process(string targetLang, string sourceText, System.Action<string, int> result, int times)
    {
        yield return Process("auto", targetLang, sourceText, result, times);
    }

    // Exactly the same as above but allow the user to change from Auto, for when google get's all Jerk Butt-y
    public static IEnumerator Process(string sourceLang, string targetLang, string sourceText, System.Action<string, int> result,
        int times)
    {
        //第一步，先使用GET方式访问谷歌翻译，拿到html的tk值
        string urlte = "https://translate.google.hk";
        UnityWebRequest request1 = new UnityWebRequest(urlte, "GET");

        request1.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        request1.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request1.useHttpContinue = false;//应对某些机型的errorCallback with error=java.io.EOFException
        yield return request1.SendWebRequest();
        if (request1.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("获取服务器列表失败 = " + request1.error);
            result.Invoke("", times);
            yield break;
        }

        long tkk = 0;

        try
        {
            //这个就是返回的初始tk值
            Debug.Log(request1.downloadHandler.text);
            var tkkText = GetTextBetween(request1.downloadHandler.text, @"tkk:'", "',");

            if (tkkText == null)
            {
                result.Invoke("", times);
            }

            var splitted = tkkText.Split('.');
            if (splitted.Length != 2 || !long.TryParse(splitted[1], out tkk))
            {
                result.Invoke("", times);
            }

        }
        catch (ArgumentException)
        {
            result.Invoke("", times);
        }

        var currentExternalKey = new ExternalKey(UnixTotalHours, tkk);
        long time = DecrypthAlgorythm(sourceText, currentExternalKey);

        string tk = time.ToString() + '.' + (time ^ currentExternalKey.Time);

        string address = "https://translate.google.hk/translate_a/single";
        string url = $"{address}" + "?" +
                    $"sl={sourceLang}&" +
                    $"tl={targetLang}&" +
                    $"hl={sourceLang}&" +
                    $"q={UnityWebRequest.EscapeURL(sourceText)}&" +
                    $"tk={tk}&" +
                    "client=t&" +
                    "dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&" +
                    "ie=UTF-8&" +
                    "oe=UTF-8&" +
                    "otf=1&" +
                    "ssel=0&" +
                    "tsel=0&" +
                    "kc=7";
        //Debug.Log("地址=" + url);
        //string url = string.Format("https://translate.google.cn/translate_a/single?client=t&sl={0}&tl={1}&hl={0}&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&dt=at&ie=UTF-8&oe=UTF-8&ssel=6&tsel=3&kc=0&tk=" + tk + "&q={2}", sourceLang, targetLang, UnityWebRequest.EscapeURL(sourceText));
        /*        string url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl="
                    + sourceLang + "&tl=" + targetLang + "&dt=t&q=" + UnityWebRequest.EscapeURL(sourceText);*/
        UnityWebRequest request = new UnityWebRequest(url, "GET");
        //request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.useHttpContinue = false;//应对某些机型的errorCallback with error=java.io.EOFException
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("获取服务器列表失败 = " + request.error);
            result.Invoke("", times);
            yield break;
        }
        Debug.Log(request.downloadHandler.text);
        //JsonData jsonData = JsonMapper.ToObject(request.downloadHandler.text);
        //string translatedText = jsonData[0][0][0].ToString();
        ////Debug.Log("返回的text=" + request.downloadHandler.text);
        //Debug.Log("解析后=" + translatedText);
        //result.Invoke(translatedText, times);
    }

    private static long DecrypthAlgorythm(string source, ExternalKey CurrentExternalKey)
    {
        List<long> code = new List<long>();

        for (int g = 0; g < source.Length; g++)
        {
            int l = source[g];
            if (l < 128)
            {
                code.Add(l);
            }
            else
            {
                if (l < 2048)
                {
                    code.Add(l >> 6 | 192);
                }
                else
                {
                    if (55296 == (l & 64512) && g + 1 < source.Length && 56320 == (source[g + 1] & 64512))
                    {
                        l = 65536 + ((l & 1023) << 10) + (source[++g] & 1023);
                        code.Add(l >> 18 | 240);
                        code.Add(l >> 12 & 63 | 128);
                    }
                    else
                    {
                        code.Add(l >> 12 | 224);
                    }
                    code.Add(l >> 6 & 63 | 128);
                }
                code.Add(l & 63 | 128);
            }
        }

        long time = CurrentExternalKey.Time;

        foreach (long i in code)
        {
            time += i;
            Xr(ref time, "+-a^+6");
        }

        Xr(ref time, "+-3^+b+-f");

        time ^= CurrentExternalKey.Value;

        if (time < 0)
            time = (time & int.MaxValue) + 2147483648;

        time %= (long)1e6;

        return time;
    }

    private static void Xr(ref long a, string b)
    {
        for (int c = 0; c < b.Length - 2; c += 3)
        {
            long d = b[c + 2];

            d = 'a' <= d ? d - 87 : (long)char.GetNumericValue((char)d);
            d = '+' == b[c + 1] ? (long)((ulong)a >> (int)d) : a << (int)d;
            a = '+' == b[c] ? a + d & 4294967295 : a ^ d;
        }
    }
}


public class NewBehaviourScript : MonoBehaviour
{
    public LocalizationText LocalizationText;
    public int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Translate.Process("en", "哈哈", (
            a, b) =>
        {
            Debug.Log(a);
            Debug.Log(b);
        }, 10));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown( KeyCode.Space)) {
            if (index == 0)
            {
                LocalizationText.text.SetKey("xx");

            }
            else
            {
                LocalizationText.text.SetKey("ss");

            }

        }
    }
}
