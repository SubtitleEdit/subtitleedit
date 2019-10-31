using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    public class TeletextRunSettings
    {
        public int PageNumber { get; set; }
        public int PageNumberBcd { get; set; }
        public int TransmissionMode { get; set; }
        public HashSet<int> PageNumbersInt { get; set; } = new HashSet<int>();
        public HashSet<int> PageNumbersBcd { get; set; } = new HashSet<int>();
        public Dictionary<int, Paragraph> PageNumberAndParagraph { get; set; } = new Dictionary<int, Paragraph>();

        //private Dictionary<int,Teletext.States> _states = new Dictionary<int, Teletext.States>();
        private Teletext.States _states = new Teletext.States();

        public Teletext.States GetState()
        {
            return _states;
            //if (_states.ContainsKey(PageNumber))
            //{
            //    return _states[PageNumber];
            //}
            //var state = new Teletext.States();
            //_states.Add(PageNumberBcd, state);
            //return state;
        }

        //public bool ContainsPageSub(int sub)
        //{
        //    return PageNumbersBcd.Any(p => Teletext.Page(p) == Teletext.Page(sub));
        //}
        //public bool ContainsMagazine(int magazine)
        //{
        //    return PageNumbersBcd.Any(p => Teletext.M(p) == Teletext.Page(sub));
        //}
    }
}
