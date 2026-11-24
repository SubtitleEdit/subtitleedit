# Short-cue translation post-processing experiment

This Python 3 tool compares a UTF-8 SubRip source with its translated draft. It previews dictionary matches by default. Give it `--output` to apply eligible replacements in a new draft. It checks that the cue timestamps match in order and preserves cue numbering, timing, line endings, and UTF-8 BOM.

```sh
python3 tools/translation-postprocess/postprocess.py source.srt draft.da.srt --target-language da
python3 tools/translation-postprocess/postprocess.py source.srt draft.da.srt --target-language da --output reviewed.da.srt
```

A rule matches the entire source and draft cue using case and punctuation, with equivalent Unicode composition and curly apostrophes accepted. Cues with inline markup, speaker dashes, music markers, or multiple lines are skipped. A rule with `replacement` is eligible for application. A rule with `suggestions` is preview-only and includes the adjacent source cues when available. This lets a reviewer distinguish an apology, a condolence, and a request for clarification.

The 39 target dictionaries cover the 33 languages with shipped `*_NoBreakAfterList.xml` assets, plus Brazilian and European Portuguese, Japanese, Korean, and Simplified and Traditional Chinese. Each locale starts with eight common phrases. The English target contains reverse echo rules from each other locale. Other targets start with English source rules for exact echoes and selected missing punctuation. A few lists also include specific mistranslations observed or proposed for testing. These are starter lists with one register per seed phrase. Locale coverage means the rules load and apply as intended. It does not imply a native review for every locale.

Auto Translate uses the same dictionary files when **Post-processing dictionary** is enabled. The Python tool can also run separately on an existing SRT. Run the fixture tests with `python3 -m unittest discover -s tools/translation-postprocess`.
