﻿namespace Irene.Utils;

static partial class Util {
	// Converting strings to/from single-line, easily parseable text.
	public static string Escape(this string str) {
		string text = str;
		foreach (string escape_code in _escapeCodes.Keys) {
			string codepoint = _escapeCodes[escape_code];
			text = text.Replace(codepoint, escape_code);
		}
		return text;
	}
	public static string Unescape(this string str) {
		string text = str;
		foreach (string escape_code in _escapeCodes.Keys) {
			string codepoint = _escapeCodes[escape_code];
			text = text.Replace(escape_code, codepoint);
		}
		return text;
	}
	private static readonly Dictionary<string, string> _escapeCodes = new () {
		{ @"\n"    , "\n"     },
		{ @"\esc"  , "\x1B"   },
		{ @":bbul:", "\u2022" },
		{ @":wbul:", "\u25E6" },
		{ @":emsp:", "\u2003" },
		{ @":ensp:", "\u2022" },
		{ @":nbsp:", "\u00A0" },
		{ @":+-:"  , "\u00B1" },
	};

	// Create a blank file at the given path, if it doesn't exist.
	// Returns true if file was created, false otherwise.
	public static bool CreateIfMissing(string path, ref object @lock) {
		bool did_create = false;
		lock (@lock) {
			if (!File.Exists(path)) {
				File.Create(path).Close();
				did_create = true;
			}
		}
		return did_create;
	}
}