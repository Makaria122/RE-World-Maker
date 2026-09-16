#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace REWorldMaker.Editor
{
    internal enum RELanguageMode
    {
        Auto = 0,
        Japanese = 1,
        English = 2,
        Korean = 3,
        Chinese = 4
    }

    [InitializeOnLoad]
    internal static class REWorldMakerLocalization
    {
        private const string PreferenceKey = "REWorldMaker.Language";
        private const string EasySettingsPreferenceKey = "REWorldMaker.EasySettings";
        private const string EasySettingsMenu = "RE World Maker/Easy Settings (簡単設定)";
        private const string AutoMenu = "RE World Maker/Language/Auto (System Language)";
        private const string JapaneseMenu = "RE World Maker/Language/日本語";
        private const string KoreanMenu = "RE World Maker/Language/한국어";
        private const string ChineseMenu = "RE World Maker/Language/中文（简体）";
        private const string EnglishMenu = "RE World Maker/Language/English";

        static REWorldMakerLocalization()
        {
            EditorApplication.delayCall += UpdateMenuChecks;
        }

        private static RELanguageMode EffectiveLanguage
        {
            get
            {
                if (Mode != RELanguageMode.Auto) return Mode;
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Japanese: return RELanguageMode.Japanese;
                    case SystemLanguage.Korean: return RELanguageMode.Korean;
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                    case SystemLanguage.ChineseTraditional: return RELanguageMode.Chinese;
                    default: return RELanguageMode.English;
                }
            }
        }

        internal static RELanguageMode Mode =>
            (RELanguageMode)EditorPrefs.GetInt(PreferenceKey, (int)RELanguageMode.Auto);

        internal static bool EasySettingsEnabled => EditorPrefs.GetBool(EasySettingsPreferenceKey, true);

        internal static string Text(string english, string japanese, string korean, string chinese)
        {
            switch (EffectiveLanguage)
            {
                case RELanguageMode.Japanese: return japanese;
                case RELanguageMode.Korean: return korean;
                case RELanguageMode.Chinese: return chinese;
                default: return english;
            }
        }

        [MenuItem(AutoMenu, false, 100)]
        private static void SelectAuto() => SetMode(RELanguageMode.Auto);

        [MenuItem(JapaneseMenu, false, 101)]
        private static void SelectJapanese() => SetMode(RELanguageMode.Japanese);

        [MenuItem(KoreanMenu, false, 102)]
        private static void SelectKorean() => SetMode(RELanguageMode.Korean);

        [MenuItem(ChineseMenu, false, 103)]
        private static void SelectChinese() => SetMode(RELanguageMode.Chinese);

        [MenuItem(EnglishMenu, false, 104)]
        private static void SelectEnglish() => SetMode(RELanguageMode.English);

        [MenuItem(EasySettingsMenu, false, 20)]
        private static void ToggleEasySettings()
        {
            EditorPrefs.SetBool(EasySettingsPreferenceKey, !EasySettingsEnabled);
            UpdateMenuChecks();
            InternalEditorUtility.RepaintAllViews();
        }

        [MenuItem(EasySettingsMenu, true)]
        private static bool ValidateEasySettings() { UpdateMenuChecks(); return true; }

        [MenuItem(AutoMenu, true)]
        private static bool ValidateAuto() { UpdateMenuChecks(); return true; }

        [MenuItem(JapaneseMenu, true)]
        private static bool ValidateJapanese() { UpdateMenuChecks(); return true; }

        [MenuItem(KoreanMenu, true)]
        private static bool ValidateKorean() { UpdateMenuChecks(); return true; }

        [MenuItem(ChineseMenu, true)]
        private static bool ValidateChinese() { UpdateMenuChecks(); return true; }

        [MenuItem(EnglishMenu, true)]
        private static bool ValidateEnglish() { UpdateMenuChecks(); return true; }

        private static void SetMode(RELanguageMode mode)
        {
            EditorPrefs.SetInt(PreferenceKey, (int)mode);
            UpdateMenuChecks();
            InternalEditorUtility.RepaintAllViews();
        }

        private static void UpdateMenuChecks()
        {
            Menu.SetChecked(AutoMenu, Mode == RELanguageMode.Auto);
            Menu.SetChecked(JapaneseMenu, Mode == RELanguageMode.Japanese);
            Menu.SetChecked(KoreanMenu, Mode == RELanguageMode.Korean);
            Menu.SetChecked(ChineseMenu, Mode == RELanguageMode.Chinese);
            Menu.SetChecked(EnglishMenu, Mode == RELanguageMode.English);
            Menu.SetChecked(EasySettingsMenu, EasySettingsEnabled);
        }
    }
}
#endif
