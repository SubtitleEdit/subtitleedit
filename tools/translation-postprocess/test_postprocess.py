import codecs
import json
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from postprocess import DICTIONARIES, DECORATION, load_rules, process


def subrip(cues, first_second=0, newline="\n", *, bom=False):
    def stamp(second, millisecond):
        return f"00:{second // 60:02}:{second % 60:02},{millisecond:03}"

    blocks = [
        f"{i + 1}{newline}{stamp(i + first_second, 0)} --> {stamp(i + first_second, 999)}{newline}{text}"
        for i, text in enumerate(cues)
    ]
    content = (2 * newline).join(blocks) + newline
    return (codecs.BOM_UTF8 if bom else b"") + content.encode("utf-8")


class PostprocessTests(unittest.TestCase):
    def setUp(self):
        directory = TemporaryDirectory()
        self.addCleanup(directory.cleanup)
        root = Path(directory.name)
        self.source = root / "source.srt"
        self.draft = root / "draft.srt"
        self.output = root / "output.srt"

    def test_dictionary_inventory_and_round_trip(self):
        """Every source/target pair can be read and all its exact rules match through SubRip."""
        shipped = Path(__file__).resolve().parents[2] / "Dictionaries"
        locales = {path.stem for path in DICTIONARIES.glob("*.json")}
        layout_languages = {path.name.split("_")[0] for path in shipped.glob("*_NoBreakAfterList.xml")}
        self.assertTrue(layout_languages <= locales)
        for path in sorted(DICTIONARIES.glob("*.json")):
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual(path.stem, data["target_language"])
            for source, entries in data["rules"].items():
                with self.subTest(source=source, target=path.stem):
                    self.assertIn(source, locales)
                    self.assertGreaterEqual(len(entries), 8)
                    for entry in entries:
                        self.assertNotEqual("replacement" in entry, "suggestions" in entry)
                        choices = [entry["replacement"]] if "replacement" in entry else entry["suggestions"]
                        for phrase in [entry["source"], entry["draft"], *choices]:
                            self.assertFalse(DECORATION.search(phrase))
                    rules = load_rules(source, path.stem)
                    self.source.write_bytes(subrip([entry["source"] for entry in entries]))
                    self.draft.write_bytes(subrip([entry["draft"] for entry in entries]))
                    matches = process(self.source, self.draft, rules, self.output)
                    self.assertEqual(len(entries), len(matches))
                    expected = [entry.get("replacement", entry["draft"]) for entry in entries]
                    self.assertEqual(self.output.read_bytes(), subrip(expected))
                    self.output.unlink()

    def test_only_exact_plain_cues_are_edited_and_output_preserves_encoding(self):
        sources = ["Thank you.", "Thank you.", "Thank you.", "I'm sorry.", "I don’t know.", "Thank you!"]
        draft = ["Thank you.", "<i>Thank you.</i>", "- Thank you.", "Det gør mig ondt.", "Det ved jeg ikke", "Thank you."]
        self.source.write_bytes(subrip(sources))
        self.draft.write_bytes(subrip(draft, newline="\r\n", bom=True))
        rules = load_rules("en", "da")
        matches = process(self.source, self.draft, rules)
        self.assertFalse(self.output.exists())
        self.assertEqual([match["cue"] for match in matches], ["1", "4", "5"])
        self.assertEqual(matches[1]["previous_source"], "Thank you.")
        self.assertEqual(matches[1]["next_source"], "I don’t know.")
        process(self.source, self.draft, rules, self.output)
        edited = ["Tak.", *draft[1:4], "Det ved jeg ikke.", draft[5]]
        self.assertEqual(self.output.read_bytes(), subrip(edited, newline="\r\n", bom=True))

    def test_timing_mismatch_fails_before_writing(self):
        self.source.write_bytes(subrip(["Thank you."]))
        self.draft.write_bytes(subrip(["Thank you."], first_second=1))
        with self.assertRaisesRegex(ValueError, "identical cue timestamps"):
            process(self.source, self.draft, load_rules("en", "da"), self.output)
        self.assertFalse(self.output.exists())

    def test_output_must_be_a_new_file(self):
        self.source.write_bytes(subrip(["Thank you."]))
        self.draft.write_bytes(subrip(["Thank you."]))
        with self.assertRaises(FileExistsError):
            process(self.source, self.draft, load_rules("en", "da"), self.draft)


if __name__ == "__main__":
    unittest.main()
