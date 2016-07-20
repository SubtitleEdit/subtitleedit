///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Original project by pyomin at https://github.com/pyomin/aribts4j/blob/master/src/main/java/com/github/pyomin/aribts4j/util/AribString.java
// Converted to C# from java (July 2016 by Nikse)
// Note: Keep this file close to original, so it will be easier to compare to original code
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    public static class AribStringExtension
    {
        public static byte[] GetUtf8Bytes(this string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }
    }

    public static class AribString
    {
        private enum StringSize
        {
            Small,     //SSZ
            Medium,    //MSZ
            Normal,    //NSZ
            Micro,     //SZX 0x60
            HighW,     //SZX 0x41
            WidthW,    //SZX 0x44
            W,         //SZX 0x45
            Special1,  //SZX 0x6B
            Special2,  //SZX 0x64
        }

        private static readonly bool[] AbCharSizeTable = {
            false,  // CODE_UNKNOWN                         不明なグラフィックセット(非対応)
            true,   // CODE_KANJI                           Kanji
            false,  // CODE_ALPHANUMERIC                    Alphanumeric
            false,  // CODE_HIRAGANA                        Hiragana
            false,  // CODE_KATAKANA                        Katakana
            false,  // CODE_MOSAIC_A                        Mosaic A
            false,  // CODE_MOSAIC_B                        Mosaic B
            false,  // CODE_MOSAIC_C                        Mosaic C
            false,  // CODE_MOSAIC_D                        Mosaic D
            false,  // CODE_PROP_ALPHANUMERIC               Proportional Alphanumeric
            false,  // CODE_PROP_HIRAGANA                   Proportional Hiragana
            false,  // CODE_PROP_KATAKANA                   Proportional Katakana
            false,  // CODE_JIS_X0201_KATAKANA              JIS X 0201 Katakana
            true,   // CODE_JIS_KANJI_PLANE_1               JIS compatible Kanji Plane 1
            true,   // CODE_JIS_KANJI_PLANE_2               JIS compatible Kanji Plane 2
            true    // CODE_ADDITIONAL_SYMBOLS              Additional symbols
    };
        private const int CodeUnknown = 0;      // 不明なグラフィックセット(非対応)
        private const int CodeKanji = 1;        // Kanji
        private const int CodeAlphanumeric = 2; // Alphanumeric
        private const int CodeHiragana = 3;    // Hiragana
        private const int CodeKatakana = 4;    // Katakana
        private const int CodeMosaicA = 5;    // Mosaic A
        private const int CodeMosaicB = 6;    // Mosaic B
        private const int CodeMosaicC = 7;    // Mosaic C
        private const int CodeMosaicD = 8;    // Mosaic D
        private const int CodePropAlphanumeric = 9; // Proportional Alphanumeric
        private const int CodePropHiragana = 10;    // Proportional Hiragana
        private const int CodePropKatakana = 11;    // Proportional Katakana
        private const int CodeJisX0201Katakana = 12;  // JIS X 0201 Katakana
        private const int CodeJisKanjiPlane1 = 13;   // JIS compatible Kanji Plane 1
        private const int CodeJisKanjiPlane2 = 14;   // JIS compatible Kanji Plane 2
        private const int CodeAdditionalSymbols = 15;  // Additional symbols

        private static readonly string[] AszSymbolsTable1 =
            {
                    "【HV】", "【SD】", "【Ｐ】", "【Ｗ】", "【MV】", "【手】", "【字】", "【双】",            // 0x7A50 - 0x7A57      90/48 - 90/55
                    "【デ】", "【Ｓ】", "【二】", "【多】", "【解】", "【SS】", "【Ｂ】", "【Ｎ】",            // 0x7A58 - 0x7A5F      90/56 - 90/63
                    "■", "●", "【天】", "【交】", "【映】", "【無】", "【料】", "【年齢制限】",                // 0x7A60 - 0x7A67      90/64 - 90/71
                    "【前】", "【後】", "【再】", "【新】", "【初】", "【終】", "【生】", "【販】",            // 0x7A68 - 0x7A6F      90/72 - 90/79
                    "【声】", "【吹】", "【PPV】", "（秘）", "ほか"                                          // 0x7A70 - 0x7A74      90/80 - 90/84
            };

        private static readonly string[] AszSymbolsTable2 =
                {
                    "→", "←", "↑", "↓", "●", "○", "年", "月",                  // 0x7C21 - 0x7C28      92/01 - 92/08
                    "日", "円", "㎡", "㎥", "㎝", "㎠", "㎤", "０.",            // 0x7C29 - 0x7C30      92/09 - 92/16
                    "１.", "２.", "３.", "４.", "５.", "６.", "７.", "８.",     // 0x7C31 - 0x7C38      92/17 - 92/24
                    "９.", "氏", "副", "元", "故", "前", "[新]", "０,",         // 0x7C39 - 0x7C40      92/25 - 92/32
                    "１,", "２,", "３,", "４,", "５,", "６,", "７,", "８,",     // 0x7C41 - 0x7C48      92/33 - 92/40
                    "９,", "(社", "(財", "(有", "(株", "(代", "(問", "▶",       // 0x7C49 - 0x7C50      92/41 - 92/48
                    "◀", "〖", "〗", "⟐", "^2", "^3", "(CD", "(vn",            // 0x7C51 - 0x7C58      92/49 - 92/56
                    "(ob", "(cb", "(ce", "mb", "(hp", "(br", "(p", "(s",       // 0x7C59 - 0x7C60      92/57 - 92/64
                    "(ms", "(t", "(bs", "(b", "(tb", "(tp", "(ds", "(ag",      // 0x7C61 - 0x7C68      92/65 - 92/72
                    "(eg", "(vo", "(fl", "(ke", "y", "(sa", "x", "(sy",        // 0x7C69 - 0x7C70      92/73 - 92/80
                    "n", "(or", "g", "(pe", "r", "(R", "(C", "(箏",            // 0x7C71 - 0x7C78      92/81 - 92/88
                    "DJ", "[演]", "Fax"                                                                                                                                           // 0x7C79 - 0x7C7B      92/89 - 92/91
            };

        private static readonly string[] AszSymbolsTable3 =
                {
                    "㈪", "㈫", "㈬", "㈭", "㈮", "㈯", "㈰", "㈷",                                 // 0x7D21 - 0x7D28      93/01 - 93/08
                    "㍾", "㍽", "㍼", "㍻", "№", "℡", "〶", "○",                                  // 0x7D29 - 0x7D30      93/09 - 93/16
                    "〔本〕", "〔三〕", "〔二〕", "〔安〕", "〔点〕", "〔打〕", "〔盗〕", "〔勝〕",   // 0x7D31 - 0x7D38      93/17 - 93/24
                    "〔敗〕", "〔Ｓ〕", "［投］", "［捕］", "［一］", "［二］", "［三］", "［遊］",  // 0x7D39 - 0x7D40      93/25 - 93/32
                    "［左］", "［中］", "［右］", "［指］", "［走］", "［打］", "㍑", "㎏",         // 0x7D41 - 0x7D48      93/33 - 93/40
                    "㎐", "ha", "㎞", "㎢", "㍱", "・", "・", "1/2",                           // 0x7D49 - 0x7D50      93/41 - 93/48
                    "0/3", "1/3", "2/3", "1/4", "3/4", "1/5", "2/5", "3/5",                 // 0x7D51 - 0x7D58      93/49 - 93/56
                    "4/5", "1/6", "5/6", "1/7", "1/8", "1/9", "1/10", "☀",                 // 0x7D59 - 0x7D60      93/57 - 93/64
                    "☁", "☂", "☃", "☖", "☗", "▽", "▼", "♦",                           // 0x7D61 - 0x7D68      93/65 - 93/72
                    "♥", "♣", "♠", "⌺", "⦿", "‼", "⁉", "(曇/晴",                         // 0x7D69 - 0x7D70      93/73 - 93/80
                    "☔", "(雨", "(雪", "(大雪", "⚡", "(雷雨", "　", "・",                  // 0x7D71 - 0x7D78      93/81 - 93/88
                    "・", "♬", "☎"                                                                                                                                                   // 0x7D79 - 0x7D7B      93/89 - 93/91
            };

        private static readonly string[] AszSymbolsTable4 =
                {
                    "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ",             // 0x7E21 - 0x7E28      94/01 - 94/08
                    "Ⅸ", "Ⅹ", "Ⅺ", "Ⅻ", "⑰", "⑱", "⑲", "⑳",                // 0x7E29 - 0x7E30      94/09 - 94/16
                    "⑴", "⑵", "⑶", "⑷", "⑸", "⑹", "⑺", "⑻",            // 0x7E31 - 0x7E38      94/17 - 94/24
                    "⑼", "⑽", "⑾", "⑿", "㉑", "㉒", "㉓", "㉔",            // 0x7E39 - 0x7E40      94/25 - 94/32
                    "(A", "(B", "(C", "(D", "(E", "(F", "(G", "(H",          // 0x7E41 - 0x7E48      94/33 - 94/40
                    "(I", "(J", "(K", "(L", "(M", "(N", "(O", "(P",          // 0x7E49 - 0x7E50      94/41 - 94/48
                    "(Q", "(R", "(S", "(T", "(U", "(V", "(W", "(X",          // 0x7E51 - 0x7E58      94/49 - 94/56
                    "(Y", "(Z", "㉕", "㉖", "㉗", "㉘", "㉙", "㉚",           // 0x7E59 - 0x7E60      94/57 - 94/64
                    "①", "②", "③", "④", "⑤", "⑥", "⑦", "⑧",                  // 0x7E61 - 0x7E68      94/65 - 94/72
                    "⑨", "⑩", "⑪", "⑫", "⑬", "⑭", "⑮", "⑯",                  // 0x7E69 - 0x7E70      94/73 - 94/80
                    "❶", "❷", "❸", "❹", "❺", "❻", "❼", "❽",                  // 0x7E71 - 0x7E78      94/81 - 94/88
                    "❾", "❿", "⓫", "⓬", "㉛"                                 // 0x7E79 - 0x7E7D      94/89 - 94/93
            };

        private static readonly string[] AszSymbolsTable5 =
                {
                    "㐂", "亭", "份", "仿", "侚", "俉", "傜", "儞",                  // 0x7521 - 0x7528      85/01 - 85/08
                    "冼", "㔟", "匇", "卡", "卬", "詹", "吉", "呍",                  // 0x7529 - 0x7530      85/09 - 85/16
                    "咖", "咜", "咩", "唎", "啊", "噲", "囤", "圳",                  // 0x7531 - 0x7538      85/17 - 85/24
                    "圴", "塚", "墀", "姤", "娣", "婕", "寬", "﨑",                  // 0x7539 - 0x7540      85/25 - 85/32
                    "㟢", "庬", "弴", "彅", "德", "怗", "恵", "愰",                  // 0x7541 - 0x7548      85/33 - 85/40
                    "昤", "曈", "曙", "曺", "曻", "桒", "・", "椑",                  // 0x7549 - 0x7550      85/41 - 85/48
                    "椻", "橅", "檑", "櫛", "・", "・", "・", "毱",                  // 0x7551 - 0x7558      85/49 - 85/56
                    "泠", "洮", "海", "涿", "淊", "淸", "渚", "潞",                  // 0x7559 - 0x7560      85/57 - 85/64
                    "濹", "灤", "・", "・", "煇", "燁", "爀", "玟",                  // 0x7561 - 0x7568      85/65 - 85/72
                    "・", "珉", "珖", "琛", "琡", "琢", "琦", "琪",                  // 0x7569 - 0x7570      85/73 - 85/80
                    "琬", "琹", "瑋", "㻚", "畵", "疁", "睲", "䂓",                  // 0x7571 - 0x7578      85/81 - 85/88
                    "磈", "磠", "祇", "禮", "・", "・"                               // 0x7579 - 0x757E      85/89 - 85/94
            };

        private static readonly string[] AszSymbolsTable6 =
                {
                    "・", "秚", "稞", "筿", "簱", "䉤", "綋", "羡",                  // 0x7621 - 0x7628      86/01 - 86/08
                    "脘", "脺", "・", "芮", "葛", "蓜", "蓬", "蕙",                  // 0x7629 - 0x7630      86/09 - 86/16
                    "藎", "蝕", "蟬", "蠋", "裵", "角", "諶", "跎",                  // 0x7631 - 0x7638      86/17 - 86/24
                    "辻", "迶", "郝", "鄧", "鄭", "醲", "鈳", "銈",                  // 0x7639 - 0x7640      86/25 - 86/32
                    "錡", "鍈", "閒", "雞", "餃", "饀", "髙", "鯖",                  // 0x7641 - 0x7648      86/33 - 86/40
                    "鷗", "麴", "麵"                                                                                                                                                   // 0x7649 - 0x764B      86/41 - 86/43
            };

        private static readonly int[] CodeG = new int[4];
        private static int _pLockingGL;
        private static int _pLockingGR;
        private static int _pSingleGL;
        private static int _byEscSeqCount;
        private static int _byEscSeqIndex;
        private static bool _bIsEscSeqDrcs;
        private static StringSize _emStrSize;
        private static byte[] _acAlphanumericTable;
        private static byte[] _acHiraganaTable;
        private static byte[] _acKatakanaTable;
        private static byte[] _acJisKatakanaTable;

        private static bool IsSmallCharMode()
        {
            bool bRet = false;
            switch (_emStrSize)
            {
                case StringSize.Small:
                    bRet = true;
                    break;
                case StringSize.Medium:
                    bRet = true;
                    break;
                case StringSize.Normal:
                    bRet = false;
                    break;
                case StringSize.Micro:
                    bRet = true;
                    break;
                case StringSize.HighW:
                    bRet = false;
                    break;
                case StringSize.WidthW:
                    bRet = false;
                    break;
                case StringSize.W:
                    bRet = false;
                    break;
                case StringSize.Special1:
                    bRet = false;
                    break;
                case StringSize.Special2:
                    bRet = false;
                    break;
                default:
                    break;
            }
            return bRet;
        }

        public static string AribToString(byte[] sourceData)
        {
            var lpszDst = new byte[994096];
            return AribToStringInternal(lpszDst, sourceData, sourceData.Length);
        }

        private static string AribToStringInternal(byte[] lpszDst, byte[] pSrcData, int dwSrcLen)
        {

            if (pSrcData == null || dwSrcLen == 0 || lpszDst == null)
                return null;

            int dwSrcPos = 0;
            int dwDstLen = 0;
            int dwSrcData;

            // 状態初期設定
            _byEscSeqCount = 0;
            _pSingleGL = -1;

            CodeG[0] = CodeKanji;
            CodeG[1] = CodeAlphanumeric;
            CodeG[2] = CodeHiragana;
            CodeG[3] = CodeKatakana;

            _pLockingGL = 0;
            _pLockingGR = 2;

            _emStrSize = StringSize.Normal;

            while (dwSrcPos < dwSrcLen)
            {
                dwSrcData = pSrcData[dwSrcPos] & 0xFF;

                if (_byEscSeqCount == 0)
                {

                    // GL/GR領域
                    if ((dwSrcData >= 0x21) && (dwSrcData <= 0x7E))
                    {
                        // GL領域
                        int curCodeSet = (_pSingleGL != -1) ? CodeG[_pSingleGL] : CodeG[_pLockingGL];
                        _pSingleGL = -1;

                        if (AbCharSizeTable[curCodeSet])
                        {
                            // 2バイトコード
                            if ((dwSrcLen - dwSrcPos) < 2)
                                break;

                            dwDstLen += ProcessCharCode(lpszDst, dwDstLen, (pSrcData[dwSrcPos + 0] << 8) | pSrcData[dwSrcPos + 1], curCodeSet);
                            dwSrcPos++;
                        }
                        else
                        {
                            // 1バイトコード
                            dwDstLen += ProcessCharCode(lpszDst, dwDstLen, dwSrcData, curCodeSet);
                        }
                    }
                    else if ((dwSrcData >= 0xA1) && (dwSrcData <= 0xFE))
                    {
                        // GR領域
                        int curCodeSet = CodeG[_pLockingGR];

                        if (AbCharSizeTable[curCodeSet])
                        {
                            // 2バイトコード
                            if ((dwSrcLen - dwSrcPos) < 2)
                                break;

                            dwDstLen += ProcessCharCode(lpszDst, dwDstLen, ((pSrcData[dwSrcPos + 0] & 0x7F) << 8) | (pSrcData[dwSrcPos + 1] & 0x7F), curCodeSet);
                            dwSrcPos++;
                        }
                        else
                        {
                            // 1バイトコード
                            dwDstLen += ProcessCharCode(lpszDst, dwDstLen, (dwSrcData & 0x7F), curCodeSet);
                        }
                    }
                    else
                    {
                        // 制御コード
                        switch (dwSrcData)
                        {
                            case 0x0F:
                                LockingShiftGL(0);
                                break;  // LS0
                            case 0x0E:
                                LockingShiftGL(1);
                                break;  // LS1
                            case 0x19:
                                SingleShiftGL(2);
                                break;  // SS2
                            case 0x1D:
                                SingleShiftGL(3);
                                break;  // SS3
                            case 0x1B:
                                _byEscSeqCount = 1;
                                break;  // ESC
                            case 0x89:
                                _emStrSize = StringSize.Medium;
                                break;  // MSZ
                            case 0x8A:
                                _emStrSize = StringSize.Normal;
                                break;  // NSZ
                            case 0x20:
                            case 0xA0:
                                //SP 空白
                                //空白は文字サイズの影響あり
                                if (!IsSmallCharMode())
                                {
                                    lpszDst[dwDstLen] = 0x20;
                                    lpszDst[dwDstLen + 1] = 0x00;
                                    //strcpy(&lpszDst[dwDstLen], "　");
                                    //dwDstLen += 3;
                                    dwDstLen += 2;
                                }
                                else
                                {
                                    lpszDst[dwDstLen++] = 0x20;
                                    //lpszDst[dwDstLen++] = TEXT(' ');
                                }
                                break;
                            default:
                                break;        // 非対応
                        }
                    }
                }
                else
                {
                    // エスケープシーケンス処理
                    ProcessEscapeSeq(dwSrcData);
                }

                dwSrcPos++;
            }

            // 終端文字
            lpszDst[dwDstLen] = 0x00;
            //lpszDst[dwDstLen] = TEXT('\0');

            var buffer = new byte[dwDstLen];
            Buffer.BlockCopy(lpszDst, 0, buffer, 0, dwDstLen);
            return NewString(buffer, "UTF-8"); //Arrays.copyOfRange(lpszDst, 0, dwDstLen)
        }

        private static int ProcessCharCode(byte[] lpszDst, int ptr, int wCode, int codeSet)
        {
            switch (codeSet)
            {
                case CodeKanji:
                case CodeJisKanjiPlane1:
                case CodeJisKanjiPlane2:
                    // 漢字コード出力
                    return PutKanjiChar(lpszDst, ptr, wCode);

                case CodeAlphanumeric:
                case CodePropAlphanumeric:
                    // 英数字コード出力
                    if (IsSmallCharMode() == false)
                    {
                        //全角テーブルコード取得
                        return PutAlphanumericChar(lpszDst, ptr, wCode);
                    }
                    else
                    {
                        //半角はそのまま出力
                        lpszDst[ptr] = (byte)(wCode & 0x00FF);
                        return 1;
                    }

                case CodeHiragana:
                case CodePropHiragana:
                    // ひらがなコード出力
                    return PutHiraganaChar(lpszDst, ptr, wCode);

                case CodePropKatakana:
                case CodeKatakana:
                    // カタカナコード出力
                    return PutKatakanaChar(lpszDst, ptr, wCode);

                case CodeJisX0201Katakana:
                    // JISカタカナコード出力
                    return PutJisKatakanaChar(lpszDst, ptr, wCode);

                case CodeAdditionalSymbols:
                    // 追加シンボルコード出力
                    return PutSymbolsChar(lpszDst, ptr, wCode);

                default:
                    return 0;
            }
        }

        private static string NewString(byte[] buffer, string encodingName)
        {
            return System.Text.Encoding.GetEncoding(encodingName).GetString(buffer);
        }

        private static int PutKanjiChar(byte[] lpszDst, int ptr, int wCode)
        {
            byte[] code = new byte[8];

            code[0] = 0x1B;
            code[1] = 0x24;
            code[2] = 0x40;
            code[3] = (byte)((wCode & 0x7F00) >> 8);
            code[4] = (byte)(wCode & 0x007F);
            code[5] = 0x1B;
            code[6] = 0x28;
            code[7] = 0x4A;

            code = new byte[2];
            code[0] = 33;
            code[1] = 74;

            byte[] utf8Bytes = NewString(code, "ISO-2022-JP").GetUtf8Bytes();

            Array.Copy(utf8Bytes, 0, lpszDst, ptr, utf8Bytes.Length); //System.arraycopy(utf8Bytes, 0, lpszDst, ptr, utf8Bytes.Length);
            lpszDst[ptr + utf8Bytes.Length] = 0x00;

            return utf8Bytes.Length;
        }

        private static int PutAlphanumericChar(byte[] lpszDst, int ptr, int wCode)
        {
            // 英数字全角文字コード変換
            if (_acAlphanumericTable == null)
            {
                _acAlphanumericTable = (
                                "　　　　　　　　　　　　　　　　" +
                                "　　　　　　　　　　　　　　　　" +
                                "　！”＃＄％＆’（）＊＋，－．／" + 
                                "０１２３４５６７８９：；＜＝＞？" +
                                "＠ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯ" +
                                "ＰＱＲＳＴＵＶＷＸＹＺ［￥］＾＿" +
                                "　ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏ" +
                                "ｐｑｒｓｔｕｖｗｘｙｚ｛｜｝￣　"
                ).GetUtf8Bytes();
            }

            /*
            #ifdef _UNICODE
                    lpszDst[0] = acAlphanumericTableZenkaku[wCode];

                    return 1;
            #else
            */
            lpszDst[ptr + 0] = _acAlphanumericTable[wCode * 3 + 0];
            lpszDst[ptr + 1] = _acAlphanumericTable[wCode * 3 + 1];
            lpszDst[ptr + 2] = _acAlphanumericTable[wCode * 3 + 2];

            return 3;
            /*
            #endif
            */
        }

        private static int PutHiraganaChar(byte[] lpszDst, int ptr, int wCode)
        {
            // ひらがな文字コード変換
            if (_acHiraganaTable == null)
            {
                _acHiraganaTable = (
                        "　　　　　　　　　　　　　　　　" +
                                "　　　　　　　　　　　　　　　　" +
                                "　ぁあぃいぅうぇえぉおかがきぎく" +
                                "ぐけげこごさざしじすずせぜそぞた" +
                                "だちぢっつづてでとどなにぬねのは" +
                                "ばぱひびぴふぶぷへべぺほぼぽまみ" +
                                "むめもゃやゅゆょよらりるれろゎわ" +
                                "ゐゑをん　　　ゝゞー。「」、・　"
                ).GetUtf8Bytes();
            }
            /*
            #ifdef _UNICODE
                    lpszDst[0] = acHiraganaTable[wCode];

                    return 1UL;
            #else
            */
            lpszDst[ptr + 0] = _acHiraganaTable[wCode * 3 + 0];
            lpszDst[ptr + 1] = _acHiraganaTable[wCode * 3 + 1];
            lpszDst[ptr + 2] = _acHiraganaTable[wCode * 3 + 2];

            return 3;
            /*
            #endif
            */
        }

        private static int PutKatakanaChar(byte[] lpszDst, int ptr, int wCode)
        {
            // カタカナ英数字文字コード変換
            if (_acKatakanaTable == null)
            {
                _acKatakanaTable = (
                        "　　　　　　　　　　　　　　　　" +
                                "　　　　　　　　　　　　　　　　" +
                                "　ァアィイゥウェエォオカガキギク" +
                                "グケゲコゴサザシジスズセゼソゾタ" +
                                "ダチヂッツヅテデトドナニヌネノハ" +
                                "バパヒビピフブプヘベペホボポマミ" +
                                "ムメモャヤュユョヨラリルレロヮワ" +
                                "ヰヱヲンヴヵヶヽヾー。「」、・　"
                ).GetUtf8Bytes();
            }
            /*
            #ifdef _UNICODE
                    lpszDst[0] = acKatakanaTable[wCode];

                    return 1UL;
            #else
            */
            lpszDst[ptr + 0] = _acKatakanaTable[wCode * 3 + 0];
            lpszDst[ptr + 1] = _acKatakanaTable[wCode * 3 + 1];
            lpszDst[ptr + 2] = _acKatakanaTable[wCode * 3 + 2];

            return 3;
            /*
            #endif
            */
        }

        private static int PutJisKatakanaChar(byte[] lpszDst, int ptr, int wCode)
        {
            // JISカタカナ文字コード変換
            if (_acJisKatakanaTable == null)
            {
                _acJisKatakanaTable = (
                        "　　　　　　　　　　　　　　　　" +
                                "　　　　　　　　　　　　　　　　" +
                                "　。「」、・ヲァィゥェォャュョッ" +
                                "ーアイウエオカキクケコサシスセソ" +
                                "タチツテトナニヌネノハヒフヘホマ" +
                                "ミムメモヤユヨラリルレロワン゛゜" +
                                "　　　　　　　　　　　　　　　　" +
                                "　　　　　　　　　　　　　　　　"
                ).GetUtf8Bytes();
            }

            /*
            #ifdef _UNICODE
                    lpszDst[0] = acJisKatakanaTable[wCode];

                    return 1UL;
            #else
            */
            lpszDst[ptr + 0] = _acJisKatakanaTable[wCode * 3 + 0];
            lpszDst[ptr + 1] = _acJisKatakanaTable[wCode * 3 + 1];
            lpszDst[ptr + 2] = _acJisKatakanaTable[wCode * 3 + 2];

            return 3;
            /*
            #endif
            */
        }

        private static int PutSymbolsChar(byte[] lpszDst, int ptr, int wCode)
        {
            // 追加シンボル文字コード変換(とりあえず必要そうなものだけ)

            // シンボルを変換する
            String newS = null;
            if ((wCode >= 0x7A50) && (wCode <= 0x7A74))
            {
                newS = AszSymbolsTable1[wCode - 0x7A50];
            }
            else if ((wCode >= 0x7C21) && (wCode <= 0x7C7B))
            {
                newS = AszSymbolsTable2[wCode - 0x7C21];
            }
            else if ((wCode >= 0x7D21) && (wCode <= 0x7D7B))
            {
                newS = AszSymbolsTable3[wCode - 0x7D21];
            }
            else if ((wCode >= 0x7E21) && (wCode <= 0x7E7D))
            {
                newS = AszSymbolsTable4[wCode - 0x7E21];
            }
            else if ((wCode >= 0x7521) && (wCode <= 0x757E))
            {
                newS = AszSymbolsTable5[wCode - 0x7521];
            }
            else if ((wCode >= 0x7621) && (wCode <= 0x764B))
            {
                newS = AszSymbolsTable6[wCode - 0x7621];
            }
            else
            {
                newS = "・";
            }

            byte[] newB = newS.GetUtf8Bytes();
            Array.Copy(newB, 0, lpszDst, ptr, newB.Length); //System.arraycopy(newB, 0, lpszDst, ptr, newB.Length);

            return newB.Length;
        }

        private static void ProcessEscapeSeq(int byCode)
        {
            // エスケープシーケンス処理
            switch (_byEscSeqCount)
            {
                // 1バイト目
                case 1:
                    switch (byCode)
                    {
                        // Invocation of code elements
                        case 0x6E:
                            LockingShiftGL(2);
                            _byEscSeqCount = 0;
                            return;         // LS2
                        case 0x6F:
                            LockingShiftGL(3);
                            _byEscSeqCount = 0;
                            return;         // LS3
                        case 0x7E:
                            LockingShiftGR(1);
                            _byEscSeqCount = 0;
                            return;         // LS1R
                        case 0x7D:
                            LockingShiftGR(2);
                            _byEscSeqCount = 0;
                            return;         // LS2R
                        case 0x7C:
                            LockingShiftGR(3);
                            _byEscSeqCount = 0;
                            return;         // LS3R

                        // Designation of graphic sets
                        case 0x24:
                        case 0x28:
                            _byEscSeqIndex = 0;
                            break;
                        case 0x29:
                            _byEscSeqIndex = 1;
                            break;
                        case 0x2A:
                            _byEscSeqIndex = 2;
                            break;
                        case 0x2B:
                            _byEscSeqIndex = 3;
                            break;
                        default:
                            _byEscSeqCount = 0;
                            return;         // エラー
                    }
                    break;

                // 2バイト目
                case 2:
                    if (DesignationGSET(_byEscSeqIndex, byCode))
                    {
                        _byEscSeqCount = 0;
                        return;
                    }

                    switch (byCode)
                    {
                        case 0x20:
                            _bIsEscSeqDrcs = true;
                            break;
                        case 0x28:
                            _bIsEscSeqDrcs = true;
                            _byEscSeqIndex = 0;
                            break;
                        case 0x29:
                            _bIsEscSeqDrcs = false;
                            _byEscSeqIndex = 1;
                            break;
                        case 0x2A:
                            _bIsEscSeqDrcs = false;
                            _byEscSeqIndex = 2;
                            break;
                        case 0x2B:
                            _bIsEscSeqDrcs = false;
                            _byEscSeqIndex = 3;
                            break;
                        default:
                            _byEscSeqCount = 0;
                            return;         // エラー
                    }
                    break;

                // 3バイト目
                case 3:
                    if (!_bIsEscSeqDrcs)
                    {
                        if (DesignationGSET(_byEscSeqIndex, byCode))
                        {
                            _byEscSeqCount = 0;
                            return;
                        }
                    }
                    else
                    {
                        if (DesignationDRCS(_byEscSeqIndex, byCode))
                        {
                            _byEscSeqCount = 0;
                            return;
                        }
                    }

                    if (byCode == 0x20)
                    {
                        _bIsEscSeqDrcs = true;
                    }
                    else
                    {
                        // エラー
                        _byEscSeqCount = 0;
                        return;
                    }
                    break;

                // 4バイト目
                case 4:
                    DesignationDRCS(_byEscSeqIndex, byCode);
                    _byEscSeqCount = 0;
                    return;
            }

            _byEscSeqCount++;
        }

        private static void LockingShiftGL(int byIndexG)
        {
            // LSx
            _pLockingGL = byIndexG;
        }

        private static void LockingShiftGR(int byIndexG)
        {
            // LSxR
            _pLockingGR = byIndexG;
        }

        private static void SingleShiftGL(int byIndexG)
        {
            // SSx
            _pSingleGL = byIndexG;
        }

        private static bool DesignationGSET(int byIndexG, int byCode)
        {
            // Gのグラフィックセットを割り当てる
            switch (byCode)
            {
                case 0x42:
                    CodeG[byIndexG] = CodeKanji;
                    return true;    // Kanji
                case 0x4A:
                    CodeG[byIndexG] = CodeAlphanumeric;
                    return true;    // Alphanumeric
                case 0x30:
                    CodeG[byIndexG] = CodeHiragana;
                    return true;    // Hiragana
                case 0x31:
                    CodeG[byIndexG] = CodeKatakana;
                    return true;    // Katakana
                case 0x32:
                    CodeG[byIndexG] = CodeMosaicA;
                    return true;    // Mosaic A
                case 0x33:
                    CodeG[byIndexG] = CodeMosaicB;
                    return true;    // Mosaic B
                case 0x34:
                    CodeG[byIndexG] = CodeMosaicC;
                    return true;    // Mosaic C
                case 0x35:
                    CodeG[byIndexG] = CodeMosaicD;
                    return true;    // Mosaic D
                case 0x36:
                    CodeG[byIndexG] = CodePropAlphanumeric;
                    return true;    // Proportional Alphanumeric
                case 0x37:
                    CodeG[byIndexG] = CodePropHiragana;
                    return true;    // Proportional Hiragana
                case 0x38:
                    CodeG[byIndexG] = CodePropKatakana;
                    return true;    // Proportional Katakana
                case 0x49:
                    CodeG[byIndexG] = CodeJisX0201Katakana;
                    return true;    // JIS X 0201 Katakana
                case 0x39:
                    CodeG[byIndexG] = CodeJisKanjiPlane1;
                    return true;    // JIS compatible Kanji Plane 1
                case 0x3A:
                    CodeG[byIndexG] = CodeJisKanjiPlane2;
                    return true;    // JIS compatible Kanji Plane 2
                case 0x3B:
                    CodeG[byIndexG] = CodeAdditionalSymbols;
                    return true;    // Additional symbols
                default:
                    return false;         // 不明なグラフィックセット
            }
        }

        private static bool DesignationDRCS(int byIndexG, int byCode)
        {
            // DRCSのグラフィックセットを割り当てる
            switch (byCode)
            {
                case 0x40:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-0
                case 0x41:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-1
                case 0x42:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-2
                case 0x43:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-3
                case 0x44:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-4
                case 0x45:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-5
                case 0x46:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-6
                case 0x47:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-7
                case 0x48:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-8
                case 0x49:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-9
                case 0x4A:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-10
                case 0x4B:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-11
                case 0x4C:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-12
                case 0x4D:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-13
                case 0x4E:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-14
                case 0x4F:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // DRCS-15
                case 0x70:
                    CodeG[byIndexG] = CodeUnknown;
                    return true;    // Macro
                default:
                    return false;   // 不明なグラフィックセット
            }
        }

    }
}
