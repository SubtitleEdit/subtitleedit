using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class Cavena890 : SubtitleFormat
    {

        static List<int> _hebrewCodes = new List<int> {
            0x40, // א
            0x41, // ב
            0x42, // ג
            0x43, // ד
            0x44, // ה
            0x45, // ו
            0x46, // ז
            0x47, // ח
            0x49, // י
            0x4c, // ל
            0x4d, // ם
            0x4e, // מ
            0x4f, // ן
            0x50, // נ
            0x51, // ס
            0x52, // ע
            0x54, // פ
            0x56, // צ
            0x57, // ק
            0x58, // ר
            0x59, // ש
            0x5A, // ת
            0x4b, // כ
            0x4a, // ך
            0x48, // ע
            0x53, // ף
        };

        static List<string> _hebrewLetters = new List<string> {
            "א",
            "ב",
            "ג",
            "ד",
            "ה",
            "ו",
            "ז",
            "ח",
            "י",
            "ל",
            "ם",
            "מ",
            "ן",
            "נ",
            "ס",
            "ע",
            "פ",
            "צ",
            "ק",
            "ר",
            "ש",
            "ת",
            "כ",
            "ך",
            "ע",
            "ף",
        };

        static List<int> _chineseCodes = new List<int> {
            0x5C11, // 少
            0x5973, // 女
            0x5075, // 偵
            0x63A2, // 探
            0x5357, // 南
            0x897F, // 西
            0x5728, // 在
            0x5B78, // 學
            0x751F, // 生
            0x6703, // 會
            0x5DE6, // 左
            0x8F49, // 轉
            0x002C, // ，
            0x8C9D, // 貝
            0x7D72, // 絲
            0x4E0D, // 不
            0x662F, // 是
            0x53F3, // 右
            0x89AA, // 親
            0x611B, // 愛
            0x7684, // 的
            0x908A, // 邊
            0x0020, // space
            0x597D, // 好
            0x770B, // 看
            0x5230, // 到
            0x4E86, // 了
            0x55CE, // 嗎
            0x51F1, // 凱
            0x5229, // 利
            0x9928, // 館
            0x4EC0, // 什
            0x9EBC, // 麼
            0x6211, // 我
            0x6C92, // 沒
            0x8036, // 耶
            0x90A3, // 那
            0x50B3, // 傳
            0x771F, // 真
            0x7D66, // 給
            0x59B3, // 妳
            0x7684, // 的
            0x5730, // 地
            0x5716, // 圖
            0x5462, // 呢
            0x6821, // 校
            0x5712, // 園
            0x7B49, // 等
            0x6BD4, // 比
            0x4E0A, // 上
            0x9762, // 面
            0x6A19, // 標
            0x793A, // 示
            0x65B9, // 方
            0x5411, // 向
            0x9084, // 還
            0x6709, // 有
            0x9810, // 預
            0x8A08, // 計
            0x9054, // 達
            0x6642, // 時
            0x9593, // 間
            0x55AC, // 喬
            0x59EC, // 姬
            0x628A, // 把
            0x5B83, // 它
            0x77E5, // 知
            0x9053, // 道
            0x627E, // 找
            0x5979, // 她
            0x932F, // 錯
            0x627E, // 找
            0x56E0, // 因
            0x70BA, // 為
            0x8FF7, // 迷
            0x8DEF, // 路
            0x800C, // 而
            0x8FF7, // 迷
            0x8DEF, // 路
            0x5C31, // 就
            0x5427, // 吧
            0x7B97, // 算
            0x5BF6, // 寶
            0x99AC, // 馬
            0x5C31, // 就
            0x8AAA, // 說
            0x9019, // 這
            0x4E16, // 世
            0x795E, // 神
            0x8056, // 聖
            0x4E00, // 一
            0x523B, // 刻
            0x8981, // 要
            0x88AB, // 被
            0x4E9B, // 些
            0x53EF, // 可
            0x7537, // 男
            0x6DF9, // 淹
            0x8A18, // 記
            0x5F97, // 得
            0x9AD8, // 高
            0x4E2D, // 中
            0x5E7E, // 幾
            0x5E74, // 年
            0x8001, // 老
            0x62B1, // 抱
            0x6028, // 怨
            0x54EA, // 哪
            0x53BB, // 去
            0x4ED6, // 他
            0x5011, // 們
            0x90FD, // 都
            0x88E1, // 裡
            0x539F, // 原
            0x4F86, // 來
            0x4ED6, // 他
            0x5011, // 們
            0x90FD, // 都
            0x6CB3, // 河
            0x7554, // 畔
            0x5927, // 大
            0x8DB3, // 足
            0x7403, // 球
            0x5B69, // 孩
            0x6E9C, // 溜
            0x51B0, // 冰
            0x5B69, // 孩
            0x751A, // 甚
            0x81F3, // 至
            0x96FB, // 電
            0x8166, // 腦
            0x602A, // 怪
            0x80CE, // 胎
            0x89BA, // 覺
            0x81EA, // 自
            0x5DF1, // 己
            0x50CF, // 像
            0x7375, // 獵
            0x4EBA, // 人
            0x64E0, // 擠
            0x6EFF, // 滿
            0x5E25, // 帥
            0x6C23, // 氣
            0x6027, // 性
            0x7A9F, // 窟
            0x767C, // 發
            0x8A93, // 誓
            0x65E9, // 早
            0x8A71, // 話
            0x524D, // 前
            0x63A5, // 接
            0x500B, // 個
            0x63D2, // 插
            0x64AD, // 播
            0x55E8, // 嗨
            0x2600, // …
            0x54C8, // 哈
            0x56C9, // 囉
            0x5BBF, // 宿
            0x820D, // 舍
            0x64FA, // 擺
            0x5E73, // 平
            0x66C9, // 曉
            0x904E, // 過
            0x5F9E, // 從
            0x62BD, // 抽
            0x6876, // 桶
            0x62BD, // 抽
            0x9846, // 顆
            0x7C64, // 籤
            0x865F, // 號
            0x78BC, // 碼
            0xF800, // ３
            0x3334, // ４
            0x5E78, // 幸
            0x904B, // 運
            0x6578, // 數
            0x5B57, // 字
            0x53CD, // 反
            0x6B63, // 正
            0x6700, // 最
            0x5F8C, // 後
            0x986F, // 顯
            0x8EAB, // 身
            0x624B, // 手
            0x8D85, // 超
            0x731B, // 猛
            0x624D, // 超
            0x505C, // 停
            0x8ECA, // 車
            0x8B49, // 證
            0x624D, // 才
            0x80FD, // 能
            0x505C, // 停
            0x9EBB, // 麻
            0x7169, // 煩
            0x4E0B, // 下
            0x544A, // 告
            0x60E1, // 惡
            0x8CF4, // 賴
            0x3300, // ３
            0x3200, // ２
            0x3757, // ７
            0x8B00, // 型
            0x5600, // Ｖ
            0x3871, // ８
            0xC36C, // 燃
            0xB956, // 油
            0x745C, // 噴
            0x045F, // 射
            0x1564, // 引
            0xCE00, // 擎
            0x3490, // ４
            0x1F63, // 速
            0x926A, // 排
            0x9400, // 檔
            0x5EE0, // 廠
            0x5167, // 內
            0x88DD, // 裝
            0x6562, // 敢
            0x76F8, // 相
            0x4FE1, // 信
            0x7D42, // 終
            0x65BC, // 於
            0x7D42, // 終
            0x65BC, // 於
            0x592A, // 太
            0x68D2, // 棒
            0x554A, // 啊
            0x53EA, // 只
            0x6839, // 根
            0x672C, // 本
            0x60F3, // 想
            0x8DDF, // 跟
            0x540C, // 同
            0x623F, // 房
            0x6A23, // 樣
            0x5B50, // 子
            0x614B, // 態
            0x5EA6, // 度
            0x6A21, // 模
            0x7CCA, // 糊
            0x5F88, // 很
            0x80AF, // 肯
            0x6E05, // 清
            0x695A, // 楚
            0x8868, // 表
            0x59B9, // 妹
            0x6240, // 所
            0x4EE5, // 以
            0x5B9A, // 定
            0x5C0A, // 尊
            0x91CD, // 重
            0x610F, // 意
            0x898B, // 見
            0x6CD5, // 法
            0x7BA1, // 管
            0x4EFB, // 任
            0x4F55, // 何
            0x72C0, // 狀
            0x6CC1, // 況
            0x7D55, // 絕
            0x5C0D, // 對
            0x7576, // 當
            0x5BA4, // 室
            0x53CB, // 友
            0x561B, // 嘛
            0x53C8, // 又
            0x600E, // 怎
            0x5BB6, // 家
            0x7576, // 當
            0x591A, // 多
            0x559C, // 喜
            0x6B61, // 歡
            0x5E36, // 帶
            0x9700, // 需
            0x7136, // 然
            0x5E36, // 帶
            0x8457, // 著
            0x5C0F, // 小
            0x718A, // 熊
            0x8C93, // 貓
            0x8D77, // 起
            0x5566, // 啦
            0x6771, // 東
            0x5BE6, // 實
            0x5947, // 奇
            0x6731, // 朱
            0x723E, // 爾
            0x554F, // 問
            0x8AB0, // 誰
            0x6F22, // 漢
            0x514B, // 克
            0x6F22, // 漢
            0x514B, // 克
            0x62C9, // 拉
            0x66FC, // 曼
            0x76EE, // 目
            0x77AA, // 瞪
            0x53E3, // 口
            0x5446, // 呆
            0x6316, // 挖
            0x51FA, // 出
            0x9A5A, // 驚
            0x63D0, // 提
            0x984C, // 題
            0x5C45, // 居
            0x73FE, // 現
            0x9580, // 門
            0x5920, // 夠
            0x7E7C, // 繼
            0x7E8C, // 續
            0x8D70, // 走
            0x8208, // 興
            0x8A8D, // 認
            0x8B58, // 識
            0x4F60, // 你
            0x665A, // 晚
            0x8B80, // 讀
            0x821E, // 舞
            0x6587, // 文
            0x5F04, // 弄
            0x58A8, // 墨
            0x4F60, // 你
            0x4EFD, // 份
            0x5168, // 全
            0x754C, // 界
            0x7121, // 無
            0x804A, // 聊
            0x520A, // 刊
            0x8FEB, // 迫
            0x5538, // 唸
            0x9A57, // 驗
            0x69AE, // 榮
            0x683C, // 格
            0x9AD4, // 體
            0x80B2, // 育
            0x8AB2, // 課
            0x9234, // 鈴
            0x6728, // 木
            0x7E3D, // 總
            0x8A00, // 言
            0x4E4B, // 之
            0x5929, // 天
            0x4F19, // 伙
            0x4F34, // 伴
            0x652F, // 支
            0x6490, // 撐
            0x9806, // 順
            0x5B8C, // 完
            0x66F8, // 書
            0x61C2, // 懂
            0x8F15, // 輕
            0x5C0B, // 尋
            0x5E79, // 幹
            0x8B1D, // 謝
            0x76F4, // 直
            0x525B, // 剛
            0x7B2C, // 第
            0x5370, // 印
            0x8C61, // 象
            0x7CDF, // 糟
            0x9032, // 進
            0x6B65, // 步
            0x7A7A, // 空
            0x5982, // 如
            0x679C, // 果
            0x5404, // 各
            0x4F4D, // 位
            0x59D0, // 姐
            0x5E6B, // 幫
            0x5FD9, // 忙
            0x8FA6, // 辦
            0x6848, // 案
            0x9001, // 送
            0x62AB, // 披
            0x85A9, // 薩
            0x9858, // 願
            0x9001, // 送
            0x62AB, // 披
            0x85A9, // 薩
            0x4F46, // 但
            0x8CC7, // 資
            0x8A0A, // 訊
            0x6280, // 技
            0x8853, // 術
            0x96A8, // 隨
            0x6548, // 效
            0x52DE, // 勞
            0x5F37, // 強
            0x719F, // 熟
            0x9F0E, // 鼎
            0x540D, // 名
            0x4ECA, // 今
            0x7CBE, // 精
            0x91C7, // 采
            0x65B0, // 新
            0x7761, // 睡
            0x9CE5, // 鳥
            0x7AA9, // 窩
            0x9AE6, // 髦
            0x5957, // 套
            0x5225, // 別
            0x64D4, // 擔
            0x5FC3, // 心
            0x639B, // 掛
            0x79AE, // 禮
            0x7269, // 物
            0x7C43, // 籃
            0x518D, // 再
            0x676F, // 杯
            0x8ABF, // 調
            0x9152, // 酒
            0x8822, // 蠢
            0x9748, // 靈
            0x4F4F, // 住
            0x71C8, // 燈
            0x4E3B, // 主
            0x3486, // ４
            0x5F00, // 號
            0x5FB7, // 德
            0x5DDE, // 州
            0x91CC, // 里
            0x65AF, // 斯
            0x6C40, // 汀
            0x5A1C, // 娜
            0x6613, // 易
            0x838E, // 莎
            0x746A, // 瑪
            0x9E97, // 麗
            0x4E9E, // 亞
            0x4EAD, // 亭
            0x91D1, // 金
            0x65AF, // 斯
            0x59AE, // 妮
            0x8A3B, // 註
            0x518A, // 冊
            0x59AE, // 妮
            0x6D41, // 流
            0x5A9B, // 媛


        };

        static List<string> _chineseLetters = new List<string> {
             "少",
             "女",
             "偵",
             "探",
             "南",
             "西",
             "在",
             "學",
             "生",
             "會",
             "左",
             "轉",
             "，",
             "貝",
             "絲",
             "不",
             "是",
             "右",
             "親",
             "愛",
             "的",
             "邊",
             " ",
             "好",
             "看",
             "到",
             "了",
             "嗎",
             "凱",
             "利",
             "館",
             "什",
             "麼",
             "我",
             "沒",
             "耶",
             "那",
             "傳",
             "真",
             "給",
             "妳",
             "的",
             "地",
             "圖",
             "呢",
             "校",
             "園",
             "等",
             "比",
             "上",
             "面",
             "標",
             "示",
             "方",
             "向",
             "還",
             "有",
             "預",
             "計",
             "達",
             "時",
             "間",
             "喬",
             "姬",
             "把",
             "它",
             "知",
             "道",
             "找",
             "她",
             "錯",
             "找",
             "因",
             "為",
             "迷",
             "路",
             "而",
             "迷",
             "路",
             "就",
             "吧",
             "算",
             "寶",
             "馬",
             "就",
             "說",
             "這",
             "世",
             "神",
             "聖",
             "一",
             "刻",
             "要",
             "被",
             "些",
             "可",
             "男",
             "淹",
             "記",
             "得",
             "高",
             "中",
             "幾",
             "年",
             "老",
             "抱",
             "怨",
             "哪",
             "去",
             "他",
             "們",
             "都",
             "裡",
             "原",
             "來",
             "他",
             "們",
             "都",
             "河",
             "畔",
             "大",
             "足",
             "球",
             "孩",
             "溜",
             "冰",
             "孩",
             "甚",
             "至",
             "電",
             "腦",
             "怪",
             "胎",
             "覺",
             "自",
             "己",
             "像",
             "獵",
             "人",
             "擠",
             "滿",
             "帥",
             "氣",
             "性",
             "窟",
             "發",
             "誓",
             "早",
             "話",
             "前",
             "接",
             "個",
             "插",
             "播",
             "嗨",
             "…",
             "哈",
             "囉",
             "宿",
             "舍",
             "擺",
             "平",
             "曉",
             "過",
             "從",
             "抽",
             "桶",
             "抽",
             "顆",
             "籤",
             "號",
             "碼",
             "３",
             "４",
             "幸",
             "運",
             "數",
             "字",
             "反",
             "正",
             "最",
             "後",
             "顯",
             "身",
             "手",
             "超",
             "猛",
             "超",
             "停",
             "車",
             "證",
             "才",
             "能",
             "停",
             "麻",
             "煩",
             "下",
             "告",
             "惡",
             "賴",
             "３",
             "２",
             "７",
             "型",
             "Ｖ",
             "８",
             "燃",
             "油",
             "噴",
             "射",
             "引",
             "擎",
             "４",
             "速",
             "排",
             "檔",
             "廠",
             "內",
             "裝",
             "敢",
             "相",
             "信",
             "終",
             "於",
             "終",
             "於",
             "太",
             "棒",
             "啊",
             "只",
             "根",
             "本",
             "想",
             "跟",
             "同",
             "房",
             "樣",
             "子",
             "態",
             "度",
             "模",
             "糊",
             "很",
             "肯",
             "清",
             "楚",
             "表",
             "妹",
             "所",
             "以",
             "定",
             "尊",
             "重",
             "意",
             "見",
             "法",
             "管",
             "任",
             "何",
             "狀",
             "況",
             "絕",
             "對",
             "當",
             "室",
             "友",
             "嘛",
             "又",
             "怎",
             "家",
             "當",
             "多",
             "喜",
             "歡",
             "帶",
             "需",
             "然",
             "帶",
             "著",
             "小",
             "熊",
             "貓",
             "起",
             "啦",
             "東",
             "實",
             "奇",
             "朱",
             "爾",
             "問",
             "誰",
             "漢",
             "克",
             "漢",
             "克",
             "拉",
             "曼",
             "目",
             "瞪",
             "口",
             "呆",
             "挖",
             "出",
             "驚",
             "提",
             "題",
             "居",
             "現",
             "門",
             "夠",
             "繼",
             "續",
             "走",
             "興",
             "認",
             "識",
             "你",
             "晚",
             "讀",
             "舞",
             "文",
             "弄",
             "墨",
             "你",
             "份",
             "全",
             "界",
             "無",
             "聊",
             "刊",
             "迫",
             "唸",
             "驗",
             "榮",
             "格",
             "體",
             "育",
             "課",
             "鈴",
             "木",
             "總",
             "言",
             "之",
             "天",
             "伙",
             "伴",
             "支",
             "撐",
             "順",
             "完",
             "書",
             "懂",
             "輕",
             "尋",
             "幹",
             "謝",
             "直",
             "剛",
             "第",
             "印",
             "象",
             "糟",
             "進",
             "步",
             "空",
             "如",
             "果",
             "各",
             "位",
             "姐",
             "幫",
             "忙",
             "辦",
             "案",
             "送",
             "披",
             "薩",
             "願",
             "送",
             "披",
             "薩",
             "但",
             "資",
             "訊",
             "技",
             "術",
             "隨",
             "效",
             "勞",
             "強",
             "熟",
             "鼎",
             "名",
             "今",
             "精",
             "采",
             "新",
             "睡",
             "鳥",
             "窩",
             "髦",
             "套",
             "別",
             "擔",
             "心",
             "掛",
             "禮",
             "物",
             "籃",
             "再",
             "杯",
             "調",
             "酒",
             "蠢",
             "靈",
             "住",
             "燈",
             "主",
             "４",
             "號",
             "德",
             "州",
             "里",
             "斯",
             "汀",
             "娜",
             "易",
             "莎",
             "瑪",
             "麗",
             "亞",
             "亭",
             "金",
             "斯",
             "妮",
             "註",
             "冊",
             "妮",
             "流",
             "媛",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
             "",
        };

        public override string Extension
        {
            get { return ".890"; }
        }

        public override string Name
        {
            get { return "Cavena 890"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return false; }
        }

        private string _language;

        public void Save(string fileName, Subtitle subtitle)
        {
            _language = null;
            if (subtitle.Header != null && subtitle.Header.StartsWith("890-language:"))
                _language = subtitle.Header.Remove(0, "890-language:".Length);

            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            if (_language != null && _language.StartsWith("HEB"))
            {
                byte[] buffer = new byte[388];
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0;

                buffer[32] = 0x3A;
                buffer[33] = 0x30;
                buffer[34] = 0x30;

                buffer[134] = 0x09;
                buffer[135] = 0x07;
                buffer[136] = 0x15;
                buffer[137] = 0x13;
                buffer[138] = 0x56;
                buffer[139] = 0x56;
                buffer[140] = 0x08;
                buffer[141] = 0x90;

                buffer[146] = 0x09;
                buffer[147] = 0x8f;

                buffer[172] = 0x08;
                buffer[173] = 0x90;

                buffer[179] = 0xF6;
                buffer[180] = 0x01;

                buffer[187] = 0x48; // HEBNOA.V
                buffer[188] = 0x45;
                buffer[189] = 0x42;
                buffer[190] = 0x4E;
                buffer[191] = 0x4F;
                buffer[192] = 0x41;
                buffer[193] = 0x2E;
                buffer[194] = 0x56;

                buffer[208] = 0xf6;
                buffer[209] = 0x01;
                buffer[210] = 0xf3;
                buffer[211] = 0x01;
                buffer[213] = 0x03;

                buffer[246] = 0x56;
                buffer[247] = 0x46;
                buffer[248] = 0x4F;
                buffer[249] = 0x4E;
                buffer[250] = 0x54;
                buffer[251] = 0x4C;
                buffer[252] = 0x2E;
                buffer[253] = 0x56;
                buffer[254] = 0x4B;
                buffer[255] = 0x02;
                buffer[256] = 0x30;
                buffer[257] = 0x30;
                buffer[258] = 0x3A;
                buffer[259] = 0x30;
                buffer[260] = 0x30;
                buffer[261] = 0x3A;
                buffer[262] = 0x30;
                buffer[263] = 0x30;
                buffer[264] = 0x3A;
                buffer[265] = 0x30;
                buffer[266] = 0x30;

                fs.Write(buffer, 0, buffer.Length);
            }
            else
            {
                //header
                for (int i = 0; i < 22; i++)
                    fs.WriteByte(0);

                byte[] buffer = ASCIIEncoding.ASCII.GetBytes("Subtitle Edit");
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 18 - buffer.Length; i++)
                    fs.WriteByte(0);

                string title = Path.GetFileNameWithoutExtension(fileName);
                if (title.Length > 25)
                    title = title.Substring(0, 25);
                buffer = ASCIIEncoding.ASCII.GetBytes(title);
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 28 - title.Length; i++)
                    fs.WriteByte(0);

                buffer = ASCIIEncoding.ASCII.GetBytes("NV");
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 66 - buffer.Length; i++)
                    fs.WriteByte(0);

                buffer = new byte[] { 0xA0, 0x05, 0x04, 0x03, 0x06, 0x06, 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, 0x07, 0x07, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, 0x4E, 0xBF, 0x02, 0x45, 0x54, 0x4C, 0x35, 0x30, 0x35, 0x56, 0x46, 0x4F, 0x4E, 0x54, 0x4C, 0x2E, 0x56, 0x44, 0x46, 0x4F, 0x4E, 0x54, 0x4C, 0x2E, 0x44,
                0x01, 0x07, 0x01, 0x08, 0x00, 0xBF, 0x02, 0xBF, 0x02, 0x00, 0x00, 0x0D, 0xBF, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x46, 0x4F, 0x4E, 0x54, 0x4C, 0x2E, 0x56, 0x14, 0x56, 0x31, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x54, 0x44 };
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 92; i++)
                    fs.WriteByte(0);
            }

            // paragraphs
            int number = 16;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // number
                fs.WriteByte((byte)(number / 256));
                fs.WriteByte((byte)(number % 256));

                WriteTime(fs, p.StartTime);
                WriteTime(fs, p.EndTime);

                var buffer = new byte[] { 0x14, 00, 00, 00, 00, 00, 00, 0x16 };
                fs.Write(buffer, 0, buffer.Length);

                WriteText(fs, p.Text, p == subtitle.Paragraphs[subtitle.Paragraphs.Count-1]);

                number += 16;
            }
            fs.Close();
        }

        private void WriteText(FileStream fs, string text, bool isLast)
        {
            string line1 = string.Empty;
            string line2 = string.Empty;
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 2)
                lines = Utilities.AutoBreakLine(text).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
                line1 = lines[0];
            if (lines.Length > 1)
                line2 = lines[1];

            var buffer = GetTextAsBytes(line1);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00, 00, 00 };
            fs.Write(buffer, 0, buffer.Length);

            buffer = GetTextAsBytes(line2);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00 };
            if (!isLast)
                fs.Write(buffer, 0, buffer.Length);
        }

        private byte[] GetTextAsBytes(string text)
        {
            byte[] buffer = new byte[51];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0x7F;

            var encoding = Encoding.Default;
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                string current = text.Substring(i, 1);
                if (_language != null && _language.StartsWith("HEB"))
                {
                    int letterIndex = _hebrewLetters.IndexOf(current);
                    if (letterIndex >= 0)
                        buffer[index] = (byte)_hebrewCodes[letterIndex];
                    else if (i + 3 < text.Length && text.Substring(i, 3) == "<i>")
                        buffer[index] = 0x88;
                    else if (i + 4 < text.Length && text.Substring(i, 4) == "</i>")
                        buffer[index] = 0x98;
                    else
                        buffer[index] = encoding.GetBytes(current)[0];
                    index++;
                }
                else
                {
                    if (index < 50)
                    {
                        if (current == "æ")
                            buffer[index] = 0x1B;
                        else if (current == "ø")
                            buffer[index] = 0x1C;
                        else if (current == "å")
                            buffer[index] = 0x1D;
                        else if (current == "Æ")
                            buffer[index] = 0x5B;
                        else if (current == "Ø")
                            buffer[index] = 0x5C;
                        else if (current == "Å")
                            buffer[index] = 0x5D;
                        else if (current == "Ä")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (current == "ä")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == "Ö")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x4F;
                        }
                        else if (current == "ö")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x6F;
                        }
                        else if (current == "å")
                        {
                            buffer[index] = 0x8C;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == "Å")
                        {
                            buffer[index] = 0x8C;
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (i + 3 < text.Length && text.Substring(i, 3) == "<i>")
                            buffer[index] = 0x88;
                        else if (i + 4 < text.Length && text.Substring(i, 4) == "</i>")
                            buffer[index] = 0x98;
                        else
                            buffer[index] = encoding.GetBytes(current)[0];
                        index++;
                    }
                }
            }

            return buffer;
        }

        private void WriteTime(FileStream fs, TimeCode timeCode)
        {
            double totalMilliseconds = timeCode.TotalMilliseconds; // +TimeSpan.FromHours(10).TotalMilliseconds; // +10 hours
            int frames = (int)Math.Round(totalMilliseconds / (1000.0 /Configuration.Settings.General.CurrentFrameRate));
            fs.WriteByte((byte)(frames / 256 / 256));
            fs.WriteByte((byte)(frames / 256));
            fs.WriteByte((byte)(frames % 256));
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(".890"))
                    {
                        byte[] buffer = File.ReadAllBytes(fileName);
                        if (buffer.Length > 0x185 && buffer[0x185] == 0x10)
                            return true;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            const int TextLength = 51;

            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            _language = GetLanguage(buffer);
            if (_language != null)
                subtitle.Header = "890-language:" + _language;

            int i = 455;
            int lastNumber = -1;
            while (i < buffer.Length - 20)
            {
                int start = i - TextLength;

                int number = buffer[start - 16] * 256 + buffer[start - 15];

                Paragraph p = new Paragraph();
                double startFrame = buffer[start - 14] * 256 * 256 + buffer[start - 13] * 256 + buffer[start - 12];
                double endFrame = buffer[start - 11] * 256 * 256 + buffer[start - 10] * 256 + buffer[start - 9];
                string line1 = FixText(buffer, start, TextLength);
                string line2 = FixText(buffer, start + TextLength + 6, TextLength);
                if (lastNumber == number)
                {
                    p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    string temp = (line1 + Environment.NewLine + line2).Trim();
                    if (temp.Length > 0)
                        p.Text = temp;
                }
                else
                {
                    subtitle.Paragraphs.Add(p);
                    p.StartTime.TotalMilliseconds = ((1000.0 / Configuration.Settings.General.CurrentFrameRate) * startFrame);
                    p.EndTime.TotalMilliseconds = ((1000.0 / Configuration.Settings.General.CurrentFrameRate) * endFrame);
                    p.Text = (line1 + Environment.NewLine + line2).Trim();
                }

                lastNumber = number;

                i+=128;
            }

            subtitle.Renumber(1);
        }

        private string GetLanguage(byte[] buffer)
        {
            if (buffer.Length < 200)
                return null;

            return Encoding.ASCII.GetString(buffer, 187, 6);
        }

        private string FixText(byte[] buffer, int start, int textLength)
        {
            string text;
            if (_language == "HEBNOA")
            {
                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                var sb = new StringBuilder();
                for (int i = 0; i < textLength; i++)
                {
                    int b = buffer[start + i];
                    int idx = _hebrewCodes.IndexOf(b);
                    if (idx >= 0)
                        sb.Append(_hebrewLetters[idx]);
                    else
                        sb.Append(encoding.GetString(buffer, start+i, 1));
                }

                text = sb.ToString();

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

                text = Utilities.FixEnglishTextInRightToLeftLanguage(text, "0123456789abcdefghijklmnopqrstuvwxyz");
            }
            else if (_language == "CCKM44")
            {
                var sb = new StringBuilder();
                int i = 0;
                int index = start;
                while (i < textLength)
                {
                    if (buffer[index] != 0)
                    {
                        sb.Append(GetChineseString(Encoding.UTF8, buffer, ref index));
                    }
                    index++;
                    i = index - start;
                }
                text = sb.ToString();

                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

                text = text.Replace(encoding.GetString(new byte[] { 0x88 }), "<i>");
                text = text.Replace(encoding.GetString(new byte[] { 0x98 }), "</i>");

                if (text.Contains("<i></i>"))
                    text = text.Replace("<i></i>", "<i>");
                if (text.Contains("<i>") && !text.Contains("</i>"))
                    text += "</i>";
            }
            else
            {
                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                text = encoding.GetString(buffer, start, textLength);

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?
                text = text.Replace(encoding.GetString(new byte[] { 0x1B }), "æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x1C }), "ø");
                text = text.Replace(encoding.GetString(new byte[] { 0x1D }), "å");
                text = text.Replace(encoding.GetString(new byte[] { 0x1E }), "Æ");

                text = text.Replace(encoding.GetString(new byte[] { 0x5B }), "Æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x5C }), "Ø");
                text = text.Replace(encoding.GetString(new byte[] { 0x5D }), "Å");


                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x41 }), "Ä");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x61 }), "ä");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x4F }), "Ö");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x6F }), "ö");

                text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x61 }), "å");
                text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x41 }), "Å");

                text = text.Replace(encoding.GetString(new byte[] { 0x88 }), "<i>");
                text = text.Replace(encoding.GetString(new byte[] { 0x98 }), "</i>");

                if (text.Contains("<i></i>"))
                    text = text.Replace("<i></i>", "<i>");
                if (text.Contains("<i>") && !text.Contains("</i>"))
                    text += "</i>";
            }

            return text;
        }

        public static string GetChineseString(Encoding encoding, byte[] buffer, ref int index)
        {
            byte b = buffer[index];
            int idx = _chineseCodes.IndexOf(b);
            if (idx >= 0)
                return _chineseLetters[idx];

            if (buffer.Length > index + 1)
            {
                idx = _chineseCodes.IndexOf(b * 256 + buffer[index + 1]);
                if (idx >= 0)
                {
                    index++;
                    return _chineseLetters[idx];
                }
                else
                {
                    index++;
                    string hx1 = String.Format("{0:X}", buffer[index - 1]);
                    if (hx1.Length == 1)
                        hx1 = "0" + hx1;
                    string hx2 = String.Format("{0:X}", buffer[index]);
                    if (hx2.Length == 1)
                        hx2 = "0" + hx2;
                    return " (0x" + hx1 + hx2 + ") ";

                }
            }

            return encoding.GetString(buffer, index, 1);
        }

    }
}