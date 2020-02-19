using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfComponent.External.Properties
{
    // minimap2 options
    // https://lh3.github.io/minimap2/minimap2.html

    public class Minimap2Options : BaseOptions
    {
        public static readonly string Pb = "map-pb";
        public static readonly string Ont = "map-ont";

        public static readonly string asm5 = "asm5";
        public static readonly string asm10 = "asm10";
        public static readonly string asm20 = "asm20";
        public static readonly string avapb = "ava-pb";
        public static readonly string avaont = "ava-ont";
        public static readonly string splicehq = "splice:hq";
        public static readonly string sr = "sr";  // Short single-end reads

        public string Preset = string.Empty;
        public bool isMapping = true;
        public string UseCore = string.Empty;

        [System.ComponentModel.DataAnnotations.Required()]
        public string Reference = string.Empty;

        [System.ComponentModel.DataAnnotations.Required()]
        public IEnumerable<string> QueryFastqs = null;

        [System.ComponentModel.DataAnnotations.Required()]
        public string OutFile = string.Empty;


        public string OtherOptions = string.Empty;
    }

}
