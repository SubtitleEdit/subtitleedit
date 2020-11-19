using System.Collections.Generic;
using System.Text;
using static Nikse.SubtitleEdit.Core.Common.SeJsonParser;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Validate json file - a simple C# json validator.
    /// See http://json.org/
    /// </summary>
    public class SeJsonValidator
    {
        public List<string> Errors { get; private set; } = new List<string>();

        public class StateElement
        {
            public SeJsonState State { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }

        private readonly HashSet<char> _whiteSpace = new HashSet<char> { ' ', '\r', '\n', '\t' };

        public bool ValidateJson(string content)
        {
            Errors = new List<string>();

            int i = 0;
            int max = content.Length;
            var state = new Stack<StateElement>();
            var objectName = string.Empty;
            while (i < max)
            {
                var ch = content[i];
                if (_whiteSpace.Contains(ch)) // ignore white space
                {
                    i++;
                }

                else if (state.Count == 0) // root
                {
                    if (ch == '{')
                    {
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = SeJsonState.Object
                        });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        state.Push(new StateElement
                        {
                            Name = "Root",
                            State = SeJsonState.Array
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} as position {i}");
                        return false;
                    }
                }

                else if (state.Peek().State == SeJsonState.Object) // after '{'
                {
                    if (ch == '"')
                    {
                        i++;
                        int end = content.IndexOf('"', i);
                        objectName = content.Substring(i, end - i).Trim();
                        int colon = content.IndexOf(':', end);
                        if (colon < 0)
                        {
                            Errors.Add($"Fatal - expected char : afterposition {end}");
                            return false;
                        }

                        i += colon - i + 1;
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = SeJsonState.Value
                        });
                    }
                    else if (ch == '}')
                    {
                        i++;
                        state.Pop();
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} as position {i}");
                            return false;
                        }
                    }
                    else if (ch == ']') // next object
                    {
                        i++;
                        if (state.Peek().Count > 0)
                        {
                            state.Pop();
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} as position {i}");
                            return false;
                        }
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} as position {i}");
                        return false;
                    }
                }

                else if (state.Peek().State == SeJsonState.Value) // value - string/ number / object / array / true / false / null + "," + "}"
                {
                    if (ch == '"') // string
                    {
                        i++;
                        var skip = true;
                        int end = 0;
                        var endSeek = i;
                        while (skip)
                        {
                            end = content.IndexOf('"', endSeek);
                            if (end < 0)
                            {
                                Errors.Add($"Fatal - expected char \" after position {endSeek}");
                                return false;
                            }
                            skip = content[end - 1] == '\\';
                            if (skip)
                            {
                                endSeek = end + 1;
                            }
                            if (endSeek >= max)
                            {
                                Errors.Add($"Fatal - expected end tag after position {endSeek}");
                                return false;
                            }
                        }
                        //var objectValue = content.Substring(i, end - i).Trim();
                        i += end - i + 1;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '}') // empty value
                    {
                        i++;
                        var value = state.Pop();
                        if (state.Count > 0)
                        {
                            if (value.State == SeJsonState.Value)
                            {
                                state.Pop();
                            }
                            else
                            {
                                state.Peek().Count++;
                            }
                        }
                    }
                    else if (ch == ',') // next object
                    {
                        i++;
                        state.Pop();
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} as position {i}");
                            return false;
                        }
                    }
                    else if (ch == 'n' && max > i + 3 && content[i + 1] == 'u' && content[i + 2] == 'l' && content[i + 3] == 'l' ||
                             ch == 't' && max > i + 3 && content[i + 1] == 'r' && content[i + 2] == 'u' && content[i + 3] == 'e')
                    {
                        i += 4;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == 'f' && max > i + 4 && content[i + 1] == 'a' && content[i + 2] == 'l' && content[i + 3] == 's' && content[i + 4] == 'e')
                    {
                        i += 5;
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if ("+-0123456789".IndexOf(ch) >= 0)
                    {
                        var sb = new StringBuilder();
                        while ("+-0123456789.Ee".IndexOf(content[i]) >= 0 && i < max)
                        {
                            sb.Append(content[i]);
                            i++;
                        }
                        state.Pop();
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement
                        {
                            State = SeJsonState.Object,
                            Name = objectName
                        });
                        i++;
                    }
                    else if (ch == '[')
                    {
                        if (state.Count > 1)
                        {
                            var value = state.Pop();
                            state.Peek().Count++;
                            state.Push(value);
                        }
                        state.Push(new StateElement
                        {
                            State = SeJsonState.Array,
                            Name = objectName
                        });
                        i++;
                    }
                    else
                    {
                        Errors.Add($"Unexpected char {ch} as position {i}");
                        return false;
                    }
                }

                else if (state.Peek().State == SeJsonState.Array) // array, after '['
                {
                    if (ch == ']')
                    {
                        state.Pop();
                        i++;
                    }
                    else if (ch == ',' && state.Peek().Count > 0)
                    {
                        if (state.Count > 0 && state.Peek().Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        else
                        {
                            Errors.Add($"Unexpected char {ch} as position {i}");
                            return false;
                        }
                        i++;
                    }
                    else if (ch == '{')
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement
                        {
                            Name = objectName,
                            State = SeJsonState.Object
                        });
                        i++;
                    }
                    else
                    {
                        if (state.Count > 0)
                        {
                            state.Peek().Count++;
                        }
                        state.Push(new StateElement
                        {
                            Name = objectName + "_array_value",
                            State = SeJsonState.Value
                        });
                    }
                }

            }
            return Errors.Count == 0;
        }
    }
}